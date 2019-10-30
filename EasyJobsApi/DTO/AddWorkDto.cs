using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EasyJobsApi.DTO
{
    public class AddWorkDto
    {
        public Guid member_id { get; set; }
        public string work_name { get; set; }
        public string work_desc { get; set; }
        public string tel { get; set; }
        public int labor_cost { get; set; }
        public string duration { get; set; }
        public DateTime datetime { get; set; }
        public float lat { get; set; }
        public float @long { get; set; }
        public string loc_name { get; set; }

    }
}