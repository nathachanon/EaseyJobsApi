using EasyJobsApi.DTO;
using EasyJobsApi.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EasyJobsApi.Controllers
{
    public class TestController : ApiController
    {
        private EasyJobsEntities1 db = new EasyJobsEntities1();
        
        [HttpGet]
        public IQueryable<object> Get()
        {
            var test = db.Member.Select(s => new MemberDto {
                member_id = s.member_id,
                name = s.name,
                surname = s.surname,
                picture = s.picture,
                tel  = s.tel,
                email = s.email
            });
            return test;
        }

       
    }
}
