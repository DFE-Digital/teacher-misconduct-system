using DFE.TMS.Business.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
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

        private static object LockPreCreate = new object();
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
            TracingService.Trace($"Entering: EnsureFullSyncOfPortalTimeOffRequestsWithBookableResourceCalendar(EntityReference bookableReference," 
             + $" DateTime startTime {startTime}"
             + $" DateTime endTime {endTime}");

            //1 . We search for all portal time off requests between the start and end time
            List<Entity> allPortalRequestsBetweenDates = GetAllPortalRequestsBetweenDates(bookableReference, startTime, endTime);
            TracingService.Trace($"Found {allPortalRequestsBetweenDates?.Count} portalRequestDates");
            
            //2.  Also between the start and end time we search for the days that we do not have a portal time off request
            List<DateTime> daysWithoutPortalRequest = GetListOfDatesThatDoNotHaveAPortalTimeOffRequestAganstIt(allPortalRequestsBetweenDates, startTime, endTime);
            TracingService.Trace($"There are {daysWithoutPortalRequest?.Count} days without portal requests between {startTime} and {endTime}");

            //3.  Foreach portal time off request we ensure that we have a calendar item and id's against the record
            EnsurePortalTimeOffRequestsHaveCalendarIds(allPortalRequestsBetweenDates);

            //4.  Foreach day in 2 we ensure that they are no time off requests.
            RemoveCalendarItemsFromDaysWithoutPortalRequest(bookableReference, daysWithoutPortalRequest);

            TracingService.Trace("Finishing sync");
        }

        private void RemoveCalendarItemsFromDaysWithoutPortalRequest(EntityReference bookableReference, List<DateTime> daysWithoutNoPortalRequest)
        {
            throw new NotImplementedException();
        }

        public void PreCreatePortalTimeOffRequest(Entity targetToCreate)
        {
            TracingService.Trace("Entering: PreCreatePortalTimeOffRequest(Entity targetToCreate)");
            // Get the date of the time off request
            // check and see if it's ok to create
            // create the time off request and populate the target to create
            // with the various ids 

            lock (LockPreCreate)
            {
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

        private List<Entity> GetAllPortalRequestsBetweenDates(EntityReference bookableResource, DateTime from, DateTime to)
        {
            TracingService.Trace($"Entering: GetAllPortalRequestsBetweenDates(EntityReference bookableResource, DateTime from {from}, DateTime to {to})");
            QueryExpression query = new QueryExpression(C.PortalTimeOffRequest.EntityName);
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.ColumnSet = new ColumnSet(true);

            // Condition First Find a bookable resource
            query.Criteria.AddCondition(new ConditionExpression(C.PortalTimeOffRequest.Resource, ConditionOperator.Equal, bookableResource.Id));

            // Condition Second only active portal time off requests
            query.Criteria.AddCondition(new ConditionExpression(C.PortalTimeOffRequest.State, ConditionOperator.Equal, 0));

            // Condition any portal request that are greater than and less than the given dates
            query.Criteria.AddCondition(new ConditionExpression(C.PortalTimeOffRequest.Start, ConditionOperator.GreaterEqual, from));
            query.Criteria.AddCondition(new ConditionExpression(C.PortalTimeOffRequest.Start, ConditionOperator.LessEqual, to));

            var entityCollection = OrganizationService.RetrieveMultiple(query);
            if (entityCollection?.Entities.Count > 0)
                return entityCollection.Entities.ToList();

            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="portalTimeOffRequest">
        /// The entity list contained should be the result of a previous query finding all portal time off requests within the specified
        /// date range.
        /// </param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private List<DateTime> GetListOfDatesThatDoNotHaveAPortalTimeOffRequestAganstIt(List<Entity> portalTimeOffRequest, DateTime from, DateTime to)
        {
            Dictionary<string, DateTime> daysWithouthPortalTimeOffRequests = new Dictionary<string, DateTime>();
            const string Format = "yyyyMMdd";

            // Sanitize from and to dates just in case
            DateTime dateFrom = new DateTime(from.Year, from.Month, from.Day);
            DateTime dateTo = new DateTime(to.Year, to.Month, to.Day);

            // Then if it's the same day and only 1 check
            if (dateFrom.CompareTo(dateTo) == 0)
            {
                daysWithouthPortalTimeOffRequests.Add(dateFrom.ToString(Format), dateFrom);
            }
            else
            {
                DateTime start = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day);
                // soon as the start goes over 1 day of the date to then complete the loop
                while (start.CompareTo(dateTo) != 1)
                {
                    if(!daysWithouthPortalTimeOffRequests.ContainsKey(start.ToString(Format)))
                        start = start.AddDays(1);
                }
            }

            // check and see if we have portal time off requests
            if (portalTimeOffRequest?.Count > 0)
            {
                foreach (var entity in portalTimeOffRequest)
                {
                    if (entity.Contains(C.PortalTimeOffRequest.Start))
                    {
                        DateTime date = entity.GetAttributeValue<DateTime>(C.PortalTimeOffRequest.Start);
                        if (daysWithouthPortalTimeOffRequests.ContainsKey(date.ToString(Format)))
                        {
                            daysWithouthPortalTimeOffRequests.Remove(date.ToString(Format));
                        }
                    }
                }
            }

            // Just return all the days found between the selection
            return daysWithouthPortalTimeOffRequests.Select(x => x.Value).ToList();
        }

        private void EnsurePortalTimeOffRequestsHaveCalendarIds(List<Entity> allPortalRequestsBetweenDates)
        {
            foreach (var portalRequest in allPortalRequestsBetweenDates)
            {
                if (portalRequest.Contains(C.PortalTimeOffRequest.Start) && portalRequest.Contains(C.PortalTimeOffRequest.Resource))
                {
                    // get the calendarRules for that day for this bookable resource
                    if (!CalendarBusinessLogic.OkToCreateTimeOffRequest(
                        portalRequest.GetAttributeValue<EntityReference>(C.PortalTimeOffRequest.Resource),
                        portalRequest.GetAttributeValue<DateTime>(C.PortalTimeOffRequest.Start),
                        out List<CalendarRule> foundRules))
                    {
                        // IF WE HAVE more time off requests than we should remove them from the resource calendar
                        if (foundRules.Count > 1)
                        {
                            TracingService.Trace("We have more rules for this day. Deleting these...");
                            foreach (var calendarRule in foundRules)
                            {
                                CalendarBusinessLogic.DeleteCalendarRequest(Guid.Parse(calendarRule.CalendarId), Guid.Parse(calendarRule.InnerCalendarId));
                            }

                            // Create then update the portal request with the new 
                            var idsToUpdatePortalRequest = CalendarBusinessLogic
                                .CreateTimeOffRequestForBookableResource(
                                portalRequest.GetAttributeValue<EntityReference>(C.PortalTimeOffRequest.Resource).Id,
                                portalRequest.GetAttributeValue<DateTime>(C.PortalTimeOffRequest.Start));

                            UpdatePortalRequestWithCalendarIds(portalRequest.Id, idsToUpdatePortalRequest.Item1, idsToUpdatePortalRequest.Item2);
                        }

                        // If we only have 1 rule then we ensure that the portal time off request matches the calendar id
                        if (foundRules.Count == 1)
                        {
                            TracingService.Trace($"We only have one occurrence of the calendar item for day {foundRules[0].StartTime}");
                            if (!CheckIfPortalRequestHasSameRuleInformation(portalRequest, foundRules[0]))
                            {
                                TracingService.Trace($"Updating portal time off request {portalRequest.Id} with calendarItems");
                                UpdatePortalRequestWithCalendarIds(portalRequest.Id, Guid.Parse(foundRules[0].CalendarId), Guid.Parse(foundRules[1].InnerCalendarId));
                            }
                        }
                    }
                    else
                    {
                        TracingService.Trace("Create calendar item for this portal request and update it!");
                        // Create then update the portal request with the new 
                        var idsToUpdatePortalRequest = CalendarBusinessLogic
                            .CreateTimeOffRequestForBookableResource(
                            portalRequest.GetAttributeValue<EntityReference>(C.PortalTimeOffRequest.Resource).Id,
                            portalRequest.GetAttributeValue<DateTime>(C.PortalTimeOffRequest.Start));

                        UpdatePortalRequestWithCalendarIds(portalRequest.Id, idsToUpdatePortalRequest.Item1, idsToUpdatePortalRequest.Item2);
                    }
                }
            }
        }

        private bool CheckIfPortalRequestHasSameRuleInformation(Entity portalRequest, CalendarRule calendarRule)
        {
            List<bool> everythingChecksOut = new List<bool>();

            everythingChecksOut.Add(portalRequest.Contains(C.PortalTimeOffRequest.CalendarId));
            everythingChecksOut.Add(portalRequest.Contains(C.PortalTimeOffRequest.InnerCalendarId));

            if (everythingChecksOut.All(x => true))
            {
                everythingChecksOut.Add(
                    Guid.Parse(portalRequest.GetAttributeValue<string>(C.PortalTimeOffRequest.CalendarId))
                    .CompareTo(Guid.Parse(calendarRule.CalendarId)) == 0);

                everythingChecksOut.Add(
                    Guid.Parse(portalRequest.GetAttributeValue<string>(C.PortalTimeOffRequest.InnerCalendarId))
                    .CompareTo(Guid.Parse(calendarRule.InnerCalendarId)) == 0);

                return everythingChecksOut.All(x => true);
            }

            return false;
        }

        private void UpdatePortalRequestWithCalendarIds(Guid id, Guid? item1, Guid? item2)
        {
            if (item1.HasValue && item2.HasValue)
            {
                Entity portalTimeOffRequestUpdate = new Entity(C.PortalTimeOffRequest.EntityName, id);
                portalTimeOffRequestUpdate[C.PortalTimeOffRequest.CalendarId] = item1.Value;
                portalTimeOffRequestUpdate[C.PortalTimeOffRequest.InnerCalendarId] = item2.Value;

                OrganizationService.Update(portalTimeOffRequestUpdate);
            }
        }
    }
}
