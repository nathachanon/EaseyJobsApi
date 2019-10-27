using EasyJobsApi.DTO;
using EasyJobsApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EasyJobsApi.Controllers
{
    public class WorkController : ApiController
    {
        private EasyJobsEntities1 db_local = new EasyJobsEntities1();
        private EasyJobsEntities2 db = new EasyJobsEntities2();

        [Route("api/Work/AddWork")]
        [HttpPost]
        public IHttpActionResult AddWork([FromBody] AddWorkDto req)
        {
            var member = JsonConvert.SerializeObject(req);
            AddWorkDto ADW = JsonConvert.DeserializeObject<AddWorkDto>(member);

            System.Guid gen_location_id = Guid.NewGuid();
            System.Guid gen_status_id = Guid.NewGuid();

            Location addlocation = new Location
            {
                location_id = gen_location_id,
                lat = ADW.lat,
                @long = ADW.@long,
                loc_name = ADW.loc_name
            };

            Status addStatus = new Status
            {
                status_id = gen_status_id,
                status1 = "ว่าง",
            };

            Work addwork = new Work
            {
                work_id = Guid.NewGuid(),
                work_name = ADW.work_name,
                work_desc = ADW.work_desc,
                tel = ADW.tel,
                labor_cost = ADW.labor_cost,
                duration = ADW.duration,
                datetime = ADW.datetime,
                member_id = ADW.member_id,
                status_id = gen_status_id,
                location_id = gen_location_id
            };

            
            db.Status.Add(addStatus);
            db.Location.Add(addlocation);
            db.Work.Add(addwork);
            db.SaveChangesAsync();
            return Ok(addwork);
        }

        [Route("api/allWork")]
        [HttpGet]
        public IQueryable<object> GetAllWork()
        {
            var work = db.Work.Select(s => new WorkDto
            {
                work_id = s.work_id,
                work_name = s.work_name,
                work_desc = s.work_desc,
                labor_cost = s.labor_cost,
                duration = s.duration,
                member_id = s.member_id,
                location_id = s.location_id,
                status_id = s.status_id
            });
            return work;
        }
    }
}
