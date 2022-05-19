using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFE.TMS.Business.Models
{
    public class FullSyncResults
    {
        public int NumberOfPortalRequestsFound { get; set; }
        public int NumberOfPortalRequestsDeactivated { get; set; }
        public int NumberOfTimeOffRequestsDeleted { get; set; }

        public TimeSpan ElapsedTime { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Number of Portal Requests Found : {NumberOfPortalRequestsFound}");
            builder.AppendLine($"Number of Duplicated Portal Requests Deactivated: {NumberOfPortalRequestsDeactivated}");
            builder.AppendLine($"Number of Time Off Requests Deleted: {NumberOfTimeOffRequestsDeleted}");
            builder.AppendLine($"Process Ran for {ElapsedTime.Hours} Hours {ElapsedTime.Minutes} Minutes {ElapsedTime.Seconds} Seconds");

            return builder.ToString();
        }
    }
}
