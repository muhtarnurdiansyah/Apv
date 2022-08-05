using Apv.Models;
using Apv.Models.Master;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Apv.Controllers.Master
{
    public class MasCurrenciesController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        // GET: MasCurrencies
        public ActionResult Index()
        {            
            var result = _context.Currency.Where(x => x.IsDelete == false).ToList();            
            return View(result);
        }
        public JsonResult GetById(int Id)
        {
            var result = _context.Currency.SingleOrDefault(x => x.Id == Id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetByName(string Name)
        {
            var result = _context.Currency.SingleOrDefault(x => x.Nama == Name && x.IsDelete == false);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Save(Currency Currency)
        {
            if (Currency.Id == 0)
            {
                Currency.CreateDate = DateTime.Now;
                _context.Currency.Add(Currency);
            }
            else
            {
                var currency = _context.Currency.SingleOrDefault(x => x.Id == Currency.Id);
                currency.Nama = Currency.Nama;
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
                var delete = _context.Currency.SingleOrDefault(x => x.Id == Id);
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