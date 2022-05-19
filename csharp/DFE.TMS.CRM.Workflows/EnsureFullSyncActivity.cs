using DFE.TMS.Business.Logic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace DFE.TMS.CRM.Workflows
{
    public class EnsureFullSyncActivity : CodeActivity
    {

        //Define the properties
        [Input("Bookable Resource")]
        [RequiredArgument]
        [ReferenceTarget("bookableresource")]
        public InArgument<EntityReference> BookableResource { get; set; }

        [Input("From")]
        [RequiredArgument]
        public InArgument<DateTime> StartFrom { get; set; }

        [Input("To")]
        [RequiredArgument]
        public InArgument<DateTime> EndTo { get; set; }

        [Output("Log")]
        public OutArgument<string> Log { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();

            //Create an Organization Service
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            ITracingService tracingService = context.GetExtension<ITracingService>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.InitiatingUserId);

            CalendarBusinessLogic calLogic = new CalendarBusinessLogic(service, tracingService);
            PortalTimeOffRequestLogic portalLogic = new PortalTimeOffRequestLogic(service, tracingService, calLogic);

            var results = portalLogic.EnsureFullSyncOfPortalTimeOffRequestsWithBookableResourceCalendar(
                BookableResource.Get<EntityReference>(context),
                StartFrom.Get<DateTime>(context),
                EndTo.Get<DateTime>(context));

            Log.Set(context, results.ToString());
        }
    }
}
