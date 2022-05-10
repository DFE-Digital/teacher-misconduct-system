using DFE.TMS.Business.Logic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.Business.Console.Scratch
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = Connect();
            CalendarBusinessLogic logic = new CalendarBusinessLogic(client,new ConsoleTracingService());
            var calendarItems = logic.RetrieveUnavailabilityCalendarItemsForBookableResourceGivenStartTime(new EntityReference() { Id = Guid.Parse("91e7f215-8377-ec11-8d21-0022489da882") }, new DateTime(2022, 5, 4));
            // Delete the calendar rule item
            //logic.DeleteCalendarRequest(Guid.Parse(calendarItems[0].CalendarId), Guid.Parse(calendarItems[0].InnerCalendarId));
            //logic.CreateTimeOffRequestForBookableResource(Guid.Parse("91e7f215-8377-ec11-8d21-0022489da882"), new DateTime(2022, 5, 6));
        }

        public static CrmServiceClient Connect()
        {
            CrmServiceClient service = null;
            string connection = GetConnectionStringFromAppConfig("Connect");
            var connectionParams = connection.Split(';');
            //System.Console.WriteLine($"INFO: Connecting to {connectionParams[2].Trim()}");
            System.Console.WriteLine("Connecting ...");
            service = new CrmServiceClient(connection);
            return service;
        }

        private static string GetConnectionStringFromAppConfig(string name)
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[name].ConnectionString;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

    public class ConsoleTracingService : ITracingService
    {
        public void Trace(string format, params object[] args)
        {
            System.Console.WriteLine(string.Format(format, args));
        }
    }
}
