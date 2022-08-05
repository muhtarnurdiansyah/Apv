using Apv.Models;
using Apv.Models.Master;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Apv.Controllers.Master
{
    public class MasVendorsController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        // GET: MasVendors
        public ActionResult Index()
        {            
            var result = _context.Vendor.Where(x => x.IsDelete == false).ToList();            
            return View(result);
        }
        public JsonResult GetById(int Id)
        {
            var result = _context.Vendor.SingleOrDefault(x => x.Id == Id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetByName(string Name)
        {
            var result = _context.Vendor.SingleOrDefault(x => x.Nama == Name && x.IsDelete == false);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Save(Vendor Vendor)
        {
            if (Vendor.Id == 0)
            {
                Vendor.CreateDate = DateTime.Now;
                _context.Vendor.Add(Vendor);
            }
            else
            {
                var currency = _context.Vendor.SingleOrDefault(x => x.Id == Vendor.Id);
                currency.Nama = Vendor.Nama;
                currency.UpdateDate = DateTime.Now;
                _context.Entry(currency).State = EntityState.Modified;
            }
            var result = _context.SaveChanges();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Delete(int Id)
        {
            bool result = false;
            if (Id != 0)
            {
                var delete = _context.Vendor.SingleOrDefault(x => x.Id == Id);
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