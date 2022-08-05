using Apv.Models;
using Apv.Models.Master;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Apv.ViewModel;

namespace Apv.Controllers.Master
{
    public class MasTaxsController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        // GET: MasTaxs
        public ActionResult Index()
        {
            List<int> JenisPotong = new List<int> { 3, 4 };
            var result = _context.SubJenisPotongan.Include(x => x.JenisPotongan).Where(x => JenisPotong.Contains(x.JenisPotonganId)).ToList();
            return View(result);
        }
       
        public JsonResult GetById(int Id)
        {
            var result = _context.SubJenisPotongan.SingleOrDefault(x => x.Id == Id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetByName(string Name)
        {
            var result = _context.Vendor.SingleOrDefault(x => x.Nama == Name && x.IsDelete == false);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetJenisPajak()
        {
            var result = _context.JenisPotongan.Where(x => x.Id == 3 || x.Id == 4).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }        
        public JsonResult Save(VMmaks data)
        {
            if (data.Id == null)

            {
                SubJenisPotongan sjp = new SubJenisPotongan();
                sjp.Nama = data.Nama;
                sjp.Nama2 = data.Nama2;
                sjp.NoRek = data.NoRek;
                sjp.NoRek2 = data.NoRek2;
                sjp.Nilai = Convert.ToDecimal(data.Nilai);
                sjp.JenisPotonganId = data.JenisPotonganId;
                _context.SubJenisPotongan.Add(sjp);
            }
            else
            {
                var edit = _context.SubJenisPotongan.SingleOrDefault(x => x.Id == data.Id);
                edit.Nama = data.Nama;
                edit.Nama2 = data.Nama2;
                edit.NoRek = data.NoRek;
                edit.NoRek2 = data.NoRek2;
                edit.Nilai = Convert.ToDecimal(data.Nilai);
                edit.JenisPotonganId = data.JenisPotonganId;
                _context.Entry(edit).State = EntityState.Modified;
            }
            var result = _context.SaveChanges();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult Delete(VMmaks data)
        //{
        //    if (data.Id == null)

        //    {
        //        SubJenisPotongan sjp = new SubJenisPotongan();
        //        sjp.Nama = data.Nama;
        //        sjp.Nama2 = data.Nama2;
        //        sjp.NoRek = data.NoRek;
        //        sjp.NoRek2 = data.NoRek2;
        //        sjp.Nilai = Convert.ToDecimal(data.Nilai);
        //        sjp.JenisPotonganId = data.JenisPotonganId;
        //        _context.SubJenisPotongan.Add(sjp);
        //    }
        //    else
        //    {
        //        var edit = _context.SubJenisPotongan.SingleOrDefault(x => x.Id == data.Id);
        //        edit.Nama = data.Nama;
        //        edit.Nama2 = data.Nama2;
        //        edit.NoRek = data.NoRek;
        //        edit.NoRek2 = data.NoRek2;
        //        edit.Nilai = Convert.ToDecimal(data.Nilai);
        //        edit.JenisPotonganId = data.JenisPotonganId;
        //        _context.Entry(edit).State = EntityState.Modified;
        //    }
        //    var result = _context.SaveChanges();

        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult Delete(VMmaks data)
        {
            bool result = false;
            if (data.Id != null)
            {
                var delete = _context.Vendor.SingleOrDefault(x => x.Id == data.Id);
                delete.IsDelete = true;
                delete.DeleteDate = DateTime.Now;
                _context.Entry(delete).State = EntityState.Modified;
                _context.SaveChanges();

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}