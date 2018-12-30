using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Worktrip.Models;
using System.Linq;
using System.Data.Entity;
using System.Diagnostics;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

namespace WorktripTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestEmail()
        {
            Worktrip.Models.EmailHelper.SendEmail("noreply@worktrip.tax", "WorkTrip", "dingo9082@yahoo.com", "David", "Hello", "Test Message");
        }

        [TestMethod]
        public void DbTest()
        {
            using (var db = new WorktripEntities())
            {
                var list = new List<int>()
                {
                    140,3,24,3432,2345106,4324,3,6,8
                };

                var evens = list.Where(n => n % 2 == 0).OrderByDescending(n => n);

                var firstOver10k = list.FirstOrDefault(n => n > 10000);

                var odds =
                    from n in list
                    where n % 2 == 1
                    select n;

                Trace.WriteLine(evens);
            }
        }

        [TestMethod]
        public void PopulatePerDiem()
        {
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(@"C:\Users\David\Desktop\excels\Airport IRS Database2.xlsx");
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets["Merge"];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            Dictionary<string, PerDiem> perDiems = new Dictionary<string, PerDiem>();

            for (int i = 2; i < 1450; i++)
            {

                //for (int j = 1; j <= 12; j++)
                //{
                //    //new line
                //    if (j == 1)
                //        Console.Write("\r\n");

                //    //write the value to the console
                //    if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                //    {
                //        var a = xlRange.Cells[i, j].Value2.ToString();

                //        Console.Write(a + "\t");
                //    }

                //    //add useful things here!   
                //}

                if (xlWorksheet.Cells[i, "K"] == null || xlWorksheet.Cells[i, "K"].Value2 == null) continue;


                string Country = xlWorksheet.Cells[i, "E"].Value2 != null ? xlWorksheet.Cells[i, "E"].Value2.ToString() : "US";
                string State = xlWorksheet.Cells[i, "D"].Value2 != null ? xlWorksheet.Cells[i, "D"].Value2.ToString() : "N/A";
                string City = xlWorksheet.Cells[i, "C"].Value2 != null ? xlWorksheet.Cells[i, "C"].Value2.ToString() : "N/A";
                string Airport = xlWorksheet.Cells[i, "G"].Value2 != null ? xlWorksheet.Cells[i, "G"].Value2.ToString() : "N/A";
                int Rate = 0;

                bool parsed = int.TryParse(xlWorksheet.Cells[i, "K"].Value2.ToString(), out Rate);

                if (!perDiems.ContainsKey(Airport))
                {
                    var perDiem = new PerDiem()
                    {
                        Country = Country,
                        State = State,
                        City = City,
                        Airport = Airport,
                        Rate = Rate
                    };

                    perDiems.Add(Airport, perDiem);
                }
            }

            xlWorkbook = xlApp.Workbooks.Open(@"C:\Users\David\Desktop\excels\FY2017-PerDiemRatesMasterFile2.xls");
            xlWorksheet = xlWorkbook.Sheets["FY2017"];

            for (int i = 4; i < 348; i++)
            {
                string state = xlWorksheet.Cells[i, "A"].Value2.ToString().ToLower();
                string city = xlWorksheet.Cells[i, "B"].Value2.ToString().ToLower();
                int rate = 0;

                bool parsed = int.TryParse(xlWorksheet.Cells[i, "G"].Value2.ToString(), out rate);

                foreach (var kv in perDiems)
                {
                    if (kv.Value.State.ToLower() == state && kv.Value.City.ToLower() == city)
                    {
                        kv.Value.Rate = rate;
                    }
                }
            }

            using (var db = new WorktripEntities())
            {
                foreach (var kv in perDiems)
                {
                    PerDiemRate pdr = new PerDiemRate()
                    {
                        AirportCode = kv.Key,
                        Rate = kv.Value.Rate
                    };

                    db.PerDiemRates.Add(pdr);
                }

                db.SaveChanges();
            }
        }


        public class PerDiem
        {
            public string Country;
            public string State;
            public string City;
            public string Airport;
            public int Rate;
        }
    }
}
