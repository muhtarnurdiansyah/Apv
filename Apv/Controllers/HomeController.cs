using Apv.Models;
using Apv.Models.Transaksi;
using Apv.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Apv.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        public ActionResult Index()
        {
            List<MainDetail> result = _context.MainDetail.Include(x => x.Main.Vendor).Where(x => x.IsDelete == false && x.Main.IsDelete == false).ToList().GroupBy(x => x.Main.VendorId).Select(x => x.FirstOrDefault()).ToList();

            return View(result);
        }
        public JsonResult Get(int Id)
        {
            List<DashVM> result = new List<DashVM>();

            var main = _context.Main.Where(x => x.VendorId == Id).ToList();
            foreach (var item in main)
            {
                var MainDetail = _context.MainDetail.Include(x => x.JenisDokumen).Where(x => x.MainId == item.Id).ToList();
                var Kontrak = MainDetail.FirstOrDefault(x => x.JenisDokumenId == 1);
                var TotalNominal = MainDetail.Sum(x => x.TotalNominal);
                var MainDetailId = MainDetail.Select(x => x.Id).ToList();
                var GetTotal = _context.TransMainDetail.Where(x => x.Trans.IsDelete == false && MainDetailId.Contains(x.MainDetailId) && x.Trans.StatusId >= 6).Select(x => x.TotalNominal).DefaultIfEmpty(0).Sum();
                var Sisa = TotalNominal - GetTotal;

                DashVM DashVM = new DashVM();
                List<DashItemVM> DashItemVMs = new List<DashItemVM>();

                #region Total
                DashItemVM DashItemVM1 = new DashItemVM();
                DashItemVM1.Count = TotalNominal.ToString("n2");
                DashItemVM1.Judul = "Total";
                DashItemVMs.Add(DashItemVM1);
                #endregion

                #region Terbayar
                DashItemVM DashItemVM2 = new DashItemVM();
                DashItemVM2.Count = GetTotal.ToString("n2");
                DashItemVM2.Judul = "Terbayar";
                DashItemVMs.Add(DashItemVM2);
                #endregion                

                #region Sisa
                DashItemVM DashItemVM3 = new DashItemVM();
                DashItemVM3.Count = Sisa.ToString("n2");
                DashItemVM3.Judul = "Sisa";
                DashItemVMs.Add(DashItemVM3);
                #endregion                

                DashVM.Judul = Kontrak.Nomor;
                DashVM.Judul2 = Kontrak.JenisDokumen.Nama;
                DashVM.Id = item.Id;
                DashVM.Icon = "check";
                if (item.IsActive)
                {
                    DashVM.Warna = "green";
                }
                else
                {
                    DashVM.Warna = "red";
                }
                DashVM.DashItemVM = DashItemVMs;
                result.Add(DashVM);
            }

            #region backup
            //var maindetail = _context.MainDetail.Include(x => x.JenisDokumen).Include(x => x.Main.Vendor).Where(x => x.MainId == Id).ToList();

            //foreach (var item in maindetail)
            //{
            //    var gettotal = _context.TransMainDetail.Where(x => x.Trans.IsDelete == false && x.MainDetailId == item.Id && x.Trans.StatusId >= 7).Select(x => x.TotalNominal).DefaultIfEmpty(0).Sum();

            //    DashVM DashVM = new DashVM();
            //    List<DashItemVM> DashItemVMs = new List<DashItemVM>();

            //    #region Total
            //    DashItemVM DashItemVM1 = new DashItemVM();
            //    DashItemVM1.Count = item.TotalNominal.ToString("n2");
            //    DashItemVM1.Judul = "Total";
            //    DashItemVMs.Add(DashItemVM1);
            //    #endregion

            //    #region Terbayar
            //    DashItemVM DashItemVM2 = new DashItemVM();
            //    DashItemVM2.Count = gettotal.ToString("n2");
            //    DashItemVM2.Judul = "Terbayar";
            //    DashItemVMs.Add(DashItemVM2);
            //    #endregion

            //    var sisa = item.TotalNominal - gettotal;

            //    #region Sisa
            //    DashItemVM DashItemVM3 = new DashItemVM();
            //    DashItemVM3.Count = sisa.ToString("n2");
            //    DashItemVM3.Judul = "Sisa";
            //    DashItemVMs.Add(DashItemVM3);
            //    #endregion

            //    if (item.JenisDokumenId != 3)
            //    {
            //        DashVM.Judul = item.JenisDokumen.Nama;
            //    }
            //    else
            //    {
            //        DashVM.Judul = item.JenisDokumen.Nama + " " + item.Index;
            //    }

            //    DashVM.Judul2 = item.Nomor;

            //    DashVM.Id = item.Id;
            //    DashVM.Icon = "check";
            //    if (item.IsActive)
            //    {
            //        DashVM.Warna = "green";
            //    }
            //    else
            //    {
            //        DashVM.Warna = "red";
            //    }
            //    DashVM.DashItemVM = DashItemVMs;
            //    result.Add(DashVM);
            //}
            #endregion

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Detail(int Id)
        {
            DetailDashVM result = new DetailDashVM();
            result.Id = Id;
            //result.MainDetail = _context.MainDetail.Include(x => x.Main.Vendor).Include(x => x.JenisDokumen).FirstOrDefault(x => x.Id == Id);var MainDetail = _context.MainDetail.Include(x=> x.JenisDokumen).Where(x => x.MainId == item.Id).ToList();
            var MainDetail = _context.MainDetail.Include(x => x.Main.Vendor).Where(x => x.MainId == Id).ToList();
            result.MainDetail = MainDetail.FirstOrDefault(x => x.JenisDokumenId == 1);
            var TotalNominal = MainDetail.Sum(x => x.TotalNominal);
            var MainDetailId = MainDetail.Select(x => x.Id).ToList();

            var gettotal = _context.TransMainDetail.Where(x => x.Trans.IsDelete == false && MainDetailId.Contains(x.MainDetailId) && x.Trans.StatusId >= 6).Select(x => x.TotalNominal).DefaultIfEmpty(0).Sum();
            result.Terbayar = gettotal;

            result.Outstanding = TotalNominal - gettotal;

            return View(result);
        }
        public JsonResult GetList(int Id)
        {
            //decimal SisaNominal = _context.MainDetail.FirstOrDefault(x => x.MainId == Id && x.JenisDokumenId == 1).TotalNominal;
            decimal SisaNominal = 0;
            List<TransViewVM> result = _context.TransMainDetail.Include(x => x.Trans.KodeSurat).
                Where(x => x.Trans.StatusId >= 6 && x.MainDetail.MainId == Id && x.MainDetail.Main.IsDelete == false &&
                x.MainDetail.IsDelete == false && x.TotalNominal > 0).Select(x => new TransViewVM
            {
                Id = x.TransId,
                KodeSurat = x.Trans.KodeSurat,
                MainDetail = x.MainDetail,
                Nomor = x.Trans.Nomor,
                //Uraian = x.FirstOrDefault().Trans.Uraian,
                DocDate = x.Trans.DocDate,
                Termin = x.Trans.Termin,
                TotalNominal = x.Trans.TotalNominal,
                DetailTotalNominal = x.TotalNominal
            }).OrderBy(x => x.MainDetail.Id).ThenBy(x => x.Id).ToList();

            var MaindetailId = 0;
            foreach (var item in result)
            {
                if (MaindetailId == 0 || MaindetailId != item.MainDetail.Id)
                {
                    item.Uraian = item.MainDetail.TotalNominal.ToString("n2");
                    SisaNominal = SisaNominal + item.MainDetail.TotalNominal;
                    MaindetailId = item.MainDetail.Id;
                }
                else
                {
                    item.Uraian = "";
                }
                SisaNominal = SisaNominal - item.DetailTotalNominal;
                item.SisaNominal = SisaNominal;
            }

            var JsonResult = Json(new { data = result }, JsonRequestBehavior.AllowGet);
            JsonResult.MaxJsonLength = int.MaxValue;
            return JsonResult;
        }
        public JsonResult GetDetailById(int TransId)
        {
            TransVM result = new TransVM();
            result.Trans = _context.Trans.FirstOrDefault(x => x.Id == TransId);
            result.TransMainDetail = _context.TransMainDetail.Include(x => x.MainDetail.Main.Vendor).Where(x => x.TransId == TransId).ToList();
            result.TransRekeningKredit = _context.TransRekening.Where(x => x.TransId == TransId && x.IsMain == false && x.IsDebit == false).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}