using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Worktrip.Models
{
    public class HelpersModel
    {
    }

    public class AirlineSelectModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static List<AirlineSelectModel> GetAirlines()
        {
            using (var db = new WorktripEntities())
            {
                return db.Airlines.Select(a => new AirlineSelectModel
                {
                    Id = a.Id,
                    Name = a.Name
                }).ToList();
            }
        }
    }

    public class AirportSelectModel
    {
        public string Id { get; set; }
        public string Text { get; set; }

        public static List<AirportSelectModel> SearchAirports(string searchTerm)
        {
            using (var db = new WorktripEntities())
            {
                var query = db.Airports.Where(a => a.Code.StartsWith(searchTerm) || a.Name.StartsWith(searchTerm)).ToList();

                return query.Select(q => new AirportSelectModel
                {
                    Id = q.Code,
                    Text = q.Name
                }).ToList();
            }
        }
    }
}