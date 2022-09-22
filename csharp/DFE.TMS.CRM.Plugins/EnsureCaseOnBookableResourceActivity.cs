using DFE.TMS.Business.Logic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.CRM.Plugins
{
    public class EnsureCaseOnBookableResourceActivity : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                var service = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(context.UserId);
                var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                // Get the target entity
                Entity target = (Entity)context.InputParameters["Target"]; ;

                EnsureCaseIsOnBookableResourceTarget(target,service,trace);

            }
            catch (Exception exc)
            {
                throw new InvalidPluginExecutionException(exc.Message);
            }
        }

        private void EnsureCaseIsOnBookableResourceTarget(Entity target, IOrganizationService service, ITracingService trace)
        {
            if (target.Contains(C.BookableResourceBooking.CaseActivityId)
               && !target.Contains(C.BookableResourceBooking.CaseId))
            {
                var caseActivityId = target.GetAttributeValue<EntityReference>(C.BookableResourceBooking.CaseActivityId);
                var caseActivity = service.Retrieve(C.CaseActivity.EntityName, caseActivityId.Id, new ColumnSet(C.CaseActivity.CaseId,C.CaseActivity.CaseActivityType));
                if (caseActivity != null 
                    && caseActivity.Contains(C.CaseActivity.CaseId) 
                    && caseActivity.Contains(C.CaseActivity.CaseActivityType))
                {
                    trace.Trace("We have the case Id and the case activity type. Need to check if it's an actual case");
                    var caseId = caseActivity.GetAttributeValue<EntityReference>(C.CaseActivity.CaseId);
                    var caseActivityType = caseActivity.GetAttributeValue<OptionSetValue>(C.CaseActivity.CaseActivityType);
                    if (caseId.LogicalName == C.Incident.EntityName && caseActivityType.Value == (int)C.CaseActivity.CaseActivityTypeEnum.Hearing)
                    {
                        trace.Trace($"We have fullfilled the conditions for populating the caseId '{caseId.Id}' now ensuring it's on the target.");
                        target[C.BookableResourceBooking.CaseId] = new EntityReference(C.Incident.EntityName, caseId.Id);
                    }
                }
            }
        }
    }
}
