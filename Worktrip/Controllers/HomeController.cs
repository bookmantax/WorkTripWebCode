using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Worktrip.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace Worktrip.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private const int IMAGE_RESIZE_SIZE = 1200;

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Customer"))
                {
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    return RedirectToAction("Admin");
                }
            }

            return View();
        }

        public ActionResult ResetPassword(string userId, string code)
        {
            ViewBag.action = "resetpw";
            ViewBag.userId = userId;
            ViewBag.code = code;

            return View("Index");
        }

        [Authorize]
        public ActionResult Dashboard()
        {
            var userTaxInfo = UserInfoViewModel.GetUserInfo(User.Identity.GetUserId());

            ViewBag.BraintreeToken = BraintreeUtils.GenerateClientToken();

            ViewBag.Airlines = AirlineSelectModel.GetAirlines();

            ViewBag.IsAdmin = User.Identity.IsAuthenticated && (User.IsInRole("Admin") || User.IsInRole("Preparer"));

            return View(userTaxInfo);
        }

        [Authorize(Roles = "Admin, Preparer")]
        public ActionResult Admin()
        {
            var userTaxInfo = UserInfoViewModel.GetUserInfo(User.Identity.GetUserId());
            ViewBag.Airlines = AirlineSelectModel.GetAirlines();
            ViewBag.Preparers = Preparers.GetPreparers();

            return View(userTaxInfo);
        }

        [Authorize]
        public JsonResult UpdatePersonalInfo(UserInfoViewModel userInfo)
        {
            userInfo.SavePersonalInfo(User.Identity.GetUserId());

            return Json(UserInfoViewModel.GetUserInfo(User.Identity.GetUserId()));
        }

        [Authorize]
        public JsonResult UpdateYesNoQuestions(UserInfoViewModel userInfo)
        {
            userInfo.SaveYesNoQuestions(User.Identity.GetUserId());

            return Json(UserInfoViewModel.GetUserInfo(User.Identity.GetUserId()));
        }

        [Authorize]
        public JsonResult UpdateTaxInfo(UserInfoViewModel userInfo)
        {
            userInfo.SaveTaxInfo(User.Identity.GetUserId());

            return Json(UserInfoViewModel.GetUserInfo(User.Identity.GetUserId()));
        }

        [Authorize]
        public JsonResult GetPersonalInfo()
        {
            return Json(UserInfoViewModel.GetUserInfo(User.Identity.GetUserId()));
        }

        private string ConvertStreamToBase64String(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.Position = 0;
                stream.CopyTo(memoryStream);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        [Authorize]
        public JsonResult GetuserTaxReturn(string userId, int taxYear)
        {
            bool downloadSuccessful = true;
            var results = new List<Tuple<string, string, string>>();

            using (var db = new WorktripEntities())
            {
                try
                {
                    Regex filePattern = new Regex(@"http.*\/.*\/(?<directory>.*)\/(?<filename>.*)");

                    var user = db.Users.FirstOrDefault(u => u.Id == userId);
                    var taxReturn = db.UserTaxReturns.Where(d => d.UserId == userId && d.Year == taxYear);

                    taxReturn = taxReturn.OrderBy(d => d.Id);

                    var fileUrls = new List<UserTaxReturn>();
                    if (taxReturn.Count() != 0)
                    {
                        fileUrls.Add(taxReturn.AsEnumerable().Last());


                        var parsedFilePaths = new List<Tuple<string, string>>();

                        foreach (var url in fileUrls)
                        {
                            Match match = filePattern.Match(url.Path);

                            if (match.Success)
                            {
                                var newTuple = new Tuple<string, string>(
                                    match.Groups["directory"].Value,
                                    match.Groups["filename"].Value
                                );

                                parsedFilePaths.Add(newTuple);
                            }
                        }

                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                            CloudConfigurationManager.GetSetting("StorageConnectionString"));

                        CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

                        CloudFileShare share = fileClient.GetShareReference("worktripdocs");

                        CloudFileDirectory rootDir = share.GetRootDirectoryReference();

                        CloudFileDirectory userDir = null;

                        var userDirName = "";

                        foreach (var parsedPath in parsedFilePaths)
                        {
                            if (userDirName != parsedPath.Item1)
                            {
                                userDir = rootDir.GetDirectoryReference(parsedPath.Item1);

                                if (!userDir.Exists())
                                {
                                    continue;
                                }

                                userDirName = parsedPath.Item1;
                            }

                            var filename = parsedPath.Item2;

                            CloudFile file = userDir.GetFileReference(filename);

                            if (!file.Exists())
                            {
                                continue;
                            }

                            file.FetchAttributes();

                            string fileContents = "";

                            if (file.Properties.ContentType.ToLower() == "application/pdf")
                            {
                                MemoryStream fileStream = new MemoryStream();
                                file.DownloadToStream(fileStream);

                                fileContents = ConvertStreamToBase64String(fileStream);
                            }
                            else
                            {
                                fileContents = file.DownloadText();
                            }

                            results.Add(
                                new Tuple<string, string, string>(filename, file.Properties.ContentType, fileContents)
                            );
                        }
                    }
                }
                catch (Exception e)
                {
                    //Do some error logging here..
                    downloadSuccessful = false;
                }
            }

            if (downloadSuccessful)
            {
                return Json(new
                {
                    status = 0,
                    files = results
                });
            }
            else
            {
                return Json(new { status = -1, message = "Error in downloading files" });
            }

        }

        [Authorize]
        public JsonResult GetSingleUserTaxReturn(string userId, int taxYear)
        {
            bool downloadSuccessful = true;
            var results = new List<Tuple<string, string, byte[]>>();

            using (var db = new WorktripEntities())
            {
                try
                {
                    Regex filePattern = new Regex(@"http.*\/.*\/(?<directory>.*)\/(?<filename>.*)");

                    var user = db.Users.FirstOrDefault(u => u.Id == userId);
                    var taxReturn = db.UserTaxReturns.Where(d => d.UserId == userId && d.Year == taxYear);

                    taxReturn = taxReturn.OrderBy(d => d.Id);

                    var fileUrls = new List<UserTaxReturn>();
                    if (taxReturn.Count() != 0)
                    {
                        fileUrls.Add(taxReturn.AsEnumerable().Last());
                        byte[] bytes = new byte[64000];

                        var parsedFilePaths = new List<Tuple<string, string>>();

                        foreach (var url in fileUrls)
                        {
                            Match match = filePattern.Match(url.Path);

                            if (match.Success)
                            {
                                var newTuple = new Tuple<string, string>(
                                    match.Groups["directory"].Value,
                                    match.Groups["filename"].Value
                                );

                                parsedFilePaths.Add(newTuple);
                            }
                        }

                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                            CloudConfigurationManager.GetSetting("StorageConnectionString"));

                        CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

                        CloudFileShare share = fileClient.GetShareReference("worktripdocs");

                        CloudFileDirectory rootDir = share.GetRootDirectoryReference();

                        CloudFileDirectory userDir = null;

                        var userDirName = "";

                        foreach (var parsedPath in parsedFilePaths)
                        {
                            if (userDirName != parsedPath.Item1)
                            {
                                userDir = rootDir.GetDirectoryReference(parsedPath.Item1);

                                if (!userDir.Exists())
                                {
                                    continue;
                                }

                                userDirName = parsedPath.Item1;
                            }

                            var filename = parsedPath.Item2;

                            CloudFile file = userDir.GetFileReference(filename);

                            if (!file.Exists())
                            {
                                continue;
                            }

                            file.FetchAttributes();

                            string fileContents = "";

                            if (file.Properties.ContentType.ToLower() == "application/pdf")
                            {
                                MemoryStream fileStream = new MemoryStream();
                                file.DownloadToStream(fileStream);
                                bytes = fileStream.ToArray();
                                fileContents = ConvertStreamToBase64String(fileStream);
                            }
                            else
                            {
                                fileContents = file.DownloadText();
                            }

                            results.Add(
                                new Tuple<string, string, byte[]>(filename, file.Properties.ContentType, bytes)
                            );
                        }
                    }
                }
                catch (Exception e)
                {
                    //Do some error logging here..
                    downloadSuccessful = false;
                }
            }

            if (downloadSuccessful && results.Count > 0)
            {
                return Json(new MyJsonResult
                {
                    status = 0,
                    fileName = results.ElementAtOrDefault(0).Item1,
                    fileContentType = results.ElementAtOrDefault(0).Item2,
                    fileContents = results.ElementAtOrDefault(0).Item3
            });
            }
            else
            {
                return Json(new { status = -1, message = "Error in downloading files" });
            }

        }

        public class MyJsonResult
        {
            public int status { get; set; }
            public string fileName { get; set; }
            public string fileContentType { get; set; }
            public byte[] fileContents { get; set; }
        }

        [Authorize]
        public JsonResult SubmitQuestion(string question, int taxYear)
        {
            var result = UserInfoViewModel.SubmitQuestion(question, taxYear, User.Identity.GetUserId());

            return Json(UserInfoViewModel.GetUserInfo(User.Identity.GetUserId()));
        }

        public JsonResult SearchAirports(string term)
        {
            return Json(AirportSelectModel.SearchAirports(term), JsonRequestBehavior.AllowGet);
        }

        private Stream ResizePictureForBandwidth(HttpPostedFileBase image)
        {
            var img = Image.FromStream(image.InputStream);

            if (img.Width < IMAGE_RESIZE_SIZE && img.Height < IMAGE_RESIZE_SIZE)
            {
                image.InputStream.Seek(0, SeekOrigin.Begin);
                return image.InputStream; //No need to resize
            }

            float resizeRatio = (img.Width > img.Height ? (float)img.Width / IMAGE_RESIZE_SIZE : (float)img.Height / IMAGE_RESIZE_SIZE);

            int resizedWidth = (int)(img.Width / resizeRatio);
            int resizedHeight = (int)(img.Height / resizeRatio);

            Bitmap scaledBmp = new Bitmap(resizedWidth, resizedHeight, PixelFormat.Format24bppRgb);
            scaledBmp.SetResolution(img.HorizontalResolution / resizeRatio, img.VerticalResolution / resizeRatio);

            Graphics graphics = Graphics.FromImage(scaledBmp);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            graphics.DrawImage(img, new Rectangle(0, 0, scaledBmp.Width, scaledBmp.Height));

            MemoryStream resizedStream = new MemoryStream();
            scaledBmp.Save(resizedStream, img.RawFormat);
            resizedStream.Seek(0, SeekOrigin.Begin);

            scaledBmp.Dispose();
            img.Dispose();

            return resizedStream;
        }

        [Authorize]
        public JsonResult CalculatePerDiem(int taxYear, List<Layover> layovers, string userId)
        {
            var curUserId = userId == null ? User.Identity.GetUserId() : userId;
            double total = 0;

            foreach (var layover in layovers)
            {
                if (WorktripInit.perDiemRates.ContainsKey(layover.AirportCode))
                {
                    total += WorktripInit.perDiemRates[layover.AirportCode] * layover.Days;
                }
            }

            using (var db = new WorktripEntities())
            {

                var userMiscInfo = UserInfoViewModel.GetOrCreateUserInfo(db, curUserId, taxYear);

                userMiscInfo.LayoversPerDiem = total;

                db.SaveChanges();
            }

            return Json(new { status = 0, total });
        }

        [Authorize]
        public JsonResult UploadUserTaxFile(int taxYear, string subFolder, string userId)
        {
            var curUserId = userId == null ? User.Identity.GetUserId() : userId;

            bool uploadedSuccessfully = false;
            string uploadedURI = "";

            using (var db = new WorktripEntities())
            {
                try
                {
                    var user = db.Users.FirstOrDefault(u => u.Id == curUserId);

                    HttpPostedFileBase uploadedFile = Request.Files.Get(0);

                    var compressedStream = uploadedFile.InputStream;

                    if (uploadedFile.ContentType.StartsWith("image/"))
                    {
                        compressedStream = ResizePictureForBandwidth(uploadedFile);
                    }

                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        CloudConfigurationManager.GetSetting("StorageConnectionString"));

                    CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

                    CloudFileShare share = fileClient.GetShareReference("worktripdocs");

                    CloudFileDirectory rootDir = share.GetRootDirectoryReference();

                    CloudFileDirectory userDir = rootDir.GetDirectoryReference(user.FirstName + " " + user.LastName + " " + user.PhoneNumber);

                    userDir.CreateIfNotExists();

                    if (!string.IsNullOrEmpty(subFolder))
                    {
                        userDir = userDir.GetDirectoryReference(subFolder);

                        userDir.CreateIfNotExists();
                    }

                    var newFileName = uploadedFile.FileName;
                    var fileExtension = Path.GetExtension(newFileName);

                    if (Regex.IsMatch(newFileName, ".*(\\.csv)\\.xlsx?", RegexOptions.IgnoreCase))
                    {
                        //special case for .csv.xls(x) files
                        fileExtension = ".csv";
                        newFileName = Regex.Replace(newFileName, "(\\.csv)?\\.xlsx?", "", RegexOptions.IgnoreCase) + fileExtension;
                    }

                    CloudFile file = userDir.GetFileReference(newFileName);

                    int fileDuplicateCount = 1;

                    while (file.Exists())
                    {
                        //generate a file name that doesn't exist yet
                        newFileName = Path.GetFileNameWithoutExtension(newFileName) + "(" + (fileDuplicateCount++) + ")" + fileExtension;

                        file = userDir.GetFileReference(newFileName); ;
                    }

                    file.Properties.ContentType = uploadedFile.ContentType;

                    file.UploadFromStream(compressedStream);

                    uploadedURI = file.Uri.ToString();

                    UserMiscDoc newDoc = new UserMiscDoc()
                    {
                        UserId = curUserId,
                        Date = DateTime.UtcNow,
                        Path = uploadedURI,
                        Year = taxYear
                    };

                    db.UserMiscDocs.Add(newDoc);

                    db.SaveChanges();

                    uploadedSuccessfully = true;

                    UserInfoViewModel.UpdateUserActionsLog(curUserId, "uploaded " + taxYear + " " + (fileExtension == ".csv" ? "csv file(s)" : "tax form(s)"));
                }
                catch (Exception e)
                {
                    //Do some error logging here..
                    uploadedSuccessfully = false;
                }
            }

            if (uploadedSuccessfully)
            {
                return Json(new { status = 0});
            }
            else
            {
                return Json(new { status = -1, message = "Error in saving file" });
            }
        }

        [Authorize]
        public JsonResult UploadUserTaxReturn(int taxYear, string subFolder, string userId)
        {
            var curUserId = userId == null ? User.Identity.GetUserId() : userId;

            bool uploadedSuccessfully = false;
            string uploadedURI = "";

            using (var db = new WorktripEntities())
            {
                try
                {
                    var user = db.Users.FirstOrDefault(u => u.Id == curUserId);

                    HttpPostedFileBase uploadedFile = Request.Files.Get(0);

                    var compressedStream = uploadedFile.InputStream;

                    if (uploadedFile.ContentType.StartsWith("image/"))
                    {
                        compressedStream = ResizePictureForBandwidth(uploadedFile);
                    }

                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                        CloudConfigurationManager.GetSetting("StorageConnectionString"));

                    CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

                    CloudFileShare share = fileClient.GetShareReference("worktripdocs");

                    CloudFileDirectory rootDir = share.GetRootDirectoryReference();

                    CloudFileDirectory userDir = rootDir.GetDirectoryReference(user.FirstName + " " + user.LastName + " " + user.PhoneNumber);

                    userDir.CreateIfNotExists();

                    if (!string.IsNullOrEmpty(subFolder))
                    {
                        userDir = userDir.GetDirectoryReference(subFolder);

                        userDir.CreateIfNotExists();
                    }

                    var newFileName = uploadedFile.FileName;
                    var fileExtension = Path.GetExtension(newFileName);

                    CloudFile file = userDir.GetFileReference(newFileName);

                    int fileDuplicateCount = 1;

                    while (file.Exists())
                    {
                        //generate a file name that doesn't exist yet
                        newFileName = Path.GetFileNameWithoutExtension(newFileName) + "(" + (fileDuplicateCount++) + ")" + fileExtension;

                        file = userDir.GetFileReference(newFileName); ;
                    }

                    file.Properties.ContentType = uploadedFile.ContentType;

                    file.UploadFromStream(compressedStream);

                    uploadedURI = file.Uri.ToString();

                    UserTaxReturn newReturn = new UserTaxReturn()
                    {
                        UserId = curUserId,
                        Date = DateTime.UtcNow,
                        Path = uploadedURI,
                        Year = taxYear
                    };

                    db.UserTaxReturns.Add(newReturn);

                    db.SaveChanges();

                    uploadedSuccessfully = true;

                    UserInfoViewModel.UpdateUserActionsLog(curUserId, "uploaded " + taxYear + " " + (fileExtension == ".pdf" ? "tax return" : "tax form(s)"));
                }
                catch (Exception e)
                {
                    //Do some error logging here..
                    uploadedSuccessfully = false;
                }
            }

            if (uploadedSuccessfully)
            {
                return Json(new { status = 0 });
            }
            else
            {
                return Json(new { status = -1, message = "Error in saving file" });
            }
        }

        [Authorize]
        public JsonResult MakePayment(string paymentNonce, int transactionYear)
        {
            var chargeResult = BraintreeUtils.ChargePayment(User.Identity.GetUserId(), paymentNonce, transactionYear);

            return Json(new {status = chargeResult ? 0 : -1 });
        }

        [Authorize]
        public JsonResult UpdateFirstTimeLogin()
        {
            var result = UserInfoViewModel.UpdateFirstTimeLogin(User.Identity.GetUserId(), false);

            return Json(new {status = result ? 0 : -1});
        }

        public ActionResult OldIndex()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Privacy()
        {
            ViewBag.Message = "Your Privacy policy page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}