using Apv.Models;
using Apv.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Apv.Controllers.Dashboard
{
    public class DashConfigsController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        // GET: DashConfigs
        public ActionResult Index()
        {
            List<DashConfigVM> result = new List<DashConfigVM>();

            var user = _context.Users.Where(x => x.PasswordHash != null).Count();
            var currency = _context.Currency.Where(x => x.IsDelete == false).Count();
            var vendor = _context.Vendor.Where(x => x.IsDelete == false).Count();
            var mapping = _context.MappingRole.Count();
            var tax = _context.SubJenisPotongan.Where(x => x.JenisPotonganId == 3 || x.JenisPotonganId == 4).Count();


            result.Add(new DashConfigVM { Judul = "User Account", Count = user, Icon = "user-plus", Controller = "MasAccounts", Method = "Index", Warna = "aqua" });
            result.Add(new DashConfigVM { Judul = "Mapping Role", Count = vendor, Icon = "map", Controller = "MasAccounts", Method = "Mapping", Warna = "maroon" });
            result.Add(new DashConfigVM { Judul = "Currency", Count = currency, Icon = "dollar", Controller = "MasCurrencies", Method = "Index", Warna = "yellow" });
            result.Add(new DashConfigVM { Judul = "Vendor", Count = vendor, Icon = "building", Controller = "MasVendors", Method = "Index", Warna = "blue" });
            result.Add(new DashConfigVM { Judul = "Tax", Count = tax, Icon = "dollar", Controller = "MasTaxs", Method = "Index", Warna = "navy" });


            return View(result);
        }
    }
}