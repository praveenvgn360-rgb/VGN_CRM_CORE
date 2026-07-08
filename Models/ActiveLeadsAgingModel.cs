using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace VGN_CRM_CORE.Models
{
    public class ActiveLeadsAgingModel
    {
        public string Leadtype { get; set; }
        public string ExecutiveId { get; set; }
        public string TeamLeaderId { get; set; }
        public string TeamManagerId { get; set; }
        public string TeamHeadId { get; set; }
        public System.Data.DataSet dsP = new System.Data.DataSet();
    }
}