using DFE.TMS.Business.Logic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.CRM.Workflows
{
    public class RollupFieldsActivity : CodeActivity
    {
        [Input("From Entity Name")]
        [RequiredArgument]
        public InArgument<string> FromEntityName{ get; set; }

        [Input("From Entity Id")]
        [RequiredArgument]
        public InArgument<string> FromEntityId { get; set; }

        [Input("From Entity Schema Attributes")]
        [RequiredArgument]
        public InArgument<string> FromEntitySchemaAttributes { get; set; }

        [Input("To Entity Name")]
        [RequiredArgument]
        public InArgument<string> ToEntityName { get; set; }

        [Input("To Entity Id")]
        [RequiredArgument]
        public InArgument<string> ToEntityId { get; set; }

        [Input("To Entity Schema Attributes")]
        [RequiredArgument]
        public InArgument<string> ToEntitySchemaAttributes { get; set; }


        protected override void Execute(CodeActivityContext context)
        {
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();

            //Create an Organization Service
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            ITracingService tracingService = context.GetExtension<ITracingService>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.InitiatingUserId);

            RollupFieldsToEntityLogic rollupFieldsLogic = new RollupFieldsToEntityLogic(tracingService, service);

            string[] fromEntityAttributeStrings = FromEntitySchemaAttributes.Get(context).Split(',');
            string[] toEntityAttributeStrings = ToEntitySchemaAttributes.Get(context).Split(',');

            tracingService.Trace($"Guid from Entity Id {FromEntityId.Get(context)}");
            tracingService.Trace($"Guid To Entity Id {ToEntityId.Get(context)}");

            rollupFieldsLogic.RollupToEntity(
                FromEntityName.Get(context),
                Guid.Parse(FromEntityId.Get(context)),
                fromEntityAttributeStrings,
                ToEntityName.Get(context),
                Guid.Parse(ToEntityId.Get(context)),
                toEntityAttributeStrings
            );
        }
    }
}
