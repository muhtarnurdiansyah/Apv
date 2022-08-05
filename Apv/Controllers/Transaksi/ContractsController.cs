using Apv.Models;
using Apv.Models.Master;
using Apv.Models.Transaksi;
using Apv.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Apv.Controllers.Transaksi
{
    public class ContractsController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        // GET: Contracts
        public ActionResult Index()
        {
            ContractVM result = new ContractVM();
            result.Vendor = _context.Vendor.Where(x => x.IsDelete == false).OrderBy(x=> x.Nama).ToList();
            result.Bank = _context.Bank.Where(x => x.IsDelete == false).ToList();
            return View(result);
        }
        public JsonResult GetList()
        {
            List<MainDetail> result = new List<MainDetail>();
            var MainDetailId = _context.MainDetail.Where(x => x.IsDelete == false && x.Main.IsDelete == false).ToList().GroupBy(x => x.MainId).Select(x => x.OrderByDescending(z => z.Id).FirstOrDefault().Id).ToList();
            result = _context.MainDetail.Include(x => x.Main.Vendor).Include(x => x.JenisDokumen).Where(x => MainDetailId.Contains(x.Id)).ToList();

            var JsonResult = Json(new { data = result }, JsonRequestBehavior.AllowGet);
            JsonResult.MaxJsonLength = int.MaxValue;
            return JsonResult;
        }
        public JsonResult GetSelectMain()
        {
            var result = _context.MainDetail.Include(x => x.Main.Vendor).Where(x => x.IsDelete == false && x.Main.IsDelete == false && x.JenisDokumenId == 1 && x.Main.IsActive == true).Select(x => new { MainId = x.MainId, Nomor = x.Nomor, Vendor = x.Main.Vendor.Nama }).ToList();

            var JsonResult = Json(result, JsonRequestBehavior.AllowGet);
            JsonResult.MaxJsonLength = int.MaxValue;
            return JsonResult;
        }
        public JsonResult GetListByMain(int MainId)
        {
            List<MainDetail> result = new List<MainDetail>();
            result = _context.MainDetail.Include(x => x.Main.Vendor).Include(x => x.JenisDokumen).Where(x => x.MainId == MainId && x.IsDelete == false).ToList();
            //result = _context.MainDetail.Include(x => x.Main.Vendor).Include(x => x.JenisDokumen).Where(x => x.MainId == MainId && x.IsDelete == false && x.IsActive == true && x.Main.IsActive == true).ToList();

            var JsonResult = Json(new { data = result }, JsonRequestBehavior.AllowGet);
            JsonResult.MaxJsonLength = int.MaxValue;
            return JsonResult;
        }
        public JsonResult GetMainDetailByMainId(int MainId)
        {
            var result = _context.MainDetail.Include(x=> x.Main).Where(x => x.MainId == MainId).OrderByDescending(x=> x.Id).FirstOrDefault();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetByMainDetail(int MainDetailId)
        {
            var result = _context.MainDetail.Include(x => x.Main).SingleOrDefault(x => x.Id == MainDetailId);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Save(MainVM MainVM)
        {
            var result = false;
            Main Main = MainVM.Main;
            if (Main.Id == 0)
            {
                Main.CreateDate = DateTime.Now;
                Main.IsDelete = false;
                _context.Main.Add(Main);
                _context.SaveChanges();
            }
            else
            {
                var Edit = _context.Main.SingleOrDefault(x => x.Id == Main.Id);
                Edit.Uraian = Main.Uraian;
                Edit.VendorId = Main.VendorId;
                _context.Entry(Edit).State = EntityState.Modified;
                _context.SaveChanges();
            }

            string pathfile = null;
            HttpPostedFileBase File = Request.Files[0];
            if (File != null)
            {
                var ext = Path.GetExtension(File.FileName);
                pathfile = Main.Id + "-Kontrak-" + DateTime.Now.ToString("ddMMyyyy", new System.Globalization.CultureInfo("id-ID")) + " - " + string.Format(@"{0}", DateTime.Now.Ticks) + ext;
                string path = Server.MapPath("~/Files/KontrakAddSPK/" + pathfile);
                File.SaveAs(path);
            }

            MainDetail MainDetail = MainVM.MainDetail;
            if (MainDetail.Id == 0)
            {
                MainDetail.CreateDate = DateTime.Now;
                MainDetail.IsDelete = false;
                MainDetail.IsActive = true;
                MainDetail.MainId = Main.Id;
                MainDetail.Path = pathfile;
                _context.MainDetail.Add(MainDetail);
                _context.SaveChanges();

                result = true;
            }
            else
            {
                var Edit2 = _context.MainDetail.SingleOrDefault(x => x.Id == MainDetail.Id);
                Edit2.Index = MainDetail.Index;
                Edit2.JenisDokumenId = MainDetail.JenisDokumenId;
                Edit2.Nomor = MainDetail.Nomor;
                Edit2.DocDate = MainDetail.DocDate;
                Edit2.StartDate = MainDetail.StartDate;
                Edit2.EndDate = MainDetail.EndDate;
                Edit2.TotalNominal = MainDetail.TotalNominal;
                Edit2.TotalTermin = MainDetail.TotalTermin;
                Edit2.NoRek = MainDetail.NoRek;
                Edit2.NamaRek = MainDetail.NamaRek;
                Edit2.BankId = MainDetail.BankId;
                Edit2.Cabang = MainDetail.Cabang;
                Edit2.NPWP = MainDetail.NPWP;
                Edit2.Alamat = MainDetail.Alamat;
                Edit2.UpdateDate = DateTime.Now;
                if (pathfile != null)
                {
                    Edit2.Path = pathfile;
                }
                _context.Entry(Edit2).State = EntityState.Modified;
                _context.SaveChanges();

                result = true;
            }


            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteAdendum(int Id)
        {
            bool result = false;
            if (Id != 0)
            {
                var delete = _context.MainDetail.SingleOrDefault(x => x.Id == Id);
                delete.IsDelete = true;
                delete.DeleteDate = DateTime.Now;
                _context.Entry(delete).State = EntityState.Modified;
                _context.SaveChanges();

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteKontrak(int Id)
        {
            bool result = false;
            if (Id != 0)
            {
                var delete = _context.Main.SingleOrDefault(x => x.Id == Id);
                delete.IsDelete = true;
                delete.DeleteDate = DateTime.Now;
                _context.Entry(delete).State = EntityState.Modified;
                _context.SaveChanges();

                var detail = _context.MainDetail.Where(x => x.IsDelete == false && x.MainId == Id).Select(x => x.Id).ToList();
                foreach (var item in detail)
                {
                    var delete2 = _context.MainDetail.SingleOrDefault(x => x.Id == item);
                    delete2.IsDelete = true;
                    delete2.DeleteDate = DateTime.Now;
                    _context.Entry(delete2).State = EntityState.Modified;
                    _context.SaveChanges();
                }

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ChangeStatusMain(int Id, bool Status)
        {
            bool result = false;
            if (Id != 0)
            {
                var edit = _context.Main.SingleOrDefault(x => x.Id == Id);
                edit.IsActive = Status;
                edit.UpdateDate = DateTime.Now;
                _context.Entry(edit).State = EntityState.Modified;
                _context.SaveChanges();

                if (!Status)
                {
                    var MainDetail = _context.MainDetail.Where(x => x.MainId == Id).ToList();
                    foreach (var item in MainDetail)
                    {
                        item.IsActive = Status;
                        item.UpdateDate = DateTime.Now;
                        _context.Entry(item).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                }

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ChangeStatusDetail(int Id, bool Status)
        {
            bool result = false;
            if (Id != 0)
            {
                var edit = _context.MainDetail.SingleOrDefault(x => x.Id == Id);
                edit.IsActive = Status;
                edit.UpdateDate = DateTime.Now;
                _context.Entry(edit).State = EntityState.Modified;
                _context.SaveChanges();


                var edit2 = _context.Main.SingleOrDefault(x => x.Id == edit.MainId);
                var count = _context.MainDetail.Where(x => x.MainId == edit.MainId && x.IsDelete == false && x.IsActive == true).Count();
                if (count > 0)
                {
                    edit2.IsActive = true;
                    edit2.UpdateDate = DateTime.Now;
                    _context.Entry(edit2).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                else
                {
                    edit2.IsActive = false;
                    edit2.UpdateDate = DateTime.Now;
                    _context.Entry(edit2).State = EntityState.Modified;
                    _context.SaveChanges();
                }

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}