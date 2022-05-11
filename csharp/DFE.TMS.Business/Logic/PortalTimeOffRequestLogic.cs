using DFE.TMS.Business.Models;
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

        private static object LockPostCreate = new object();
        private static object LockDelete = new object();


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
            TracingService.Trace("Entering: PreCreatePortalTimeOffRequest(Entity targetToCreate)");
            // Get the date of the time off request
            // check and see if it's ok to create
            // create the time off request and populate the target to create
            // with the various ids 
           
            if (targetToCreate != null)
                {
                if (targetToCreate.Contains(C.PortalTimeOffRequest.Resource)
                    && targetToCreate.Contains(C.PortalTimeOffRequest.Start))
                {
                    TracingService.Trace("Passed 1st Phase of Conditions");
                    var bookableResource = targetToCreate.GetAttributeValue<EntityReference>(C.PortalTimeOffRequest.Resource);
                    var timeOffDate = targetToCreate.GetAttributeValue<DateTime>(C.PortalTimeOffRequest.Start);

                    var calenderItem = CalendarBusinessLogic.CreateTimeOffRequestForBookableResource(bookableResource.Id, timeOffDate);
                    if (calenderItem.Item1.HasValue && (calenderItem.Item1.Value.CompareTo(Guid.Empty) != 0))
                    {
                        TracingService.Trace("We have a calendar item. Save against the portal time off request");
                        targetToCreate.Attributes[C.PortalTimeOffRequest.Name] = "Unavailable";
                        targetToCreate[C.PortalTimeOffRequest.CalendarId] = calenderItem.Item1.Value.ToString("D");
                        targetToCreate[C.PortalTimeOffRequest.InnerCalendarId] = calenderItem.Item2.Value.ToString("D");
                    }
                    else
                        throw new Exception($"Cannot create portal request item for this day {timeOffDate.ToString("yyyy-MM-dd")}!!");

                }
            }
        }

        public void PostCreateActivity(Entity targetEntityCreated)
        {
            lock (LockPostCreate)
            {

                TracingService.Trace("Entering: PostCreateActivity(Entity targetEntityCreated)");
                // Get the date of the time off request
                // check and see if it's ok to create
                // create the time off request and populate the target to create
                // with the various ids 
                if (targetEntityCreated != null)
                {
                    if (targetEntityCreated.Contains(C.PortalTimeOffRequest.Resource)
                        && targetEntityCreated.Contains(C.PortalTimeOffRequest.Start))
                    {
                        TracingService.Trace("Passed 1st Phase of Conditions");
                        var bookableResource = targetEntityCreated.GetAttributeValue<EntityReference>(C.PortalTimeOffRequest.Resource);
                        var timeOffDate = targetEntityCreated.GetAttributeValue<DateTime>(C.PortalTimeOffRequest.Start);

                        var calenderItem = CalendarBusinessLogic.CreateTimeOffRequestForBookableResource(bookableResource.Id, timeOffDate);
                        if (calenderItem.Item1.HasValue && (calenderItem.Item1.Value.CompareTo(Guid.Empty) != 0))
                        {
                            TracingService.Trace("We have a calendar item. Save against the portal time off request");
                            Entity targetEntityToUpdate = new Entity(C.PortalTimeOffRequest.EntityName, targetEntityCreated.Id);

                            targetEntityToUpdate.Attributes[C.PortalTimeOffRequest.Name] = "Unavailable";
                            targetEntityToUpdate[C.PortalTimeOffRequest.CalendarId] = calenderItem.Item1.Value.ToString("D");
                            targetEntityToUpdate[C.PortalTimeOffRequest.InnerCalendarId] = calenderItem.Item2.Value.ToString("D");

                            OrganizationService.Update(targetEntityToUpdate);
                        }
                        else
                            throw new Exception($"Cannot create portal request item for this day {timeOffDate.ToString("yyyy-MM-dd")}!!");

                    }
                }
            }
        }

        public void DeleteTimeOffCalendarItem(Entity targetToDelete)
        {
            // Given that we have the calendarid and inner calendar id
            // we can simply delete the calendar item
            if (targetToDelete != null && targetToDelete.Contains(C.PortalTimeOffRequest.State))
            {
                // Check for the inactivity of the portal time off request
                if (targetToDelete.GetAttributeValue<OptionSetValue>(C.PortalTimeOffRequest.State).Value == 1)
                {
                    // retrieve the resource
                    Entity portalTimeOffRequest = OrganizationService.Retrieve(C.PortalTimeOffRequest.EntityName, targetToDelete.Id,
                        new Microsoft.Xrm.Sdk.Query.ColumnSet(C.PortalTimeOffRequest.CalendarId, C.PortalTimeOffRequest.InnerCalendarId));
                    if (portalTimeOffRequest != null
                        && portalTimeOffRequest.Contains(C.PortalTimeOffRequest.CalendarId)
                        && portalTimeOffRequest.Contains(C.PortalTimeOffRequest.InnerCalendarId))
                    {
                        TracingService.Trace("Time Off Request will be in-active .. deleting calendar item.");
                        Guid calendarId = Guid.Parse(portalTimeOffRequest.GetAttributeValue<string>(C.PortalTimeOffRequest.CalendarId));
                        Guid innerCalendarId = Guid.Parse(portalTimeOffRequest.GetAttributeValue<string>(C.PortalTimeOffRequest.InnerCalendarId));
                        CalendarBusinessLogic.DeleteCalendarRequest(calendarId, innerCalendarId);
                    }
                }
            }
        }
    }
}
