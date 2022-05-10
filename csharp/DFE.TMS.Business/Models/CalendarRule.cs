using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.Business.Models
{
    public class CalendarRule
    {
        public string CalendarId { get; set; }
        public string InnerCalendarId { get; set; }
        public string CalendarRuleId { get; set; }
        public string ExtentCode { get; set; }
        public DateTime StartTime { get; set; }
    }
}
