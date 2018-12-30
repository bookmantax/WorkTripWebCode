using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Worktrip.Models;

namespace Worktrip
{
    public class WorktripInit
    {
        public static Dictionary<string, double> perDiemRates;

        public static void InitPerDiemRates()
        {
            perDiemRates = new Dictionary<string, double>();

            using (var db = new WorktripEntities())
            {
                var retrieved = db.PerDiemRates.ToList();

                foreach (var perDiem in retrieved)
                {
                    perDiemRates.Add(perDiem.AirportCode, perDiem.Rate);
                }
            }
        }
    }
}