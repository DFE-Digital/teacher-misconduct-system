using DFE.TMS.Business.Logic;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.CRM.Plugins
{
    public class PortalTimeOffRequestDeleteActivity : IPlugin
    {
        public static object lockOperation = new object();
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var service = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(context.UserId);
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            var calendarBusinessLogic = new CalendarBusinessLogic(service, trace);
            var portalBusinessLogic = new PortalTimeOffRequestLogic(service, trace, calendarBusinessLogic);

            // Get the target entity
            Entity target = null;
            if (context.InputParameters.Contains("Target"))
            {
                if (context.InputParameters["Target"] is Entity)
                {
                    target = (Entity)context.InputParameters["Target"];

                    lock (lockOperation)
                    {
                        // Since we have the target we can now perform the action on this precreated entity.
                        portalBusinessLogic.DeleteTimeOffCalendarItem(target);
                    }
                }
            }
        }
    }
}
