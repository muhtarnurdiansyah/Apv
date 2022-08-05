using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Apv.ViewModels
{

    public class LogUserVM
    {
        public string UserId { get; set; }
        public int LogId { get; set; }

    }
    public class UserPass
    {
        public string UserId { get; set; }
        public string CurPass { get; set; }
        public string NPass { get; set; }
        public string ConPass { get; set; }
    }
    public class NotifVM
    {
        public int Total { get; set; }
        public string Keterangan { get; set; }
        public string Button { get; set; }
    }
}