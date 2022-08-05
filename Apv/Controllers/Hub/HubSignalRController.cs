using Apv.Models;
using Apv.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Apv.Controllers.Hub
{
    public class HubSignalRController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();

        private ApplicationUser GetUser()
        {
            ApplicationUser result = new ApplicationUser();
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());

            result = _context.Users.Include(x => x.Unit).SingleOrDefault(x => x.Id == currentUser.Id);

            return result;
        }
        // GET: HubSignalR

        public JsonResult SendNotif(string UserId)
        {
            DateTime Datenow = DateTime.Now;
            var user = GetUser();
            //tt.triages = db.Triages.ToList().Where(t => tt.painCats.Contains(t.PainCategoryId));
            List<NotifVM> Result = new List<NotifVM>();

            #region Contoh Kirim Notifikasi
            //if (User.IsInRole("Modul DataEntry"))
            //{
            //    #region Batching Notif
            //    var GetTrans = _context.TransTracking.Where(x => x.Trans.IsDelete == false && x.Trans.IsParent == false && x.Trans.StatusId == 1 && EntityFunctions.TruncateTime(x.Trans.CreateDate) == EntityFunctions.TruncateTime(DateTime.Now)).GroupBy(x => x.TransId).Select(x => new { transid = x.FirstOrDefault().TransId, receiverid = x.OrderByDescending(y => y.Id).FirstOrDefault().ReceiverId }).ToList().Where(x => x.receiverid == user.Id).Select(x => x.transid).Count();
            //    var button = "";
            //    if (GetTrans > 0)
            //    {
            //        button = "<li><a href='"+ Url.Action("Index", "BatchPerbaikans") + "'><i class='fa fa-file-o'></i><span>List Perbaikan</span> <small class='badge bg-important Notif'>" + GetTrans + "</small></a></li>";
            //    }
            //    #endregion 
            //    Result.Add(new NotifVM { Total = GetTrans, Keterangan = "BatchPerbaikan", Button = button });
            //    #region Approved Data
            //    int countUpload = 0;
            //    int countInput = 0;
            //    var getTrackApprove = _context.TransTracking.Where(x => (x.Trans.StatusId == 8 || x.Trans.StatusId == 9) && x.Trans.IsDelete == false  && EntityFunctions.TruncateTime(x.Trans.CreateDate) == EntityFunctions.TruncateTime(DateTime.Now)).GroupBy(x => x.TransId).Select(x => new { transid = x.FirstOrDefault().TransId, receiverid = x.OrderByDescending(y => y.Id).FirstOrDefault().ReceiverId }).ToList().Where(x => x.receiverid == user.Id).Select(x => x.transid).ToList();
            //    foreach (var a in getTrackApprove)
            //    {
            //        var getjenisslip = _context.TransDetail.Include(x => x.Slip.OutputSlip).Where(x => x.TransId == a).Select(x => x.Slip.OutputSlipId).FirstOrDefault();
            //        if (getjenisslip == 2)
            //        {
            //            countUpload++;
            //        }
            //        else
            //        {
            //            countInput++;
            //        }
            //    }
            //    var buttonUpload = "";
            //    var buttonInput = "";
            //    if (countUpload > 0)
            //    {
            //        buttonUpload = "<li><a href='"+ Url.Action("Index", "UploadEntries") + "'><i class='fa fa-file-o'></i><span>List Upload</span> <small class='badge bg-important Notif'>" + countUpload + "</small></a></li>";
            //    }
            //    //


            //    if (countInput > 0)
            //    {
            //        buttonInput = "<li><a href='"+ Url.Action("Index", "InputEntries") + "'><i class='fa fa-file-o'></i><span>List Input</span> <small class='badge bg-important Notif'>" + countInput + "</small></a></li>";
            //    }
            //    #endregion
            //    Result.Add(new NotifVM { Total = countUpload, Keterangan = "UploadEntries", Button = buttonUpload });
            //    Result.Add(new NotifVM { Total = countInput, Keterangan = "InputEntries", Button = buttonInput });
            //}
            #endregion

            return Json(Result, JsonRequestBehavior.AllowGet);
        }
    }
}