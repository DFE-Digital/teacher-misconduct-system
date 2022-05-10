using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.Business.Logic
{
    public class PortalTimeOffRequestLogic
    {
        public IOrganizationService OrganizationService { get; }
        public ITracingService TracingService { get; }
        public CalendarBusinessLogic CalendarBusinessLogic { get; }


        public PortalTimeOffRequestLogic(
            IOrganizationService organizationService, 
            ITracingService tracingService,
            CalendarBusinessLogic calendarBusinessLogic)
        {
            OrganizationService = organizationService;
            TracingService = tracingService;
            CalendarBusinessLogic = calendarBusinessLogic;
        }

        public void EnsureFullSyncOfPortalTimeOffRequestsWithBookableResourceCalendar(
            EntityReference bookableReference, 
            DateTime startTime, 
            DateTime endTime)
        {
            //1 . We search for all portal time off requests between the start and end time
            //2.  Also between the start and end time we search for the days that we do not have a portal time off request
            //3.  Foreach portal time off request we ensure that we have a calendar item and id's against the record
            //4.  Foreach day in 2 we ensure that they are no time off requests.
        }

        public void PreCreatePortalTimeOffRequest(Entity targetToCreate)
        {
            // Get the date of the time off request
            // check and see if it's ok to create
            // create the time off request and populate the target to create
            // with the various ids 
        }

        public void PreDeleteTimeOffRequest(Entity targetToDelete)
        {
            // Given that we have the calendarid and inner calendar id
            // we can simply delete the calendar item
        }
    }
}
