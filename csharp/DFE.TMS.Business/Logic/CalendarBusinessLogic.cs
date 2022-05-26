using DFE.TMS.Business.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DFE.TMS.Business.Logic
{
    public enum CalendarRuleType
    {
        Workable = 1,
        TimeOff = 2,
        Transparent = 0
    }

    public class CalendarBusinessLogic
    {
        public IOrganizationService OrganizationService { get; }
        public ITracingService TracingService { get; }

        public CalendarBusinessLogic(IOrganizationService organizationService, ITracingService tracingService)
        {
            OrganizationService = organizationService;
            TracingService = tracingService;
        }

        public bool OkToCreateTimeOffRequest(EntityReference bookableResourceId, DateTime potentialDayOff, out List<CalendarRule> foundCalendarRules)
        {
            var itemsFound = RetrieveUnavailabilityCalendarItemsForBookableResourceGivenStartTime(bookableResourceId, potentialDayOff);
            foundCalendarRules = itemsFound;

            if (itemsFound == null)
            {
                TracingService.Trace($"Ok for {bookableResourceId.Id} to create TimeOff on this day {potentialDayOff}");
                return true;
            }
            else
                TracingService.Trace($"Bookable Resource {bookableResourceId.Id} has {itemsFound.Count()} on this day {potentialDayOff}. Cannot create more time off!");

            return false;
        }

        public List<CalendarRule> RetrieveUnavailabilityCalendarItemsForBookableResourceGivenStartTime(EntityReference bookableResource, DateTime startTime)
        {
            TracingService.Trace($"Entering: RetrieveUnavailabilityCalendarItemsForBookableResourceGivenStartTime(EntityReference bookableResource, DateTime startTime {startTime})");
            Entity bookableResourceEntity = RetrieveBookableResource(bookableResource);
            if (bookableResourceEntity != null && bookableResourceEntity.Contains(C.BookableResource.CalendarId))
            {
                return RetrieveUnavailabilityCalendarItemsFromCalendar(bookableResourceEntity.GetAttributeValue<EntityReference>(C.BookableResource.CalendarId), startTime);
            }

            return null;
        }

        public Entity RetrieveBookableResource(EntityReference bookableResource)
        {
            var toReturn = OrganizationService.Retrieve(
                C.BookableResource.EntityName,
                bookableResource.Id,
                new Microsoft.Xrm.Sdk.Query.ColumnSet(C.BookableResource.CalendarId));

            if (toReturn != null)
                return toReturn;

            throw new Exception("Bookable Resource does not exist");
        }

        public List<CalendarRule> RetrieveUnavailabilityCalendarItemsFromCalendar(
            EntityReference bookableResourceCalendar,
            DateTime startTime)
        {
            TracingService.Trace($"Entering: RetrieveUnavailabilityCalendarItemsFromCalendar(EntityReference bookableResourceCalendar, DateTime startTime {startTime})");
            string formattedStartTime = $"{startTime.ToString("yyyy-MM-dd")}T00:00:00Z";
            string fetchxml = string.Format("<fetch top=\"50\">"
                              + "<entity name =\"calendarrule\">"
                              + "<filter>"
                              + "<condition attribute =\"calendarid\" operator=\"eq\" value=\"{0}\" />"
                              + "<condition attribute =\"starttime\" operator=\"eq\" value=\"{1}\" />"
                              + "<condition attribute =\"extentcode\" operator=\"eq\" value=\"{2}\" />"
                              + "</filter>"
                              + "</entity>"
                              + "</fetch >", bookableResourceCalendar.Id.ToString("B"), formattedStartTime, (int)CalendarRuleType.TimeOff);

            ExecuteFetchRequest request = new ExecuteFetchRequest();
            request.FetchXml = fetchxml;

            var response = (ExecuteFetchResponse)OrganizationService.Execute(request);

            if (response.Results.Count > 0)
            {
                var serializer = new XmlSerializer(typeof(resultset));
                using (TextReader reader = new StringReader(response.FetchXmlResult))
                {
                    var responseCalendar = (resultset)serializer.Deserialize(reader);
                    return responseCalendar
                        .result?
                        .ToList()
                        .Select(x => new CalendarRule() {
                            CalendarId = x.calendarid.Value,
                            InnerCalendarId = x.innercalendarid.Value,
                            CalendarRuleId = x.calendarruleid,
                            ExtentCode = x.extentcode.Value.ToString(),
                            StartTime = x.starttime.Value
                        }).ToList();
                }
            }

            return null;
        }

        public bool CalendarRuleExists(Guid calendarId, Guid innerCalendarId)
        {
            TracingService.Trace($"Entering: CheckIfCalendarRuleExists(Guid calendarId {calendarId}, Guid innerCalendarId {innerCalendarId})");
            string fetchxml = string.Format("<fetch top=\"50\">"
                              + "<entity name =\"calendarrule\">"
                              + "<filter>"
                              + "<condition attribute =\"calendarid\" operator=\"eq\" value=\"{0}\" />"
                              + "<condition attribute =\"innercalendarid\" operator=\"eq\" value=\"{1}\" />"
                              + "</filter>"
                              + "</entity>"
                              + "</fetch >", calendarId.ToString("B"), innerCalendarId.ToString("B"));

            ExecuteFetchRequest request = new ExecuteFetchRequest();
            request.FetchXml = fetchxml;

            var response = (ExecuteFetchResponse)OrganizationService.Execute(request);

            if (response.Results.Count > 0)
            {
                var serializer = new XmlSerializer(typeof(resultset));
                using (TextReader reader = new StringReader(response.FetchXmlResult))
                {
                    var responseCalendar = (resultset)serializer.Deserialize(reader);
                    var calendarRules = responseCalendar
                        .result?
                        .ToList()
                        .Select(x => new CalendarRule()
                        {
                            CalendarId = x.calendarid.Value,
                            InnerCalendarId = x.innercalendarid.Value,
                            CalendarRuleId = x.calendarruleid,
                            ExtentCode = x.extentcode.Value.ToString(),
                            StartTime = x.starttime.Value
                        }).ToList();

                    if (calendarRules != null && calendarRules.Count > 0)
                        return true;
                }
            }

            return false;
        }

        public void DeleteCalendarRequest(Guid calendarId, Guid innerCalendarId)
        {
            if (CalendarRuleExists(calendarId, innerCalendarId))
            { 
                TracingService.Trace($"Entering DeleteCalendarRequest(Guid calendarId {calendarId}, Guid innerCalendarId {innerCalendarId})");
                OrganizationRequest request = new OrganizationRequest("msdyn_DeleteCalendar");
                string requestString = string.Format("{{\"CalendarId\":\"{0}\",\"EntityLogicalName\":\"bookableresource\",\"InnerCalendarId\":\"{1}\"}}", calendarId.ToString("D"), innerCalendarId.ToString("D"));

                request["CalendarEventInfo"] = requestString;
                OrganizationResponse response = (OrganizationResponse)OrganizationService.Execute(request);

                string responseResult = response.Results["InnerCalendarIds"]?.ToString();
            }
        }

        public (Guid?,Guid?) CreateTimeOffRequestForBookableResource(Guid bookableResourceId, DateTime timeOfDay)
        {
            if (OkToCreateTimeOffRequest(new EntityReference() { Id = bookableResourceId }, timeOfDay, out List<CalendarRule> foundRules))
            {
                TracingService.Trace($"Entering: CreateTimeOffRequestForBookableResource(Guid bookableResourceId {bookableResourceId}, DateTime timeOfDay {timeOfDay})");
                Entity bookableResource = RetrieveBookableResource(new EntityReference(C.BookableResource.EntityName) { Id = bookableResourceId });
                if (bookableResource != null && bookableResource.Contains(C.BookableResource.CalendarId))
                {
                    var calendarId = bookableResource.GetAttributeValue<EntityReference>(C.BookableResource.CalendarId).Id;
                    var innerCalendarId = CreateTimeOffRequest(calendarId, timeOfDay);
                    return (calendarId, innerCalendarId);
                }
            }

            return (Guid.Empty,Guid.Empty);
        }

        public (Guid?, Guid?) CreateTimeOffRequestForBookableResourceWithoutCheck(Guid bookableResourceId, DateTime timeOfDay)
        {
            TracingService.Trace($"Entering: CreateTimeOffRequestForBookableResource(Guid bookableResourceId {bookableResourceId}, DateTime timeOfDay {timeOfDay})");
            Entity bookableResource = RetrieveBookableResource(new EntityReference(C.BookableResource.EntityName) { Id = bookableResourceId });
            if (bookableResource != null && bookableResource.Contains(C.BookableResource.CalendarId))
            {
                var calendarId = bookableResource.GetAttributeValue<EntityReference>(C.BookableResource.CalendarId).Id;
                var innerCalendarId = CreateTimeOffRequest(calendarId, timeOfDay);
                return (calendarId, innerCalendarId);
            }

            return (Guid.Empty, Guid.Empty);
        }

        public Guid? CreateTimeOffRequest(Guid calendarId, DateTime timeOfDay)
        {
            TracingService.Trace($"Entering: CreateTimeOffRequest(Guid calendarId {calendarId}, DateTime timeOfDay {timeOfDay}) ");
            OrganizationRequest request = new OrganizationRequest("msdyn_SaveCalendar");
            string formattedStartTime = $"{timeOfDay.ToString("yyyy-MM-dd")}T03:00:00Z";
            string formattedEndTime = $"{timeOfDay.ToString("yyyy-MM-dd")}T23:59:00Z";
            string requestString = $"{{\"CalendarId\":\"{calendarId.ToString("D")}\",\"EntityLogicalName\":\"bookableresource\",\"TimeZoneCode\":85,\"RulesAndRecurrences\":[{{\"Rules\":[{{\"StartTime\":\"{formattedStartTime}\",\"EndTime\":\"{formattedEndTime}\",\"Effort\":1,\"WorkHourType\":3}}]}}]}}";

            request["CalendarEventInfo"] = requestString;

            OrganizationResponse response = (OrganizationResponse)OrganizationService.Execute(request);
            string responseResult = response?.Results["InnerCalendarIds"]?.ToString();

            if (!string.IsNullOrEmpty(responseResult))
            {

                var results = GetJsonStringArrays(responseResult);
                if (results != null && results.Count() == 1)
                    return Guid.Parse(results[0]);
            }

            return null;
        }

        private string[] GetJsonStringArrays(string jsonString)
        {

            using (MemoryStream memoryStream = new MemoryStream())
            {
                //initialize DataContractJsonSerializer object and pass Student class type to it
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(string[]));

                //user stream writer to write JSON string data to memory stream
                StreamWriter writer = new StreamWriter(memoryStream);
                writer.Write(jsonString);
                writer.Flush();

                memoryStream.Position = 0;
                //get the Desrialized data in object of type Student
                string[] serializedObject = (string[])serializer.ReadObject(memoryStream);

                return serializedObject;
            }
        }
    }
}
