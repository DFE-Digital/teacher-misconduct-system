﻿using DFE.TMS.Business.Logic;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.CRM.Plugins
{
    public class PortalTimeOffBlockPreCreateActivity : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
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
                    target = (Entity)context.InputParameters["Target"];
                    if (target.Contains(C.PortalTimeBlock.JsonDates) && target.Contains(C.PortalTimeBlock.Resource))
                    {
                        string jsonDates = target.GetAttributeValue<string>(C.PortalTimeBlock.JsonDates);
                        portalBusinessLogic.CreateDeleteTimeOffRequests(target.GetAttributeValue<EntityReference>(C.PortalTimeBlock.Resource), jsonDates);
                    }
                    else
                        throw new Exception("No JsonDates and Portal Block Resource");
                }
            }
            catch (Exception exc)
            {
                throw new InvalidPluginExecutionException(exc.Message);
            }
        }
    }
}
