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
        static void Main(string[] args)
        {
            Stopwatch stopWatchFullProgram = new Stopwatch();
            stopWatchFullProgram.Start();
            var tracing = new Log4NetTracing();
            try
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
