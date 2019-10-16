using EasyJobsApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EasyJobsApi.Controllers
{
    public class ValuesController : ApiController
    {
        private EasyJobsEntities1 db = new EasyJobsEntities1();
        // GET api/values
        public List<Member> Get()
        {
            var test = db.Member;
            return test.ToList();
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
