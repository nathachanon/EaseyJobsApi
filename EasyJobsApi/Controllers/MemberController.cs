
﻿using EasyJobsApi.DTO;
using EasyJobsApi.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http;

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
        public IHttpActionResult Register()
        {
            if (System.Web.HttpContext.Current.Request.Files.AllKeys.Any())
            {
                // Get the uploaded image from the Files collection  
                var httpPostedFile = System.Web.HttpContext.Current.Request.Files["UploadedImage"];
                var post_name = System.Web.HttpContext.Current.Request.Form["name"];
                var post_surname = System.Web.HttpContext.Current.Request.Form["surname"];
                var post_tel = System.Web.HttpContext.Current.Request.Form["tel"];
                var post_email = System.Web.HttpContext.Current.Request.Form["email"];
                var post_password = System.Web.HttpContext.Current.Request.Form["password"];

                if(post_name == null)
                {
                    return Content((HttpStatusCode)422, "name is null");
                }

                if (post_surname == null)
                {
                    return Content((HttpStatusCode)422, "surname is null");
                }

                if (post_tel == null)
                {
                    return Content((HttpStatusCode)422, "tel is null");
                }

                if (post_email == null)
                {
                    return Content((HttpStatusCode)422, "email is null");
                }

                if (post_password == null)
                {
                    return Content((HttpStatusCode)422, "password is null");
                }

                if (httpPostedFile != null)
                {
                    var get_count_member = db.Member.Where(o => o.email == post_email).Count();
                    if(get_count_member > 0)
                    {
                        return Content((HttpStatusCode)422, "Email ถูกใช้งานแล้ว");
                    }
                    else
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

                        Member register = new Member
                        {
                            member_id = Guid.NewGuid(),
                            name = post_name,
                            surname = post_surname,
                            picture = file_name,
                            tel = post_tel,
                            email = post_email,
                            password = Crypto.HashPassword(post_password)
                        };
                        db.Member.Add(register);
                        db.SaveChangesAsync();
                        return Ok(register);
                    }
                }
            }

            return Content((HttpStatusCode)422, "Image is null");
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
        [Route("api/Member/edit")]
        [HttpPost]
        public IHttpActionResult edit([FromBody] editMember req)
        {
            var member = JsonConvert.SerializeObject(req);
            editMember M = JsonConvert.DeserializeObject<editMember>(member);

            var status_update = (from x in db.Member      
                                 where x.member_id == M.member_id
                                 select new
                                 {
                                     my_member = x
                                 });

            foreach (var st in status_update)
            {
                st.my_member.name = M.name;
                st.my_member.surname = M.surname;
                st.my_member.email = M.email;
                st.my_member.tel = M.tel;

            }
            db.SaveChangesAsync();
            return Ok("Edit succes");
        }
    }
}

