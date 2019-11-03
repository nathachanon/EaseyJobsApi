using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EasyJobsApi.DTO
{
    public class MemberWorkDto
    {
        public Guid member_id { get; set; }
        public Guid work_id { get; set; }
    }

    public class MemberOnlyDto
    {
        public Guid member_id { get; set; }
    }
    public class editMember
    {
        public Guid member_id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string tel { get; set; }
        public string email { get; set; }


    }
}