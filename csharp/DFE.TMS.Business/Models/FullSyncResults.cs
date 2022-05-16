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
        public int NumberOfPortalRequestDuplicated { get; set; }
        public int NumberOfValidTimeOffRequests { get; set; }

    }
}
