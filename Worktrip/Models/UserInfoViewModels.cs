using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using Twilio;
using Microsoft.AspNet.Identity;

namespace Worktrip.Models
{
    public class UserInfoViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DOB { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public bool FirstTimeLogin { get; set; }
        public string BankName { get; set; }
        public string RoutingNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public Dictionary<string, TaxInfo> TaxInfos { get; set; }
        public Dictionary<string, List<Question>> Questions { get; set; }
        public Dictionary<string, TaxStatus> Statuses { get; set; }

        public bool SavePersonalInfo(string userId)
        {
            using (var db = new WorktripEntities())
            {
                var userInfo = db.Users.FirstOrDefault(u => u.Id == userId);

                if (userInfo == null)
                {
                    return false;
                }

                userInfo.FirstName = this.FirstName;
                userInfo.MiddleName = this.MiddleName;
                userInfo.LastName = this.LastName;
                userInfo.Email = this.Email;
                userInfo.PhoneNumber = this.PhoneNumber;
                userInfo.DOB = String.IsNullOrEmpty(this.DOB) ? (DateTime?)null : DateTime.Parse(this.DOB);
                userInfo.Address = this.Address;
                userInfo.City = this.City;
                userInfo.State = this.State;
                userInfo.Zip = this.Zip;
                userInfo.BankName = this.BankName;
                userInfo.BankAccountNumber = this.BankAccountNumber;
                userInfo.RoutingNumber = this.RoutingNumber;

                if (this.TaxInfos != null)
                {
                    foreach (var kvPair in this.TaxInfos)
                    {
                        var taxInfo = UserInfoViewModel.GetOrCreateUserInfo(db, userId, kvPair.Value.Year);

                        taxInfo.BaseAirportCode = kvPair.Value.BaseAirportCode?.ToUpper();
                        taxInfo.Airline = kvPair.Value?.Airline;
                    }
                }

                db.SaveChanges();

                UpdateUserActionsLog(userId, "updated personal info");
            }

            return true;
        }

        public bool SaveYesNoQuestions(string userId)
        {
            using (var db = new WorktripEntities())
            {
                var userInfo = db.Users.FirstOrDefault(u => u.Id == userId);

                if (userInfo == null)
                {
                    return false;
                }

                if (this.TaxInfos != null)
                {
                    foreach (var kvPair in this.TaxInfos)
                    {
                        var taxInfo = UserInfoViewModel.GetOrCreateUserInfo(db, userId, kvPair.Value.Year);

                        var u = kvPair.Value;

                        taxInfo.Married = u.Married;
                        taxInfo.Dependent = u.Dependent;
                        taxInfo.StudentLoans = u.StudentLoans;
                        taxInfo.Stocks = u.Stocks;
                        taxInfo.House = u.House;
                        taxInfo.HSA = u.HSA;
                        taxInfo.C1098T = u.C1098T;
                        taxInfo.C1099R = u.C1099R;
                        taxInfo.NewHire = u.NewHire;
                        taxInfo.MultipleW2s = u.MultipleW2s;
                        taxInfo.C1099INT = u.C1099INT;
                        taxInfo.C1099G = u.C1099G;
                        taxInfo.ScheduleK1 = u.ScheduleK1;
                        taxInfo.OwnBusiness = u.OwnBusiness;
                        taxInfo.Other = u.Other;
                        taxInfo.Itemize = u.Itemize;
                        taxInfo.DriveToWork = u.DriveToWork;
                        taxInfo.FlyReserveDays= u.FlyReserveDays;
                    }
                }

                db.SaveChanges();

                UpdateUserActionsLog(userId, "updated yes/no questions");
            }
            return true;
        }

