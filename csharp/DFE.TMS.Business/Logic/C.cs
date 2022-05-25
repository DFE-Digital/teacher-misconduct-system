using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.Business.Logic
{
    public class C
    {
        public static class BookableResource
        {
            public const string EntityName = "bookableresource";
            public const string EntityId = EntityName + "id";
            public const string CalendarId = "calendarid";
            public const string FullSyncScript = "dfe_fullsynccompletefromscript";

            public const string State = "statecode";
            public const string StatusCode = "statuscode";
        }

        public static class CalendarRule
        {

            public const string EntityName = "calendarrule";
            public const string StartTime = "starttime";
            public const string Name = "name";
            public const string CalendarId = "calendarid";
        }

        public static class PortalTimeOffRequest
        {
            public const string EntityName = "mhc_portaltimeoffrequest";
            public const string EntityId = EntityName + "id";
            public const string CalendarRuleId = "dfe_calendarruleid";
            public const string CalendarId = "dfe_calendarid";
            public const string InnerCalendarId = "dfe_innercalendarid";
            public const string Resource = "mhc_resource";
            public const string Start = "mhc_start";
            public const string End = "mhc_end";
            public const string Name = "mhc_name";

            public const string State = "statecode";
            public const string StatusCode = "statuscode";
        }

        public static class FullSyncPortalTimeOffRequest
        {
            public const string EntityName = "dfe_fullsyncportaltimeoffrequest";
            public const string EntityId = EntityName + "id";
            public const string From = "dfe_from";
            public const string To = "dfe_to";
            public const string Log = "dfe_log";
            public const string Name = "dfe_name";
            public const string BookableResource = "dfe_bookableresourceid";

            public const string State = "statecode";
            public const string StatusCode = "statuscode";
        }

        public static class PortalTimeBlock
        {
            public const string EntityName = "mhc_portaltimeoffblock";
            public const string EntityId = EntityName + "id";

            public const string Resource = "mhc_resource";
            public const string JsonDates = "dfe_jsondates";
            public const string Name = "mhc_name";

            public const string State = "statecode";
            public const string StatusCode = "statuscode";
        }
    }
}
