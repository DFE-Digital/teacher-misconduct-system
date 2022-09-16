using DFE.TMS.Business.Logic;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.CRM.Plugins
{
    public class IncidentHearingStartDateActivity : IPlugin
    {
        private const int NOH_DAYS_TO_BE_ADDED = -84;

        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                var service = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(context.UserId);
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                // Get the target entity
                Entity target = null;
                Entity image = null;

                NoHDueToBeSentCalculation(context, trace, target, image);
            }
            catch (Exception exc)
            {
                throw new InvalidPluginExecutionException(exc.Message);
            }
        }

        private  void NoHDueToBeSentCalculation(IPluginExecutionContext context, ITracingService trace, Entity target, Entity image)
        {
            if (context.InputParameters.Contains("Target")
                                && context.PreEntityImages.Contains("image"))
            {
                image = context.PreEntityImages["image"];
                target = (Entity)context.InputParameters["Target"];

                if (!image.Contains(C.Incident.NohDueToBeSentOverride)
                    || (image.Contains(C.Incident.NohDueToBeSentOverride) &&
                       !image.GetAttributeValue<bool>(C.Incident.NohDueToBeSentOverride)))
                {
                    trace.Trace("Override is No or Null. Proceeding to Update 'Noh Due To Be Sent Date'");
                    if (target.Contains(C.Incident.HearingStartDateRollup))
                    {
                        DateTime hearingStartDate = target.GetAttributeValue<DateTime>(C.Incident.HearingStartDateRollup);
                        DateTime nohDueToBeSentDate = hearingStartDate.AddDays(NOH_DAYS_TO_BE_ADDED);

                        target[C.Incident.NohDueToBeSentDate] = nohDueToBeSentDate;
                    }
                }

            }
        }
    }
}