        public bool SaveTaxInfo(string userId)
        {
            using (var db = new WorktripEntities())
            {
                var userInfo = db.Users.FirstOrDefault(u => u.Id == userId);

                if (userInfo == null)
                {
                    return false;
                }

                if (this.TaxInfos != null)
                {
                    foreach (var kvPair in this.TaxInfos)
                    {
                        var taxInfo = UserInfoViewModel.GetOrCreateUserInfo(db, userId, kvPair.Value.Year);

                        var u = kvPair.Value;

                        taxInfo.CellphoneBill = u.CellphoneBill;
                        taxInfo.ClothingFees = u.ClothingFees;
                        taxInfo.DaysInTrainingOrAway = u.DaysInTrainingOrAway;
                        taxInfo.DriverLayoverTips = u.DriverLayoverTips;
                        taxInfo.FlightGearLuggageFees = u.FlightGearLuggageFees;
                        taxInfo.IdentityFees = u.IdentityFees;
                        taxInfo.InternetBill = u.InternetBill;
                        taxInfo.LaundryFees = u.LaundryFees;
                        taxInfo.TechPurchasesFees = u.TechPurchasesFees;
                        taxInfo.TotalSpentLayoverTransportation = u.TotalSpentLayoverTransportation;
                        taxInfo.TransactionalFees = u.TransactionalFees;
                        taxInfo.UnreimbursedExpenses = u.UnreimbursedExpenses;
                        taxInfo.DLState = u.DLState;
                        taxInfo.InternationalLayovers = u.InternationalLayovers;
                    }
                }

                db.SaveChanges();

                UpdateUserActionsLog(userId, "answered " + this.TaxInfos.First().Value.Year + " tax questions");
            }

            return true;
        }

        public static UserMiscTaxInfo GetOrCreateUserInfo(WorktripEntities dbContext, string userId, int taxYear, string promoCode = null, bool notifyAdminsIfExistingCustomer = true)
        {
            var allTaxInfos = dbContext.UserMiscTaxInfoes.Where(i => i.UserId == userId).ToList();

            var taxInfo = allTaxInfos.FirstOrDefault(i => i.TaxYear == taxYear);
            bool createdNewTaxInfo = false;

            if (taxInfo == null)//New Signup
            {
                taxInfo = new UserMiscTaxInfo()
                {
                    UserId = userId,
                    TaxYear = taxYear
                };

                dbContext.UserMiscTaxInfoes.Add(taxInfo);

                dbContext.SaveChanges();

                createdNewTaxInfo = true;
            }

            var upLink = dbContext.UserToPreparers.FirstOrDefault(up => up.Year == taxYear && up.UserId == userId);
            var newUser = dbContext.UserToPreparers.FirstOrDefault(up => up.UserId == userId);

            if (upLink == null)
            {
                upLink = new UserToPreparer()
                {
                    UserId = userId,
                    StatusId = 1,
                    Year = taxYear,
                    PromoCode = promoCode 
                };
                if(newUser == null)
                {
                    upLink.ColorCode = "#fc58a1"; //New Signup
                }
                else
                {
                    upLink.ColorCode = "#58c4fc"; //Existing Signup
                }

                dbContext.UserToPreparers.Add(upLink);
                dbContext.SaveChanges();
            }

            // Notify admins if this is an existing user (from previous year), and they have not been assigned yet for this year
            if (notifyAdminsIfExistingCustomer && allTaxInfos.Count > 0 && createdNewTaxInfo && upLink.PreparerId == null)
            {
                var smsService = new SmsService();

                var preparers = dbContext.Users.Include(u => u.Roles).Where(u => u.Roles.Any(r => r.Name == "Preparer")).ToList();

                var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);

                foreach (var p in preparers)
                {
                    smsService.SendAsync(new Microsoft.AspNet.Identity.IdentityMessage
                    {
                        Destination = p.PhoneNumber,
                        Body = String.Format("Existing customer {0} ({1}) has began filling out their {2} tax info", user.FirstName + " " + user.LastName, user.PhoneNumber, taxYear)
                    });
                }
            }

            return taxInfo;
        }

        public static bool SubmitQuestion(string question, int taxYear, string userId)
        {
            using (var db = new WorktripEntities())
            {
                var userInfo = db.Users.FirstOrDefault(u => u.Id == userId);

                if (userInfo == null)
                {
                    return false;
                }

                var newQuestion = new UserQuestion()
                {
                    AskedBy = userId,
                    Question = question,
                    TaxYear = taxYear,
                    Date = DateTime.UtcNow
                };

                db.UserQuestions.Add(newQuestion);

                //try to text the tax preparer this question
                var userToPreparer = db.UserToPreparers.Where(u => u.UserId == userId && u.Year == taxYear).FirstOrDefault();

                var smsService = new SmsService();

                var textMessage =
                    string.Format("{0} asked a question for their {1} taxes: {2}",
                        userInfo.FirstName + " " + userInfo.LastName + ", " + userInfo.PhoneNumber,
                        taxYear,
                        question);

                if (userToPreparer != null)
                {
                    var preparerInfo = db.Users.Where(u => u.Id == userToPreparer.PreparerId).FirstOrDefault();

                    if (preparerInfo != null)
                    {
                        smsService.SendAsync(new Microsoft.AspNet.Identity.IdentityMessage
                        {
                            Destination = preparerInfo.PhoneNumber,
                            Body = textMessage
                        });
                    }
                }
                else
                {
                    //if this user hasn't been assigned to any preparers yet, text all preparers

                    var preparers = db.Users.Include(u => u.Roles).Where(u => u.Roles.Any(r => r.Name == "Preparer")).ToList();

                    foreach (var p in preparers)
                    {
                        smsService.Send(new IdentityMessage
                        {
                            Destination = p.PhoneNumber,
                            Body = textMessage
                        });
                    }
                }

                db.SaveChanges();
            }

            return true;
        }

