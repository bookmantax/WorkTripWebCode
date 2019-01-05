using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worktrip.Models;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.File;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Drawing;

namespace Worktrip.Controllers
{
    [Authorize(Roles ="Admin, Preparer")]
    public class AdminController : Controller
    {

        public JsonResult GetUnboundUsers(int year)
        {
            return Json(UnboundUsers.GetUnboundUsers(year));
        }

        public JsonResult UpdateUsersToPreparers(Dictionary<string, string> userToPreparer, int year)
        {
            var result = UnboundUsers.UpdateUsersToPreparers(userToPreparer, year);

            return Json(new { status = result });
        }

        public JsonResult GetMyUsersActive(int year)
        {
            return Json(UserSmallInfo.GetMyUsers(User.Identity.GetUserId(), true, year));
        }

        public JsonResult GetMyUsersFinished(int year)
        {
            return Json(UserSmallInfo.GetMyUsers(User.Identity.GetUserId(), false, year));
        }

        public JsonResult GetAllUsers(string query, int year)
        {
            return Json(UserSmallInfo.GetAllUsers(query, year));
        }

        public JsonResult GetUserInfo(string userId)
        {
            return Json(UserInfoViewModel.GetUserInfo(userId));
        }

        public JsonResult GetUserActions(int hours)
        {
            return Json(UserAction.GetUserActions(hours));
        }

        public JsonResult UpdateTaxColorCode(string userId, string color, int year)
        {
            using (var db = new WorktripEntities())
            {
                var curUserId = User.Identity.GetUserId();

                var link = db.UserToPreparers.FirstOrDefault(up => up.UserId == userId && up.Year == year);

                if (link == null)
                {
                    link = new UserToPreparer
                    {
                        UserId = userId,
                        Year = year,
                        StatusId = 1
                    };

                    db.UserToPreparers.Add(link);
                }

                link.ColorCode = color;

                db.SaveChanges();
                return Json(new { status = 0 });

            }

            return Json(new { status = -1 });
        }

        public JsonResult UpdateTaxStatus(string userId, string status, int year)
        {
            using (var db = new WorktripEntities())
            {
                var curUserId = User.Identity.GetUserId();

                var link = db.UserToPreparers.FirstOrDefault(up => up.UserId == userId && up.Year == year);

                var statusObj = db.Status.FirstOrDefault(s => s.Name == status);

                if (link != null && statusObj != null)
                {
                    link.StatusId = statusObj.Id;

                    db.SaveChanges();
                    return Json(new { status = 0 });
                }
            }

            return Json(new { status = -1 });
        }

        public JsonResult UpdateTaxPrice(string userId, decimal price, int year)
        {
            using (var db = new WorktripEntities())
            {
                var curUserId = User.Identity.GetUserId();

                var link = db.UserToPreparers.FirstOrDefault(up => up.UserId == userId && up.Year == year);

                if (link == null)
                {
                    link = new UserToPreparer
                    {
                        UserId = userId,
                        Year = year,
                        StatusId = 1
                    };

                    db.UserToPreparers.Add(link);
                }

                link.Fee = price;

                db.SaveChanges();
                return Json(new { status = 0 });
            }

            return Json(new { status = -1 });
        }

        public JsonResult UpdateTaxNotes(string userId, string notes, int year)
        {
            using (var db = new WorktripEntities())
            {
                var info = UserInfoViewModel.GetOrCreateUserInfo(db, userId, year);

                info.PreparerNotes = notes;

                db.SaveChanges();
                return Json(new { status = 0 });
            }

            return Json(new { status = -1 });
        }

        public JsonResult UpdateTaxReturnAmount(string userId, decimal amount, int year)
        {
            using (var db = new WorktripEntities())
            {
                var info = UserInfoViewModel.GetOrCreateUserInfo(db, userId, year);

                info.TaxReturn = amount;

                db.SaveChanges();
                return Json(new { status = 0 });
            }

            return Json(new { status = -1 });
        }

        private string ConvertImageStreamToBase64String(Stream stream)
        {
            using (Image image = Image.FromStream(stream))
            {
                using (MemoryStream imageStream = new MemoryStream())
                {
                    image.Save(imageStream, image.RawFormat);
                    byte[] imageBytes = imageStream.ToArray();

                    string base64String = Convert.ToBase64String(imageBytes);

                    return base64String;
                }
            }
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

        public JsonResult GetUserDocuments(string userId, int taxYear, int? skip, int? amount)
        {
            bool downloadSuccessful = true;
            var results = new List<Tuple<string, string, string>>();

            using (var db = new WorktripEntities())
            {
                try
                {
                    Regex filePattern = new Regex(@"http.*\/.*\/(?<directory>.*)\/(?<filename>.*)");

                    var user = db.Users.FirstOrDefault(u => u.Id == userId);

                    var miscDocs = db.UserMiscDocs.Where(d => d.UserId == userId && d.Year == taxYear);
                    var taxReturn = db.UserTaxReturns.Where(d => d.UserId == userId && d.Year == taxYear);

                    miscDocs = miscDocs.OrderBy(d => d.Id);
                    taxReturn = taxReturn.OrderBy(d => d.Id);

                    if (skip.HasValue)
                    {
                        miscDocs = miscDocs.Skip(skip.Value);
                        taxReturn = taxReturn.Skip(skip.Value);
                    }

                    if (amount.HasValue)
                    {
                        miscDocs = miscDocs.Take(amount.Value);
                        taxReturn = taxReturn.Take(amount.Value);
                    }

                    var fileUrls = miscDocs.Select(d => d.Path).ToList();
                    fileUrls.AddRange(taxReturn.Select(d => d.Path).ToList());

                    var parsedFilePaths = new List<Tuple<string, string>>();

                    foreach (var url in fileUrls)
                    {
                        Match match = filePattern.Match(url);

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

                        if (file.Properties.ContentType != null &&
                            file.Properties.ContentType.StartsWith("image/"))
                        {
                            MemoryStream fileStream = new MemoryStream();

                            file.DownloadToStream(fileStream);

                            fileContents = ConvertImageStreamToBase64String(fileStream);
                        }
                        else if (file.Properties.ContentType.ToLower() == "application/pdf")
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
                catch (Exception e)
                {
                    //Do some error logging here..
                    downloadSuccessful = false;
                }
            }

            if (downloadSuccessful)
            {
                return Json(new {
                    status = 0,
                    files = results
                });
            }
            else
            {
                return Json(new { status = -1, message = "Error in downloading files" });
            }
        }

        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }

    }
}