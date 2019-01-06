using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Worktrip.Models
{
    public class AdminModel
    {
    }


    public class Preparers
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CustomersAssigned { get; set; }

        public static List<Preparers> GetPreparers()
        {
            using (var db = new WorktripEntities())
            {
                //var results = db.Users.Include(u => u.Roles).Where(u => u.Roles.Any(r => r.Name == "Preparer")).
                //    Select();

                var results =
                    from u in db.Users.Include(u => u.Roles)
                    join up in db.UserToPreparers on u.Id equals up.PreparerId into leftJoin
                    from link in leftJoin.DefaultIfEmpty()
                        //join s in db.Status on link.StatusId equals s.Id
                    where u.Roles.Any(r => r.Name == "Preparer")
                    group new { u, link } by u.Id into upGroup
                    select new Preparers
                    {
                        Id = upGroup.FirstOrDefault().u.Id,
                        FirstName = upGroup.FirstOrDefault().u.FirstName,
                        LastName = upGroup.FirstOrDefault().u.LastName,
                        CustomersAssigned = upGroup.Count(g => g.link == null ? false : (g.link.Status.Name != "Finished" && g.link.Status.Name != "WaitingCustomerPayment"))
                    };

                return results.ToList();

            }
        }
    }

    public class UnboundUsers
    {
        public string Id { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? SignUpTime { get; set; }
        public string SignUpTimeStr { get; set; }

        public static List<UnboundUsers> GetUnboundUsers(int year)
        {
            using (var db = new WorktripEntities())
            {
                // Only care about users who had previous infoes filled out, and has current year info filled out, but not assigned yet
                var results =
                    from u in db.Users
                    join up in db.UserToPreparers on u.Id equals up.UserId into upGroup
                    from g in upGroup.DefaultIfEmpty()
                    where u.Roles.Any(r => r.Name == "Customer") && (g.Year == year && (g == null || g.PreparerId == null))
                    orderby u.SignUpTime
                    select u;

                return results.ToList().Select(u => new UnboundUsers
                {
                    Id = u.Id,
                    PhoneNumber = u.PhoneNumber,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    SignUpTimeStr = u.SignUpTime.HasValue ? DateTime.SpecifyKind(u.SignUpTime.Value, DateTimeKind.Utc).ToString("s") + "Z" : ""
                }).ToList();
            }
        }

        public static bool UpdateUsersToPreparers(Dictionary<string, string> userToPreparer, int year)
        {
            using (var db = new WorktripEntities())
            {
                foreach (var kvPair in userToPreparer)
                {
                    var link = db.UserToPreparers.FirstOrDefault(up => up.UserId == kvPair.Key && up.Year == year);


                    if (link == null)
                    {
                        var checkUserFields = UserInfoViewModel.CheckUserFields(kvPair.Key);

                        link = new UserToPreparer
                        {
                            UserId = kvPair.Key,
                            Year = year,
                            StatusId = checkUserFields ? 3 : 2
                        };

                        db.UserToPreparers.Add(link);
                    }

                    link.PreparerId = kvPair.Value;

                    db.SaveChanges();
                }

                return true;

            }
        }
    }

    public class UserSmallInfo
    {
        public string Id { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DOB { get; set; }
        public string Status { get; set; }
        public decimal? Price { get; set; }
        public string PreparerId { get; set; }
        public string Preparer { get; set; }
        public string Color { get; set; }
        public DateTime? FirstModified { get; set; }
        public string FirstModifiedString { get; set; }

        public static List<UserSmallInfo> GetMyUsers(string preparerId, bool getActive, int year)
        {
            using (var db = new WorktripEntities())
            {
                var links =
                    db.UserToPreparers
                        .Include(up => up.Status)
                        .Where(up => up.Year == year && up.PreparerId == preparerId && (getActive ? up.Status.Name != "Finished" : up.Status.Name == "Finished"))
                        .ToDictionary(up => up.UserId, up => up);

                var customerIds = links.Select(up => up.Key).ToArray();

                var customers = db.Users.Where(u => customerIds.Contains(u.Id)).ToList().Select(u => new UserSmallInfo
                {
                    Id = u.Id,
                    PhoneNumber = u.PhoneNumber,
                    FirstName = u.FirstName,
                    MiddleName = u.MiddleName,
                    LastName = u.LastName,
                    Email = u.Email,
                    DOB = u.DOB.HasValue ? u.DOB.Value.ToString("yyyy-MM-dd") : null,
                    Status = links[u.Id].Status.Name,
                    Price = links[u.Id].Fee,
                    Color = links[u.Id].ColorCode,
                    FirstModifiedString = links[u.Id].FirstModified.HasValue ? DateTime.SpecifyKind(links[u.Id].FirstModified.Value, DateTimeKind.Utc).ToString("s") + "Z" : ""
                });

                var customersSorted = customers.OrderByDescending(c => c.Color).ThenBy(b => b.FirstModified);

                return customersSorted.ToList();
            }
        }

        public static List<UserSmallInfo> GetAllUsers(string query, int year)
        {
            using (var db = new WorktripEntities())
            {
                var results =
                    from u in db.Users
                    join up in db.UserToPreparers on u.Id equals up.UserId into upJoined
                    from upj in upJoined.DefaultIfEmpty()
                    join s in db.Status on upj.StatusId equals s.Id into sJoined
                    from sj in sJoined.DefaultIfEmpty()
                    where upj.Year == year && u.Roles.Any(r => r.Name == "Customer")
                    orderby upj.ColorCode descending
                    select new UserSmallInfo
                    {
                        Id = u.Id,
                        PhoneNumber = u.PhoneNumber,
                        FirstName = u.FirstName,
                        MiddleName = u.MiddleName,
                        LastName = u.LastName,
                        Email = u.Email,
                        Status = sj.Name == null ? "Waiting for Assignment to Preparer" : sj.Name,
                        Price = upj.Fee,
                        PreparerId = upj.PreparerId,
                        Color = upj.ColorCode,
                        FirstModified = upj.FirstModified.HasValue ? upj.FirstModified : null,
                        FirstModifiedString = ""
                    };

                var resultsList = results.ToList();

                foreach( var r in resultsList)
                {
                    if(r.FirstModified != null)
                    {
                        r.FirstModifiedString = DateTime.SpecifyKind(r.FirstModified.Value, DateTimeKind.Utc).ToString("s") + "Z";
                    }
                }

                var resultsSorted = resultsList.OrderByDescending(c => c.Color).ThenByDescending(b => b.FirstModified.HasValue).ThenBy(a => a.FirstModified);


                if (!String.IsNullOrEmpty(query))
                {
                    resultsSorted = resultsSorted.Where(u => 
                        ((      u.FirstName + " " + u.LastName).StartsWith(query) ||
                                u.LastName.StartsWith(query) ||
                                (u.FirstName + " " + u.MiddleName + " " + u.LastName).StartsWith(query) ||
                                u.Email.StartsWith(query)
                        )).OrderBy(l => l.LastName);
                }

                var preparers = Preparers.GetPreparers().ToDictionary(p => p.Id, p => p);

                var retrieved = resultsSorted.ToList();

                foreach (var r in retrieved)
                {
                    if (!String.IsNullOrEmpty(r.PreparerId) && preparers.ContainsKey(r.PreparerId))
                    {
                        r.Preparer = preparers[r.PreparerId].FirstName + " " + preparers[r.PreparerId].LastName;
                    }
                }

                return retrieved;
            }
        }
    }

    public class UserAction : UserSmallInfo
    {
        public string Action { get; set; }
        public string TimeStr { get; set; }

        public static List<UserAction> GetUserActions(int hours)
        {
            using (var db = new WorktripEntities())
            {
                var utcNow = DateTime.UtcNow;

                var actions = db.UserActionsLogs.Where(a => DbFunctions.DiffHours(a.Timestamp.Value, utcNow) <= hours).ToList();

                var userList = actions.Select(a => a.UserId).ToList();

                var userInfoDict =
                    (from u in db.Users
                     where userList.Contains(u.Id)
                     join up in db.UserToPreparers on u.Id equals up.UserId into upJoined
                     from upj in upJoined.DefaultIfEmpty()
                     join s in db.Status on upj.StatusId equals s.Id into sJoined
                     from sj in sJoined.DefaultIfEmpty()
                     group new { u, upj, sj } by upj.UserId into g
                     select new UserSmallInfo
                     {
                         Id = g.FirstOrDefault().u.Id,
                         PhoneNumber = g.FirstOrDefault().u.PhoneNumber,
                         FirstName = g.FirstOrDefault().u.FirstName,
                         MiddleName = g.FirstOrDefault().u.MiddleName,
                         LastName = g.FirstOrDefault().u.LastName,
                         Email = g.FirstOrDefault().u.Email,
                         Status = g.FirstOrDefault().sj.Name == null ? "Waiting for Assignment to Preparer" : g.FirstOrDefault().sj.Name,
                         Price = g.FirstOrDefault().upj.Fee,
                         PreparerId = g.FirstOrDefault().upj.PreparerId,
                         Color = g.FirstOrDefault().upj.ColorCode
                     }).ToDictionary(u => u.Id, u => u);

                var result =
                    from a in actions
                    select new UserAction
                    {
                        Id = userInfoDict[a.UserId].Id,
                        PhoneNumber = userInfoDict[a.UserId].PhoneNumber,
                        FirstName = userInfoDict[a.UserId].FirstName,
                        MiddleName = userInfoDict[a.UserId].MiddleName,
                        LastName = userInfoDict[a.UserId].LastName,
                        Email = userInfoDict[a.UserId].Email,
                        Status = userInfoDict[a.UserId].Status,
                        Price = userInfoDict[a.UserId].Price,
                        PreparerId = userInfoDict[a.UserId].PreparerId,
                        Color = userInfoDict[a.UserId].Color,
                        Action = a.Action,
                        TimeStr = DateTime.SpecifyKind(a.Timestamp.Value, DateTimeKind.Utc).ToString("s") + "Z"
                    };

                return result.ToList();
            }
        }
    }
}