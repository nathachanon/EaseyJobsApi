using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EasyJobsApi.DTO
{
    public class GetJobDto
    {
        public Guid member_id { get; set; }
        public Guid work_id { get; set; }
    }
}