using EasyJobsApi.DTO;
using EasyJobsApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.UI.WebControls;

namespace EasyJobsApi.Controllers
{
    public class MemberController : ApiController
    {
        private EasyJobsEntities1 db_local = new EasyJobsEntities1();
        private EasyJobsEntities2 db = new EasyJobsEntities2();

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

        [Route("api/Member/Upload")]
        [HttpPost]
        public async Task<IHttpActionResult> Upload()
        {
            if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
            {
                // Get the uploaded image from the Files collection  
                var httpPostedFile = System.Web.HttpContext.Current.Request.Files["UploadedImage"];
                if (httpPostedFile != null)
                {
                    DTO.FileUpload imgupload = new DTO.FileUpload();
                    int length = httpPostedFile.ContentLength;
                    imgupload.imagedata = new byte[length]; //get imagedata  
                    httpPostedFile.InputStream.Read(imgupload.imagedata, 0, length);
                    imgupload.imagename = Path.GetFileName(httpPostedFile.FileName);
                    var file_name = GenerateNewFileName() + Path.GetExtension(httpPostedFile.FileName);
                    var fileSavePath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Member_image"), file_name);
                    // Save the uploaded file to "UploadedFiles" folder  
                    httpPostedFile.SaveAs(fileSavePath);
                    return Ok("Upload Success : " + file_name);
                }
            }
            return Ok("Image is not Uploaded");
        }

        private string GenerateNewFileName(string prefix = "IMG")
        {
            return prefix + "_" + DateTime.UtcNow.ToString("yyyy-MMM-dd_HH-mm-ss");
        }
    }
}
