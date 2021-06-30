using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamsBot.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public string Rep { get; set; }

        public string Region { get; set; }

        public string Item { get; set; }

        public int Units { get; set; }

        public double UnitCost { get; set; }

        public double Total { get; set; }
    }
}
