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
        [ReferenceTarget("bookableresource")]
        public InArgument<EntityReference> BookableResource { get; set; }

        [Input("StartFrom")]
        [Default("true")]
        public InArgument<DateTime> StartFrom { get; set; }

        [Input("EndTo")]
        public InArgument<DateTime> EndTo { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();

            //Create an Organization Service
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.InitiatingUserId);


        }
    }
}
