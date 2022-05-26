using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.Business.Models
{

    public class SaveCalendarJsonRequest
    {
        public Calendareventinfo CalendarEventInfo { get; set; }
    }

    public class Calendareventinfo
    {
        public string CalendarId { get; set; }
        public string EntityLogicalName { get; set; }
        public int TimeZoneCode { get; set; }
        public Rulesandrecurrence[] RulesAndRecurrences { get; set; }
    }

    public class Rulesandrecurrence
    {
        public Rule[] Rules { get; set; }
    }

    public class Rule
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Effort { get; set; }
        public int WorkHourType { get; set; }
    }

}
