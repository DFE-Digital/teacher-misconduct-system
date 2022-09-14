﻿using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.Business.Logic
{
    public class RollupFieldsToEntityLogic
    {
        public ITracingService TracingService { get; set; }
        public IOrganizationService Service { get; set; }

        public RollupFieldsToEntityLogic(
            ITracingService tracingService, 
            IOrganizationService service)
        {
            TracingService = tracingService;
            Service = service;
        }

        public void RollupToEntity(
            string fromEntityLogicalName,
            Guid fromEntityId,
            string[] fromEntityAttributes,
            string toEntityLogicalName,
            Guid toEntityId,
            string[] toEntityAttributes)
        {
            TracingService.Trace($"Rolling up Attributes from {fromEntityLogicalName} to {toEntityLogicalName}");
            FromAndToAttributesAreValid(fromEntityAttributes, toEntityAttributes);

            Entity fromEntity = Service.Retrieve(fromEntityLogicalName, fromEntityId, new Microsoft.Xrm.Sdk.Query.ColumnSet(fromEntityAttributes));

            UpdateToEntity(fromEntity, fromEntityAttributes, toEntityLogicalName, toEntityId, toEntityAttributes);
        }

        private void UpdateToEntity(Entity fromEntity, string[] fromEntityAttributes, string toEntityLogicalName, Guid toEntityId, string[] toEntityAttributes)
        {
            Entity toEntity = new Entity(toEntityLogicalName, toEntityId);

            int attributeLength = fromEntityAttributes.Length;
            for (int p = 0; p < attributeLength; p++)
            {
                TracingService.Trace($"Rolling up field from entity {fromEntity.LogicalName} '{fromEntityAttributes[p]}' to {toEntityLogicalName} {toEntityAttributes[p]}");
                toEntity[toEntityAttributes[p]] = (fromEntity[fromEntityAttributes[p]] == null) ? null : fromEntity[fromEntityAttributes[p]];
            }

            Service.Update(toEntity);
        }

        private void FromAndToAttributesAreValid(string[] fromAttributes, string[] toAttributes)
        {
            if (fromAttributes == null || toAttributes == null)
                throw new Exception("From or To Attributes are null");

            if (fromAttributes.Length != toAttributes.Length)
                throw new Exception("From and To Attributes do not have the same length!!");
        }
    }
}
