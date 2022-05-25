using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.Business.Models
{
    public class PortalBlockJsonDates
    {
        public Added[] added { get; set; }
        public Removed[] removed { get; set; }
    }

    public class Added
    {
        public string date { get; set; }
    }

    public class Removed
    {
        public string date { get; set; }
        public string id { get; set; }
    }

}
