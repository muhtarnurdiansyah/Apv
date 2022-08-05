using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Apv.Models
{
    public class LogUser
    {
        public int Id { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }
        public string SessionID { get; set; }
        public string IPAddress { get; set; }
        public bool IsLogin { get; set; }
        public DateTime LastLogin { get; set; }
    }
}