        public static UserInfoViewModel GetUserInfo(string userId)
        {
            using (var db = new WorktripEntities())
            {
                var userInfo = db.Users.FirstOrDefault(u => u.Id == userId);

                if (userInfo == null)
                {
                    return null;
                }

                return new UserInfoViewModel()
                {
                    Id = userInfo.Id,
                    FirstName = userInfo.FirstName,
                    MiddleName = userInfo.MiddleName,
                    LastName = userInfo.LastName,
                    Email = userInfo.Email,
                    PhoneNumber = userInfo.PhoneNumber,
                    DOB = userInfo.DOB.HasValue ? userInfo.DOB.Value.ToString("yyyy-MM-dd") : null,
                    Address = userInfo.Address,
                    City = userInfo.City,
                    State = userInfo.State,
                    Zip = userInfo.Zip,
                    FirstTimeLogin = userInfo.FirstTimeLogin,
                    BankName = userInfo.BankName,
                    BankAccountNumber = userInfo.BankAccountNumber,
                    RoutingNumber = userInfo.RoutingNumber,
                    TaxInfos = db.UserMiscTaxInfoes.Where(d => d.UserId == userId).Select(d => new TaxInfo
                    {
                        Year = d.TaxYear,
                        BaseAirportCode = d.BaseAirportCode,
                        DaysInTrainingOrAway = d.DaysInTrainingOrAway,
                        DriverLayoverTips = d.DriverLayoverTips,
                        TotalSpentLayoverTransportation = d.TotalSpentLayoverTransportation,
                        TransactionalFees = d.TransactionalFees,
                        ClothingFees = d.ClothingFees,
                        LaundryFees = d.LaundryFees,
                        FlightGearLuggageFees = d.FlightGearLuggageFees,
                        TechPurchasesFees = d.TechPurchasesFees,
                        CellphoneBill = d.CellphoneBill,
                        InternetBill = d.InternetBill,
                        IdentityFees = d.IdentityFees,
                        UnreimbursedExpenses = d.UnreimbursedExpenses,
                        Airline = d.Airline,
                        PreparerNotes = d.PreparerNotes,
                        TaxReturn = d.TaxReturn,
                        PerDiemsTotal = d.LayoversPerDiem,
                        DLState = d.DLState,
                        InternationalLayovers = d.InternationalLayovers,
                        Married = d.Married,
                        Dependent = d.Dependent,
                        StudentLoans = d.StudentLoans,
                        Stocks = d.Stocks,
                        House = d.House,
                        HSA = d.HSA,
                        C1098T = d.C1098T,
                        C1099R = d.C1099R,
                        NewHire = d.NewHire,
                        MultipleW2s = d.MultipleW2s,
                        C1099INT = d.C1099INT,
                        C1099G = d.C1099G,
                        ScheduleK1 = d.ScheduleK1,
                        OwnBusiness = d.OwnBusiness,
                        Other = d.Other,
                        Itemize = d.Itemize,
                        DriveToWork = d.DriveToWork,
                        FlyReserveDays = d.FlyReserveDays
                    }).ToDictionary(t => t.Year.ToString(), t => t),
                    Questions = db.UserQuestions.Include(q => q.User1).Where(q => q.AskedBy == userId).Select(q => new Question
                    {
                        TaxYear = q.TaxYear,
                        AnswerText = q.Answer,
                        AnsweredBy = q.User1.FirstName,
                        Date = q.Date,
                        QuestionText = q.Question
                    }).GroupBy(q => q.TaxYear).ToDictionary(q => q.First().TaxYear.ToString(), q => q.ToList()),
                    Statuses = db.UserToPreparers.Include(up => up.Status).Where(up => up.UserId == userId).Select(up => new TaxStatus
                    {
                        Year = up.Year,
                        PreparerId = up.PreparerId,
                        Status = up.Status.Name,
                        Fee = up.Fee,
                        PromoCode = up.PromoCode
                    }).ToDictionary(up => up.Year.ToString(), up => up)
                };
            }
        }

