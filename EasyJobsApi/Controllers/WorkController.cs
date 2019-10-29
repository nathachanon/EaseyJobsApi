using EasyJobsApi.DTO;
using EasyJobsApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace EasyJobsApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
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
            var query = from c in db.Work select c;
            //var work = db.Work.Select(s => new WorkDto
            //{
            //    work_id = s.work_id,
            //    work_name = s.work_name,
            //    work_desc = s.work_desc,
            //    labor_cost = s.labor_cost,
            //    duration = s.duration,
            //    member_id = s.member_id,
            //    location_id = s.location_id,
            //    status_id = s.status_id
            //});
            return query;
        }

        [Route("api/GetWorkDetail")]
        [HttpPost]
        public IHttpActionResult GetWorkDetail([FromBody] WorkDetail req)
        {
            var work = JsonConvert.SerializeObject(req);
            WorkDetail wr = JsonConvert.DeserializeObject<WorkDetail>(work);

            var query = from w in db.Work
                        join l in db.Location on w.location_id equals l.location_id
                        join m in db.Member on w.member_id equals m.member_id
                        where w.work_id == wr.work_id
                        select new
                        {
                             w.work_id,
                             w.work_name,
                             w.work_desc,
                             w.labor_cost,
                             w.tel,
                             w.duration,
                             l.loc_name,
                             l.lat,
                             l.@long,
                             m.name
                        };
         
            return Ok(query);
        }

        [Route("api/Work/GetJob")]
        [HttpPost]
        public IHttpActionResult GetJob([FromBody] GetJobDto req)
        {
            var member = JsonConvert.SerializeObject(req);
            GetJobDto ADW = JsonConvert.DeserializeObject<GetJobDto>(member);

            System.Guid get_id = Guid.NewGuid();
            Getjob addmember_job = new Getjob
            {
                get_id = get_id,
                member_id = ADW.member_id,
                work_id = ADW.work_id
            };
            var get_status = db.Work.Where(o => o.work_id == ADW.work_id).Select(s => s.status_id).ToArray();
            var status_update = (from x in db.Work
                                 join y in db.Status on x.status_id equals y.status_id
                                 where x.work_id == ADW.work_id
                                 select new
                                 {
                                     my_Status = y
                                 });

            foreach(var st in status_update)
            {
                st.my_Status.status1 = "มีผู้รับงานแล้ว";
            }

            db.Getjob.Add(addmember_job);
            db.SaveChangesAsync();
            return Ok(addmember_job);
        }
    }
}
