using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EasyJobsApi.DTO
{
    public class WorkDto
    {
        public System.Guid work_id { get; set; }
        public string work_name { get; set; }
        public string work_desc { get; set; }
        public int labor_cost { get; set; }
        public string duration { get; set; }
        public System.Guid member_id { get; set; }
        public System.Guid location_id { get; set; }
        public System.Guid status_id { get; set; }
    }
    public class search
    {
        public string name { get; set; }
        public System.Guid member_id { get; set; }

    }
}