        public static bool CheckUserFields(string userId)
        {
            using (var db = new WorktripEntities())
            {
                var userInfo = db.Users.FirstOrDefault(u => u.Id == userId);

                if (userInfo == null)
                {
                    return false;
                }

                return !String.IsNullOrEmpty(userInfo.Address) &&
                        !String.IsNullOrEmpty(userInfo.State) &&
                        !String.IsNullOrEmpty(userInfo.City) &&
                        !String.IsNullOrEmpty(userInfo.Zip) &&
                       !String.IsNullOrEmpty(userInfo.Email) &&
                       !String.IsNullOrEmpty(userInfo.FirstName) &&
                       !String.IsNullOrEmpty(userInfo.LastName) &&
                       !String.IsNullOrEmpty(userInfo.PhoneNumber) &&
                       userInfo.DOB.HasValue;
            }
        }

        public static bool UpdateFirstTimeLogin(string userId, bool value)
        {
            using (var db = new WorktripEntities())
            {
                var userInfo = db.Users.FirstOrDefault(u => u.Id == userId);

                if (userInfo == null)
                {
                    return false;
                }

                userInfo.FirstTimeLogin = value;

                db.SaveChanges();
            }

            return true;
        }

        public static bool UpdateUserActionsLog(string userId, string action)
        {
            using (var db = new WorktripEntities())
            {
                var latestAction = db.UserActionsLogs.Where(a => a.UserId == userId && a.Action == action).OrderByDescending(a => a.Id).FirstOrDefault();

                if (latestAction == null || (latestAction != null && (DateTime.UtcNow - latestAction.Timestamp.Value).Minutes > 4))
                {
                    UserActionsLog userAction = new UserActionsLog
                    {
                        UserId = userId,
                        Action = action,
                        Timestamp = DateTime.UtcNow
                    };

                    db.UserActionsLogs.Add(userAction);
                    db.SaveChanges();
                }
            }

            return true;
        }
    }

    public class TaxStatus
    {
        public int Year { get; set; }
        public string PreparerId { get; set; }
        public string Status { get; set; }
        public decimal? Fee { get; set; }
        public string PromoCode { get; set; }
    }

    public class TaxInfo
    {
        public int Year { get; set; }
        public string BaseAirportCode { get; set; }
        public int? DaysInTrainingOrAway { get; set; }
        public double? DriverLayoverTips { get; set; }
        public double? TotalSpentLayoverTransportation { get; set; }
        public double? TransactionalFees { get; set; }
        public double? ClothingFees { get; set; }
        public double? LaundryFees { get; set; }
        public double? FlightGearLuggageFees { get; set; }
        public double? TechPurchasesFees { get; set; }
        public double? CellphoneBill { get; set; }
        public double? InternetBill { get; set; }
        public double? IdentityFees { get; set; }
        public double? UnreimbursedExpenses { get; set; }
        public string Airline { get; set; }
        public string PreparerNotes { get; set; }
        public decimal? TaxReturn { get; set; }
        public double? PerDiemsTotal { get; set; }
        public string DLState { get; set; }
        public Boolean? InternationalLayovers { get; set; }
        public bool? Married { get; set; }
        public bool? Dependent { get; set; }
        public bool? StudentLoans { get; set; }
        public bool? Stocks { get; set; }
        public bool? House { get; set; }
        public bool? HSA { get; set; }
        public bool? C1098T { get; set; }
        public bool? C1099R { get; set; }
        public bool? NewHire { get; set; }
        public bool? MultipleW2s { get; set; }
        public bool? C1099INT { get; set; }
        public bool? C1099G { get; set; }
        public bool? ScheduleK1 { get; set; }
        public bool? OwnBusiness { get; set; }
        public bool? Other { get; set; }
        public bool? Itemize { get; set; }
        public bool? FlyReserveDays { get; set; }
        public bool? DriveToWork { get; set; }
    }

    public class Question
    {
        public string QuestionText { get; set; }
        public string AnswerText { get; set; }
        public int TaxYear { get; set; }
        public string AnsweredBy { get; set; }
        public DateTime Date { get; set; }
    }

    public class TaxForm
    {
        DateTime Date { get; set; }
        String FilePath { get; set; }
    }

    public class Layover
    {
        public string AirportCode { get; set; }
        public int Days { get; set; }
    }
}