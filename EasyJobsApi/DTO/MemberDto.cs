using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EasyJobsApi.DTO
{
    public class MemberDto
    {
        public Guid member_id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string picture { get; set; }
        public string tel { get; set; }
        public string email { get; set; }
        
    }
}