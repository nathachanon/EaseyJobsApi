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

        [Route("api/Work/AddWork")] //เพิ่มงาน
        [HttpPost]
        public IHttpActionResult AddWork([FromBody] AddWorkDto req)
        {
            var member = JsonConvert.SerializeObject(req);
            AddWorkDto ADW = JsonConvert.DeserializeObject<AddWorkDto>(member);

            System.Guid gen_location_id = Guid.NewGuid();
            System.Guid gen_status_id = Guid.NewGuid();
            System.Guid gen_work_id = Guid.NewGuid();
            DateTime now = DateTime.Now;

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
                work_id = gen_work_id,
                work_name = ADW.work_name,
                work_desc = ADW.work_desc,
                tel = ADW.tel,
                labor_cost = ADW.labor_cost,
                duration = ADW.duration,
                datetime = now,
                member_id = ADW.member_id,
                status_id = gen_status_id,
                location_id = gen_location_id
            };

            Log addLog = new Log
            {
                log_id = Guid.NewGuid(),
                log_detail = "เพิ่มงาน",
                work_id = gen_work_id,
                member_id = ADW.member_id,
                datetime = now
            };

            db.Log.Add(addLog);
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

        [Route("api/GetWorkDetail")]
        [HttpPost]
        public IHttpActionResult GetWorkDetail([FromBody] WorkDetail req)
        {
            var work = JsonConvert.SerializeObject(req);
            WorkDetail wr = JsonConvert.DeserializeObject<WorkDetail>(work);

            var query = from w in db.Work
                        join l in db.Location on w.location_id equals l.location_id
                        join m in db.Member on w.member_id equals m.member_id
                        join s in db.Status on w.status_id equals s.status_id
                        where w.work_id == wr.work_id
                        select new
                        {
                            m.member_id,
                            w.work_id,
                            w.work_name,
                            w.work_desc,
                            w.labor_cost,
                            w.tel,
                            w.duration,
                            l.loc_name,
                            l.lat,
                            l.@long,
                            m.name,
                            s.status1
                        };

            return Ok(query);
        }

        [Route("api/Work/GetJob")] //รับงาน
        [HttpPost]
        public IHttpActionResult GetJob([FromBody] GetJobDto req)
        {
            var member = JsonConvert.SerializeObject(req);
            GetJobDto ADW = JsonConvert.DeserializeObject<GetJobDto>(member);

            var get_work_status = from w in db.Work
                                  join s in db.Status on w.status_id equals s.status_id into get
                                  where w.work_id == ADW.work_id && w.member_id != ADW.member_id
                                  select get.Where(s => s.status1 == "ว่าง").Count();

            if (get_work_status.FirstOrDefault() > 0)
            {
                DateTime now = DateTime.Now;
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

                foreach (var st in status_update)
                {
                    st.my_Status.status1 = "มีผู้รับงานแล้ว";
                }

                Log addLog = new Log
                {
                    log_id = Guid.NewGuid(),
                    log_detail = "รับงาน",
                    work_id = ADW.work_id,
                    member_id = ADW.member_id,
                    datetime = now
                };

                db.Log.Add(addLog);
                db.Getjob.Add(addmember_job);
                db.SaveChangesAsync();
                return Ok(addmember_job);
            }
            else
            {
                return Content((HttpStatusCode)422, "ไม่พบงานที่รับ หรือ งานถูกรับไปแล้ว");
            }

        }

        [Route("api/allWork_blank")] //งานทั้งหมดที่ว่าง //รับ member_id ถ้าไม่มี จะโชวทั้งหมดที่ว่าง
        [HttpPost]
        public IHttpActionResult GetAllWork_blank([FromBody] MemberOnlyDto req)
        {
            var mw = JsonConvert.SerializeObject(req);
            MemberOnlyDto wr = JsonConvert.DeserializeObject<MemberOnlyDto>(mw);

            if(wr.member_id == null)
            {
                var work_blank = (from x in db.Work
                                  join y in db.Status on x.status_id equals y.status_id
                                  join z in db.Location on x.location_id equals z.location_id
                                  where y.status1 == "ว่าง"
                                  select new
                                  {
                                      work_id = x.work_id,
                                      member_id = x.member_id,
                                      work_name = x.work_name,
                                      work_desc = x.work_desc,
                                      tel = x.tel,
                                      labor_cost = x.labor_cost,
                                      duration = x.duration,
                                      datetime = x.datetime,
                                      status = y.status1,
                                      lat = z.lat,
                                      @long = z.@long,
                                      loc_name = z.loc_name
                                  });
                return Ok(work_blank);
            }
            else
            {
                var work_blank = (from x in db.Work
                                  join y in db.Status on x.status_id equals y.status_id
                                  join z in db.Location on x.location_id equals z.location_id
                                  where y.status1 == "ว่าง" && x.member_id != wr.member_id
                                  select new
                                  {
                                      work_id = x.work_id,
                                      member_id = x.member_id,
                                      work_name = x.work_name,
                                      work_desc = x.work_desc,
                                      tel = x.tel,
                                      labor_cost = x.labor_cost,
                                      duration = x.duration,
                                      datetime = x.datetime,
                                      status = y.status1,
                                      lat = z.lat,
                                      @long = z.@long,
                                      loc_name = z.loc_name
                                  });
                return Ok(work_blank);
            }
        }

        [Route("api/work_post")] //งานที่เราโพส
        [HttpPost]
        public IHttpActionResult GetWork_post([FromBody] MemberOnlyDto req)
        {
            var mw = JsonConvert.SerializeObject(req);
            MemberOnlyDto wr = JsonConvert.DeserializeObject<MemberOnlyDto>(mw);

            var work_post = (from x in db.Work
                             join z in db.Status on x.status_id equals z.status_id
                             where x.member_id == wr.member_id
                             select new 
                             {
                                 work_id = x.work_id,
                                 work_name = x.work_name,
                                 work_desc = x.work_desc,
                                 labor_cost = x.labor_cost,
                                 duration = x.duration,
                                 member_id = x.member_id,
                                 location_id = x.location_id,
                                 status = z.status1,
                                 
                             });

            return Ok(work_post);
        }

        [Route("api/work_get")] //งานที่รับ (ยังไม่ได้ทำงาน แค่รับ)
        [HttpPost]
        public IHttpActionResult GetWork_get([FromBody] MemberOnlyDto req)
        {
            var mw = JsonConvert.SerializeObject(req);
            MemberOnlyDto wr = JsonConvert.DeserializeObject<MemberOnlyDto>(mw);

       
            var work_blank = (from x in db.Work
                              join y in db.Log on x.work_id equals y.work_id
                              join z in db.Status on x.status_id equals z.status_id
                              where y.member_id == wr.member_id && x.work_id == y.work_id
                              
                              select new 
                              {
                                  work_id = x.work_id,
                                  work_name = x.work_name,
                                  work_desc = x.work_desc,
                                  labor_cost = x.labor_cost,
                                  duration = x.duration,
                                  member_id = x.member_id,
                                  location_id = x.location_id,
                                  status = z.status1,
                                
                                  
                              }).Distinct();

            return Ok(work_blank);
        }

        [Route("api/work_start")] //เริ่มงาน (คนลงงาน กดเมื่อเริ่มงาน)
        [HttpPost]
        public IHttpActionResult Work_Start([FromBody] MemberWorkDto req)
        {
            DateTime now = DateTime.Now;

            var mw = JsonConvert.SerializeObject(req);
            MemberWorkDto wr = JsonConvert.DeserializeObject<MemberWorkDto>(mw);
            var get_work_status = from w in db.Work
                                  join s in db.Status on w.status_id equals s.status_id into get
                                  where w.member_id == wr.member_id && w.work_id == wr.work_id
                                  select get.Where(s => s.status1 == "มีผู้รับงานแล้ว").Count();

            if (get_work_status.FirstOrDefault() > 0)
            {
                var status_update = (from x in db.Work
                                     join y in db.Status on x.status_id equals y.status_id
                                     where x.work_id == wr.work_id
                                     select new
                                     {
                                         my_Status = y
                                     });
                var get_worker_id = from a in db.Getjob where a.work_id == wr.work_id select a.member_id;

                foreach (var st in status_update)
                {
                    st.my_Status.status1 = "เริ่มงาน";
                }

                Log addLog = new Log
                {
                    log_id = Guid.NewGuid(),
                    log_detail = "งานที่คุณลง กำลังทำ",
                    work_id = wr.work_id,
                    member_id = wr.member_id,
                    datetime = now
                };

                Log addLog_worker = new Log
                {
                    log_id = Guid.NewGuid(),
                    log_detail = "งานที่คุณรับ กำลังทำ",
                    work_id = wr.work_id,
                    member_id = get_worker_id.First(),
                    datetime = now
                };

                db.Log.Add(addLog);
                db.Log.Add(addLog_worker);
                db.SaveChangesAsync();

                return Ok(addLog);
            }
            else
            {
                return Content((HttpStatusCode)422, "ไม่พบงานที่ต้องเริ่ม หรือ คุณไม่ใช่คนโพสงาน");
            }
        }

        [Route("api/work_finish")] //เสร็จงาน (คนลงงาน กดเมื่องานเสร็จ)
        [HttpPost]
        public IHttpActionResult Work_finish([FromBody] MemberWorkDto req)
        {
            DateTime now = DateTime.Now;

            var mw = JsonConvert.SerializeObject(req);
            MemberWorkDto wr = JsonConvert.DeserializeObject<MemberWorkDto>(mw);
            var get_work_status = from w in db.Work
                                  join s in db.Status on w.status_id equals s.status_id into get
                                  where w.member_id == wr.member_id && w.work_id == wr.work_id
                                  select get.Where(s => s.status1 == "เริ่มงาน").Count();
            if (get_work_status.FirstOrDefault() > 0)
            {
                var status_update = (from x in db.Work
                                     join y in db.Status on x.status_id equals y.status_id
                                     where x.work_id == wr.work_id
                                     select new
                                     {
                                         my_Status = y
                                     });
                var get_worker_id = from a in db.Getjob where a.work_id == wr.work_id select a.member_id;

                foreach (var st in status_update)
                {
                    st.my_Status.status1 = "เสร็จสิ้น";
                }

                Log addLog = new Log
                {
                    log_id = Guid.NewGuid(),
                    log_detail = "งานที่คุณลง เสร็จสิ้นแล้ว",
                    work_id = wr.work_id,
                    member_id = wr.member_id,
                    datetime = now
                };

                Log addLog_worker = new Log
                {
                    log_id = Guid.NewGuid(),
                    log_detail = "งานที่คุณรับ เสร็จสิ้นแล้ว",
                    work_id = wr.work_id,
                    member_id = get_worker_id.First(),
                    datetime = now
                };

                db.Log.Add(addLog);
                db.Log.Add(addLog_worker);
                db.SaveChangesAsync();

                return Ok(addLog);
            }
            else
            {
                return Content((HttpStatusCode)422, "ไม่พบงานที่เสร็จ หรือ คุณไม่ใช่คนโพสงาน");
            }
        }

        [Route("api/work_postfinish")] //งานที่เราโพสที่ทำเสร็จแล้ว
        [HttpPost]
        public IHttpActionResult GetWork_postfinish([FromBody] MemberOnlyDto req)
        {
            var mw = JsonConvert.SerializeObject(req);
            MemberOnlyDto wr = JsonConvert.DeserializeObject<MemberOnlyDto>(mw);

            var work_post = (from x in db.Work
                             join s in db.Status on x.status_id equals s.status_id
                             where x.member_id == wr.member_id && s.status1 == "เสร็จสิ้น"
                             select new WorkDto
                             {
                                 work_id = x.work_id,
                                 work_name = x.work_name,
                                 work_desc = x.work_desc,
                                 labor_cost = x.labor_cost,
                                 duration = x.duration,
                                 member_id = x.member_id,
                                 location_id = x.location_id,
                                 status_id = x.status_id
                             });

            return Ok(work_post);
        }

        [Route("api/work_postprocess")] //งานที่เราโพสที่กำลังทำ
        [HttpPost]
        public IHttpActionResult GetWork_postprocess([FromBody] MemberOnlyDto req)
        {
            var mw = JsonConvert.SerializeObject(req);
            MemberOnlyDto wr = JsonConvert.DeserializeObject<MemberOnlyDto>(mw);

            var work_post = (from x in db.Work
                             join s in db.Status on x.status_id equals s.status_id
                             where x.member_id == wr.member_id && s.status1 == "เริ่มงาน"
                             select new WorkDto
                             {
                                 work_id = x.work_id,
                                 work_name = x.work_name,
                                 work_desc = x.work_desc,
                                 labor_cost = x.labor_cost,
                                 duration = x.duration,
                                 member_id = x.member_id,
                                 location_id = x.location_id,
                                 status_id = x.status_id
                             });

            return Ok(work_post);
        }

        [Route("api/work_getprocess")] //งานที่เรารับที่กำลังทำ
        [HttpPost]
        public IHttpActionResult GetWork_getprocess([FromBody] MemberOnlyDto req)
        {
            var mw = JsonConvert.SerializeObject(req);
            MemberOnlyDto wr = JsonConvert.DeserializeObject<MemberOnlyDto>(mw);

            var work_post = (from x in db.Getjob
                             join w in db.Work on x.work_id equals w.work_id
                             join s in db.Status on w.status_id equals s.status_id
                             where x.member_id == wr.member_id && s.status1 == "เริ่มงาน"
                             select new WorkDto
                             {
                                 work_id = w.work_id,
                                 work_name = w.work_name,
                                 work_desc = w.work_desc,
                                 labor_cost = w.labor_cost,
                                 duration = w.duration,
                                 member_id = w.member_id,
                                 location_id = w.location_id,
                                 status_id = w.status_id
                             });

            return Ok(work_post);
        }

        [Route("api/work_getfinish")] //งานที่เรารับที่ทำเสร็จแล้ว
        [HttpPost]
        public IHttpActionResult GetWork_getfinish([FromBody] MemberOnlyDto req)
        {
            var mw = JsonConvert.SerializeObject(req);
            MemberOnlyDto wr = JsonConvert.DeserializeObject<MemberOnlyDto>(mw);

            var work_post = (from x in db.Getjob
                             join w in db.Work on x.work_id equals w.work_id
                             join s in db.Status on w.status_id equals s.status_id
                             where x.member_id == wr.member_id && s.status1 == "เสร็จสิ้น"
                             select new WorkDto
                             {
                                 work_id = w.work_id,
                                 work_name = w.work_name,
                                 work_desc = w.work_desc,
                                 labor_cost = w.labor_cost,
                                 duration = w.duration,
                                 member_id = w.member_id,
                                 location_id = w.location_id,
                                 status_id = w.status_id
                             });

            return Ok(work_post);
        }

        [Route("api/get_log")] //ดูประวัติในการใช้งานทั้งระบบ
        [HttpPost]
        public IHttpActionResult GetLog([FromBody] MemberOnlyDto req)
        {
            var mw = JsonConvert.SerializeObject(req);
            MemberOnlyDto wr = JsonConvert.DeserializeObject<MemberOnlyDto>(mw);

            var work_post = (from x in db.Log
                             where x.member_id == wr.member_id
                             orderby x.datetime descending
                             select new
                             {
                                 Detail = x.log_detail,
                                 Work_id = x.work_id,
                                 Datetime = x.datetime
                             });

            return Ok(work_post);
        }
        [Route("api/work/search")] //งานทั้งหมดที่ว่าง
        [HttpPost]
        public IHttpActionResult search([FromBody] search req)
        {
            var mw = JsonConvert.SerializeObject(req);
            search wr = JsonConvert.DeserializeObject<search>(mw);
            var work_blank = (from x in db.Work
                              join y in db.Status on x.status_id equals y.status_id
                              join z in db.Location on x.location_id equals z.location_id
                              where y.status1 == "ว่าง" && x.work_name.Contains(wr.name)
                              select new
                              {
                                  work_id = x.work_id,
                                  work_name = x.work_name,
                                  work_desc = x.work_desc,
                                  labor_cost = x.labor_cost,
                                  duration = x.duration,
                                  member_id = x.member_id,
                                  location_id = x.location_id,
                                  status_id = x.status_id,
                                  lat = z.lat,
                                  @long = z.@long,
                              });
            return Ok(work_blank);
        }


        [Route("api/job_count")] // 
        [HttpPost]
        public IHttpActionResult jobcount([FromBody] MemberOnlyDto req)

        {
            var mw = JsonConvert.SerializeObject(req);
            MemberOnlyDto wr = JsonConvert.DeserializeObject<MemberOnlyDto>(mw);


            var jobcount = (from x in db.Getjob
                            where x.member_id == wr.member_id

                            select new
                            {

                            }).Count();

            var workcount = (from x in db.Work
                             where x.member_id == wr.member_id

                             select new
                             {

                             }).Count();

            var work_do = (from x in db.Getjob
                             join w in db.Work on x.work_id equals w.work_id
                             join s in db.Status on w.status_id equals s.status_id
                             where x.member_id == wr.member_id && s.status1 == "เริ่มงาน"
                             select x).Count();

            var work_finish = (from x in db.Getjob
                             join w in db.Work on x.work_id equals w.work_id
                             join s in db.Status on w.status_id equals s.status_id
                             where x.member_id == wr.member_id && s.status1 == "เสร็จสิ้น"
                             select x).Count();

            var data = new { Get = jobcount,
                             Post = workcount,
                             w_do = work_do,
                            finish = work_finish };

            return Ok(data);


        }
    }
}
