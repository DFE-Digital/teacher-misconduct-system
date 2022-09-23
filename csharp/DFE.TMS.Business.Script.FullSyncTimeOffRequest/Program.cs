using DFE.TMS.Business.Logic;
using DFE.TMS.Business.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.Business.Script.FullSyncTimeOffRequest
{
    public class Program
    {
        private static string HEARING_FROMFIELDS = "dfe_hearingstartdate,dfe_hearingenddate,dfe_hearingstatus,dfe_chairid,dfe_hearingtype";
        private static string HEARING_TOFIELDS = "dfe_hearingstartdaterollup,dfe_hearingenddaterollup,dfe_hearingstatusrollup,dfe_chairidrollup,dfe_hearingtyperollup";
        
        private static string NOH_FROMFIELDS = "dfe_noticeofhearingresponsereceiveddate,dfe_noticeofhearingsentdate,dfe_draftnoticeofhearingreceiveddate";
        private static string NOH_TOFIELDS =   "dfe_noticeofhearingresponsereceiveddaterollup,dfe_noticeofhearingsentdaterollup,dfe_draftnoticeofhearingreceiveddaterollup";
        
        private static string ILF_FROMFIELDS = "dfe_instructiondate";
        private static string ILF_TOFIELDS =   "dfe_instructiondaterollup";
        
        private static string HEARINGPREP_FROMFIELDS = "dfe_bundleexpecteddate,dfe_bundlereceiveddate,dfe_formalnoticepublished,dfe_hearingbundledistributeddate";
        private static string HEARINGPREP_TOFIELDS =   "dfe_bundleexpecteddaterollup,dfe_bundlereceiveddaterollup,dfe_formalnoticepublishedrollup,dfe_hearingbundledistributeddaterollup";
        
        static void Main(string[] args)
        {
            Stopwatch stopWatchFullProgram = new Stopwatch();
            stopWatchFullProgram.Start();
            var tracing = new Log4NetTracing();
            try
            {
                EnsureCaseActivitiesRollupForHDT(tracing);
            }
            catch (Exception exc)
            {
                tracing.Trace($"ERROR - {exc.Message}");
            }
            finally {
                stopWatchFullProgram.Stop();
                System.Console.WriteLine($"Finished .... and took {stopWatchFullProgram.Elapsed.TotalSeconds}s to Process;");
            }

            System.Console.Read();
        }

        private static void EnsureCaseActivitiesRollupForHDT(Log4NetTracing log4NetTracing)
        {
            System.Console.WriteLine("Initiating HDT Field Rollup");
            var service = Connect();
            var rollupFieldsLogic = new RollupFieldsToEntityLogic(log4NetTracing,service);

            var caseActivitiesToPageThrough = RetrieveAllRelevantCaseActivities(service, log4NetTracing);
            int count = 0;

            foreach (Entity entity in caseActivitiesToPageThrough)
            {
                if (entity.Contains(C.CaseActivity.CaseActivityType) && entity.Contains(C.CaseActivity.CaseId))
                {
                    var fromId = entity.Id;
                    var toId = entity.GetAttributeValue<EntityReference>(C.CaseActivity.CaseId);

                    C.CaseActivity.CaseActivityTypeEnum enumType = (C.CaseActivity.CaseActivityTypeEnum)Enum.Parse(typeof(C.CaseActivity.CaseActivityTypeEnum), entity.GetAttributeValue<OptionSetValue>(C.CaseActivity.CaseActivityType).Value.ToString());

                    switch (enumType)
                    {
                        case C.CaseActivity.CaseActivityTypeEnum.Hearing:
                            {
                                rollupFieldsLogic.RollupToEntity(C.CaseActivity.EntityName,fromId,HEARING_FROMFIELDS.Split(','), C.Incident.EntityName, toId.Id, HEARING_TOFIELDS.Split(','));
                                break; 
                            }
                        case C.CaseActivity.CaseActivityTypeEnum.NoticeOfHearing:
                            {
                                rollupFieldsLogic.RollupToEntity(C.CaseActivity.EntityName, fromId, NOH_FROMFIELDS.Split(','), C.Incident.EntityName, toId.Id, NOH_TOFIELDS.Split(','));
                                break;
                            }
                        case C.CaseActivity.CaseActivityTypeEnum.InstructingLegalFirm:
                            {
                                rollupFieldsLogic.RollupToEntity(C.CaseActivity.EntityName, fromId, ILF_FROMFIELDS.Split(','), C.Incident.EntityName, toId.Id, ILF_TOFIELDS.Split(','));
                                break;
                            }
                        case C.CaseActivity.CaseActivityTypeEnum.HearingPreperation:
                            {
                                rollupFieldsLogic.RollupToEntity(C.CaseActivity.EntityName, fromId, HEARINGPREP_FROMFIELDS.Split(','), C.Incident.EntityName, toId.Id, HEARINGPREP_TOFIELDS.Split(','));
                                break;
                            }
                    }

                    count++;
                }

                System.Console.WriteLine($"{count} Case Activities Rolled Up!");
            }
        }

        private static List<Entity> RetrieveAllRelevantCaseActivities(CrmServiceClient service, Log4NetTracing log4NetTracing)
        {
            QueryExpression query = new QueryExpression(C.CaseActivity.EntityName);
            query.ColumnSet = new ColumnSet(C.CaseActivity.CaseActivityType, C.CaseActivity.CaseId);
            query.Criteria = new FilterExpression(LogicalOperator.Or);
            query.Criteria.AddCondition(new ConditionExpression(C.CaseActivity.CaseActivityType, ConditionOperator.Equal, (int)C.CaseActivity.CaseActivityTypeEnum.Hearing));
            query.Criteria.AddCondition(new ConditionExpression(C.CaseActivity.CaseActivityType, ConditionOperator.Equal, (int)C.CaseActivity.CaseActivityTypeEnum.HearingPreperation));
            query.Criteria.AddCondition(new ConditionExpression(C.CaseActivity.CaseActivityType, ConditionOperator.Equal, (int)C.CaseActivity.CaseActivityTypeEnum.NoticeOfHearing));
            query.Criteria.AddCondition(new ConditionExpression(C.CaseActivity.CaseActivityType, ConditionOperator.Equal, (int)C.CaseActivity.CaseActivityTypeEnum.InstructingLegalFirm));

            var collection = service.RetrieveMultiple(query);

            if (collection != null && collection.Entities.Count > 0)
            {
                return collection.Entities.ToList();
            }

            return null;
        }

        private static void EnsureFullSync(Log4NetTracing tracing)
        {
            System.Console.WriteLine("Initiating Full Sync of Portal Time Off Requests");
            var service = Connect();

            System.Console.WriteLine("Collecting Bookable Resources for Processing!");

            var entities = GetBookableResources(service);


            System.Console.WriteLine($"Retrieved {(entities != null ? entities.Count : 0)} to Process");

            var from = new DateTime(2022, 1, 1);
            var to = new DateTime(2023, 12, 31);

            if (entities != null)
            {
                var count = entities.Count();
                CalendarBusinessLogic calLogic = new CalendarBusinessLogic(service, tracing);
                PortalTimeOffRequestLogic logic = new PortalTimeOffRequestLogic(service, tracing, calLogic);
                System.Console.WriteLine($"Processing entities between {from} and {to}");
                foreach (Entity entity in entities)
                {
                    ProcessEntity(service, tracing, entity, logic, from, to);
                    count--;
                    System.Console.WriteLine($"{count} left to process.");
                }
            }
            else
            {
                System.Console.WriteLine("No bookable resources to process!");
            }
        }

        private static void ProcessEntity(IOrganizationService service, ITracingService tracing, Entity entity, PortalTimeOffRequestLogic logic, DateTime from, DateTime to)
        {
            try
            {
                System.Console.Write($"Processing BookableResource - Id {entity.ToEntityReference().Id} ... --- ");
                var log = logic.EnsureFullSyncOfPortalTimeOffRequestsWithBookableResourceCalendar(entity.ToEntityReference(), from, to);
                CreateLogEntity(service, entity, log, from, to);
                Entity entityToUpdate = new Entity(C.BookableResource.EntityName, entity.Id);
                entityToUpdate[C.BookableResource.FullSyncScript] = true;
                service.Update(entityToUpdate);
                System.Console.Write("Finished");
                System.Console.WriteLine();
            }
            catch (Exception exc)
            {
                tracing.Trace($"ERROR - {exc.Message}");
            }
        }

        private static void CreateLogEntity(IOrganizationService service, Entity entity, FullSyncResults log, DateTime from, DateTime to)
        {
            Entity entityToCreate = new Entity(C.FullSyncPortalTimeOffRequest.EntityName);
            entityToCreate.Attributes[C.FullSyncPortalTimeOffRequest.BookableResource] = entity.ToEntityReference();
            entityToCreate.Attributes[C.FullSyncPortalTimeOffRequest.From] = from;
            entityToCreate.Attributes[C.FullSyncPortalTimeOffRequest.To] = to;
            entityToCreate.Attributes[C.FullSyncPortalTimeOffRequest.Log] = log.ToString();
            entityToCreate.Attributes[C.FullSyncPortalTimeOffRequest.StatusCode] = new OptionSetValue(222750001);

            service.Create(entityToCreate);
        }

        public static List<Entity> GetBookableResources(IOrganizationService service)
        {
            QueryExpression query = new QueryExpression(C.BookableResource.EntityName);
            query.ColumnSet = new ColumnSet(C.BookableResource.CalendarId);
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(new ConditionExpression(C.BookableResource.FullSyncScript, ConditionOperator.NotEqual, true));
            query.Criteria.AddCondition(new ConditionExpression(C.BookableResource.State, ConditionOperator.Equal, 0));

            var collection = service.RetrieveMultiple(query);

            if (collection != null && collection.Entities.Count > 0)
            {
                return collection.Entities.ToList();
            }

            return null;
        }

        public static CrmServiceClient Connect()
        {
            CrmServiceClient service = null;
            string connection = GetConnectionStringFromAppConfig("Connect");
            
            System.Console.WriteLine("Connecting ...");
            service = new CrmServiceClient(connection);
            return service;
        }

        private static string GetConnectionStringFromAppConfig(string name)
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[name].ConnectionString;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public class Log4NetTracing : ITracingService
        {
            private static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Program));

            public Log4NetTracing()
            {
            }

            public void Trace(string format, params object[] args)
            {
                Log.Info(string.Format(format, args));
            }
        }
    }
}
