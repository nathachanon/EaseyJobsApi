using EasyJobsApi.DTO;
using EasyJobsApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Description;

namespace EasyJobsApi.Controllers
{
    public class MemberController : ApiController
    {
        private EasyJobsEntities1 db = new EasyJobsEntities1();

        [Route("api/allMember")]
        [HttpGet]
        public IQueryable<object> GetAllMember()
        {
            var member = db.Member.Select(s => new MemberDto
            {
                member_id = s.member_id,
                name = s.name,
                surname = s.surname,
                picture = s.picture,
                tel = s.tel,
                email = s.email
            });
            return member;
        }

        [Route("api/Member/{id}")]
        [HttpGet]
        public IQueryable<object> GetMemberById(Guid id)
        {
            var member = db.Member.Where(m => m.member_id == id).Select(s => new MemberDto
            {
                member_id = s.member_id,
                name = s.name,
                surname = s.surname,
                picture = s.picture,
                tel = s.tel,
                email = s.email
            });
            return member;
        }

        [Route("api/Member/Register")]
        [HttpPost]
        public IHttpActionResult Register([FromBody] RegisterMemberDto req)
        {
            var member = JsonConvert.SerializeObject(req);
            RegisterMemberDto M = JsonConvert.DeserializeObject<RegisterMemberDto>(member);
            Member register = new Member
            {
                member_id = Guid.NewGuid(),
                name = M.name,
                surname = M.surname,
                picture = M.picture,
                tel = M.tel,
                email = M.email,
                password = Crypto.HashPassword(M.password)
        };
            db.Member.Add(register);
            db.SaveChangesAsync();
            return Ok(register); 
        }

        [Route("api/Member/Login")]
        [HttpPost]
        public IHttpActionResult Login([FromBody] LoginMemberDto req)
        {
            var member = JsonConvert.SerializeObject(req);
            LoginMemberDto M = JsonConvert.DeserializeObject<LoginMemberDto>(member);

            var Login = db.Member.FirstOrDefault(a => a.email == M.email);
            if (Login != null && Crypto.VerifyHashedPassword(Login.password, M.password) == true)
            {
                return Ok(Login);
            }
            else
            {
                 return StatusCode(HttpStatusCode.NoContent);
            }
           
        }
    }
}
