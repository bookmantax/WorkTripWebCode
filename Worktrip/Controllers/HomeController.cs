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

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}