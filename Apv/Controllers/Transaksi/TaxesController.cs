using Apv.Models;
using Apv.Models.Transaksi;
using Apv.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Apv.Controllers.Transaksi
{
    public class TaxesController : Controller
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
        // GET: Taxes
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetList()
        {
            var User = GetUser();

            var IdTrans = _context.TransTracking.Where(x => x.Trans.IsDelete == false && x.Trans.StatusId == 5 && DbFunctions.TruncateTime(x.Trans.CreateDate) == DbFunctions.TruncateTime(DateTime.Now)).GroupBy(x => x.TransId).Select(x => new { transid = x.FirstOrDefault().TransId, receiverid = x.OrderByDescending(y => y.Id).FirstOrDefault().ReceiverId }).ToList().Where(x => x.receiverid == User.Id).Select(x => x.transid).ToList();
            
            List<int> JenisPotonganId = new List<int> { 3, 4 };
            //List<TransViewVM> result = _context.TransPotongan.Include(x => x.Trans).Where(x => JenisPotonganId.Contains(x.SubJenisPotongan.JenisPotonganId) && IdTrans.Contains(x.TransId)).Select(x => new TransViewVM
            List < TransViewVM> result = _context.TransPotongan.Include(x => x.Trans.KodeSurat).Include(x => x.SubJenisPotongan).Where(x => JenisPotonganId.Contains(x.SubJenisPotongan.JenisPotonganId) && x.IsDone == false).Select(x => new TransViewVM
            {
                Id = x.Id,
                KodeSurat = x.Trans.KodeSurat,
                Nomor = x.Trans.Nomor,
                Uraian = x.Trans.Uraian,
                DocDate = x.Trans.DocDate,
                TotalNominal = x.Total,
                SubJenisPotongan = x.SubJenisPotongan,
                Vendor = _context.TransMainDetail.FirstOrDefault(z=> z.TransId == x.TransId).MainDetail.Main.Vendor
            }).ToList();

            var JsonResult = Json(new { data = result }, JsonRequestBehavior.AllowGet);
            JsonResult.MaxJsonLength = int.MaxValue;
            return JsonResult;
        }
        public JsonResult Finish(int Id)
        {
            bool result = false;
            var User = GetUser();

            var finish = _context.TransPotongan.SingleOrDefault(x => x.Id == Id);
            finish.IsDone = true;
            _context.Entry(finish).State = EntityState.Modified;
            _context.SaveChanges();

            List<int> JenisPotonganId = new List<int> { 3, 4 };
            var cek = _context.TransPotongan.Where(x => x.TransId == finish.TransId && JenisPotonganId.Contains(x.SubJenisPotongan.JenisPotonganId) && x.IsDone == false).Count();
            if (cek == 0)
            {
                var OldTrack = _context.TransTracking.Where(x => x.TransId == finish.TransId).OrderByDescending(x => x.Id).FirstOrDefault();
                if (OldTrack.ReceiverId == User.Id)
                {
                    #region Edit Tracking Before to Add Sender
                    OldTrack.SendDate = DateTime.Now;
                    OldTrack.SenderId = User.Id;
                    _context.Entry(OldTrack).State = EntityState.Modified;
                    _context.SaveChanges();
                    #endregion

                    #region Add New Tracking for receiver
                    TransTracking NewTrack = new TransTracking();
                    NewTrack.TransId = finish.TransId;
                    NewTrack.ReceiveDate = DateTime.Now;
                    NewTrack.ReceiverId = User.Id;
                    NewTrack.ReceiverActivity = "finish the tax payment";
                    NewTrack.ReceiverIcon = "check-circle";
                    NewTrack.ReceiverColorIcon = "aqua";
                    _context.TransTracking.Add(NewTrack);
                    _context.SaveChanges();
                    #endregion

                    #region Edit Trans
                    var trans = _context.Trans.SingleOrDefault(x => x.Id == finish.TransId);

                    trans.StatusId = 6;
                    _context.Entry(trans).State = EntityState.Modified;
                    _context.SaveChanges();
                    #endregion
                }
            }

            result = true;

            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>(); //Call NotificationHub
            //context.Clients.User(User.Id).SendNotif(User.Id, 1); //call CreateSlip
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}