using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EasyJobsApi.DTO
{
    public class FileUpload
    {
        public int imageid
        {
            get;
            set;
        }
        public string imagename
        {
            get;
            set;
        }
        public byte[] imagedata
        {
            get;
            set;
        }
    }
}