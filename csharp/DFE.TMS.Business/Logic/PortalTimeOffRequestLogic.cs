using DFE.TMS.Business.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.Business.Logic
{
    public class PortalTimeOffRequestLogic
    {
        public const string DateKeyFormat = "yyyyMMdd";
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

        public FullSyncResults EnsureFullSyncOfPortalTimeOffRequestsWithBookableResourceCalendar(
            EntityReference bookableReference, 
            DateTime startTime, 
            DateTime endTime)
        {
            FullSyncResults toReturn = new FullSyncResults();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            TracingService.Trace($"Entering: EnsureFullSyncOfPortalTimeOffRequestsWithBookableResourceCalendar(EntityReference bookableReference," 
             + $" DateTime startTime {startTime}"
             + $" DateTime endTime {endTime}");

            //1 . We search for all portal time off requests between the start and end time
            List<Entity> allPortalRequestsBetweenDates = GetAllPortalRequestsBetweenDates(bookableReference, startTime, endTime);
            TracingService.Trace($"Found {allPortalRequestsBetweenDates?.Count} portalRequestDates");
            toReturn.NumberOfPortalRequestsFound = allPortalRequestsBetweenDates?.Count ?? 0 ;

            //2. We ensure we only have 1 time off portal request per day.
            allPortalRequestsBetweenDates = EnsureOnlyOnePortalRequestPerDay(allPortalRequestsBetweenDates, out int deactivatedNumber);
            TracingService.Trace($"Number of Unique Portal Requests {allPortalRequestsBetweenDates?.Count}");
            toReturn.NumberOfPortalRequestsDeactivated = deactivatedNumber;

            //3.  Also between the start and end time we search for the days that we do not have a portal time off request
            List<DateTime> daysWithoutPortalRequest = GetListOfDatesThatDoNotHaveAPortalTimeOffRequestAganstIt(allPortalRequestsBetweenDates, startTime, endTime);
            TracingService.Trace($"There are {daysWithoutPortalRequest?.Count} days without portal requests between {startTime} and {endTime}");

            //4.  Foreach portal time off request we ensure that we have a calendar item and id's against the record
            EnsurePortalTimeOffRequestsHaveCalendarIds(allPortalRequestsBetweenDates);

            //5.  Foreach day in 2 we ensure that they are no time off requests.
            RemoveCalendarItemsFromDaysWithoutPortalRequest(bookableReference, daysWithoutPortalRequest, out int calendarItemsDeleted);
            toReturn.NumberOfTimeOffRequestsDeleted = calendarItemsDeleted;
            TracingService.Trace("Finished sync between dates ");

            stopWatch.Stop();
            toReturn.ElapsedTime = stopWatch.Elapsed;
            return toReturn;
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

        public void CreateDeleteTimeOffRequests(
            EntityReference bookableResource, 
            string jsonDatesAsString)
        {

            TracingService.Trace($"Entering: EntityReference bookableResource, string jsonDateAsString {jsonDatesAsString}");
            
            if (bookableResource != null && !string.IsNullOrEmpty(jsonDatesAsString))
            {
                PortalBlockJsonDates portalBlockJsonDates = DeserializePortalBlockJsonDates(jsonDatesAsString);
                if (portalBlockJsonDates.added != null && portalBlockJsonDates.added.Count() > 0)
                {
                    TracingService.Trace("We have added dates so we need to ensure that these can be created!");
                    foreach (Added dateAdded in portalBlockJsonDates.added)
                    {
                        DateTime date = DateTime.Parse(dateAdded.date);
                        DateTime dateStart = new DateTime(date.Year, date.Month, date.Day).AddHours(3);
                        DateTime dateEnd = new DateTime(date.Year, date.Month, date.Day).AddHours(22);

                        Entity portalTimeOffRequest = new Entity(C.PortalTimeOffRequest.EntityName);
                        portalTimeOffRequest[C.PortalTimeOffRequest.Resource] = bookableResource;
                        portalTimeOffRequest[C.PortalTimeOffRequest.Name] = "Unavailable";
                        portalTimeOffRequest[C.PortalTimeOffRequest.Start] = dateStart;
                        portalTimeOffRequest[C.PortalTimeOffRequest.End] = dateEnd;

                        TracingService.Trace($"Creating time off date '{dateStart}' for bookable resource {bookableResource.Id}.");
                        PreCreatePortalTimeOffRequest(portalTimeOffRequest);
                        OrganizationService.Create(portalTimeOffRequest);
                    }
                }

                if (portalBlockJsonDates.removed != null && portalBlockJsonDates.removed.Count() > 0)
                {
                    TracingService.Trace("We have removed dates so we need to ensure that these can be removed!");
                    foreach (Removed removed in portalBlockJsonDates.removed)
                    {
                        Guid id = Guid.Parse(removed.id);
                        Entity portalTimeOffRequest = new Entity(C.PortalTimeOffRequest.EntityName, id);
                        portalTimeOffRequest[C.PortalTimeOffRequest.State] = new OptionSetValue(1);
                        TracingService.Trace($"Deactivating portalTimeOffRequest '{id}' for bookable resource {bookableResource.Id}, with date {removed.date}");
                        DeleteTimeOffCalendarItem(portalTimeOffRequest);
                        OrganizationService.Update(portalTimeOffRequest);
                    }
                }
            } else
                throw new Exception("Bookable Resource is null! && json Dates string is null!!");
        }

        private PortalBlockJsonDates DeserializePortalBlockJsonDates(string jsonDatesAsString)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                //initialize DataContractJsonSerializer object and pass Student class type to it
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(PortalBlockJsonDates));

                //user stream writer to write JSON string data to memory stream
                StreamWriter writer = new StreamWriter(memoryStream);
                writer.Write(jsonDatesAsString);
                writer.Flush();

                memoryStream.Position = 0;
                //get the Desrialized data in object of type Student
                PortalBlockJsonDates serializedObject = (PortalBlockJsonDates)serializer.ReadObject(memoryStream);

                return serializedObject;
            }
        }

        private List<Entity> EnsureOnlyOnePortalRequestPerDay(List<Entity> allPortalRequestsBetweenDates, out int deactivatedNumber)
        {
            Dictionary<string, Entity> uniquePortalRequests = new Dictionary<string, Entity>();
            deactivatedNumber = 0;
            foreach (Entity entity in allPortalRequestsBetweenDates)
            {
                if (entity.Contains(C.PortalTimeOffRequest.Start))
                {
                    var retrievedDate = entity.GetAttributeValue<DateTime>(C.PortalTimeOffRequest.Start);
                    if (uniquePortalRequests.ContainsKey(retrievedDate.ToString(DateKeyFormat)))
                    {
                        deactivatedNumber++;
                        DeactivatePortalTimeOffRequest(entity);
                    }
                    else
                    {
                        uniquePortalRequests.Add(retrievedDate.ToString(DateKeyFormat), entity);
                    }
                }

            }

            return uniquePortalRequests?.Select(x => x.Value).ToList();
        }

        private void DeactivatePortalTimeOffRequest(Entity entity)
        {
            TracingService.Trace($"Deactivating portal time off request with date {entity.GetAttributeValue<DateTime>(C.PortalTimeOffRequest.Start)}");

            Entity entityToDeactivate = new Entity(C.PortalTimeOffRequest.EntityName, entity.Id);
            entityToDeactivate[C.PortalTimeOffRequest.State] = new OptionSetValue(1);
            entityToDeactivate[C.PortalTimeOffRequest.StatusCode] = new OptionSetValue(2);

            OrganizationService.Update(entityToDeactivate);
        }

        private void RemoveCalendarItemsFromDaysWithoutPortalRequest(
            EntityReference bookableReference, 
            List<DateTime> daysWithoutPortalRequest,
            out int calendarItemsDeleted)
        {
            calendarItemsDeleted = 0;
            foreach (var date in daysWithoutPortalRequest)
            {
                if (!CalendarBusinessLogic.OkToCreateTimeOffRequest(bookableReference, date, out List<CalendarRule> rules))
                {
                    TracingService.Trace($"Deleting calendarItems for date {date}");
                    foreach (var cRule in rules)
                    {
                        CalendarBusinessLogic.DeleteCalendarRequest(Guid.Parse(cRule.CalendarId), Guid.Parse(cRule.InnerCalendarId));
                        calendarItemsDeleted++;
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

            return new List<Entity>();
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
        private List<DateTime> GetListOfDatesThatDoNotHaveAPortalTimeOffRequestAganstIt(
            List<Entity> portalTimeOffRequest, 
            DateTime from, 
            DateTime to)
        {
            Dictionary<string, DateTime> daysWithouthPortalTimeOffRequests = new Dictionary<string, DateTime>();

            // Sanitize from and to dates just in case
            DateTime dateFrom = new DateTime(from.Year, from.Month, from.Day);
            DateTime dateTo = new DateTime(to.Year, to.Month, to.Day);

            // Then if it's the same day and only 1 check
            if (dateFrom.CompareTo(dateTo) == 0)
            {
                TracingService.Trace($"Date {dateTo} From and to are the same.");
                daysWithouthPortalTimeOffRequests.Add(dateFrom.ToString(DateKeyFormat), dateFrom);
            }
            else
            {
                DateTime start = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day);
                // soon as the start goes over 1 day of the date to then complete the loop
                while (start < dateTo)
                {
                    if (!daysWithouthPortalTimeOffRequests.ContainsKey(start.ToString(DateKeyFormat)))
                    {
                        daysWithouthPortalTimeOffRequests.Add(start.ToString(DateKeyFormat), start);
                    }

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
                        if (daysWithouthPortalTimeOffRequests.ContainsKey(date.ToString(DateKeyFormat)))
                        {
                            daysWithouthPortalTimeOffRequests.Remove(date.ToString(DateKeyFormat));
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
                                UpdatePortalRequestWithCalendarIds(portalRequest.Id, Guid.Parse(foundRules[0].CalendarId), Guid.Parse(foundRules[0].InnerCalendarId));
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

            if (everythingChecksOut.All(x => x == true))
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
                portalTimeOffRequestUpdate[C.PortalTimeOffRequest.CalendarId] = item1.Value.ToString("D");
                portalTimeOffRequestUpdate[C.PortalTimeOffRequest.InnerCalendarId] = item2.Value.ToString("D");

                OrganizationService.Update(portalTimeOffRequestUpdate);
            }
        }
    }
}
