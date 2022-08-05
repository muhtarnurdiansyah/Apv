using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Apv.Models;
using Apv.Models.Setting;
using Apv.ViewModels;

namespace Apv.Controllers.Master
{
    public class MasAccountsController : Controller
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
        // GET: MasAccounts
        public ActionResult Index()
        {
            var User = GetUser();
            var result = _context.Users.Include(x => x.Jabatan).Include(x => x.Unit.Kelompok).Include(x => x.StatusPegawai).Where(x => x.PasswordHash != null && !x.NPP.Contains("Other") && x.Id != User.Id).ToList();
            return View(result);
        }
        public ActionResult Edit(string UserId)
        {
            UserRoleVM result = new UserRoleVM();
            result.User = _context.Users.Include(x => x.Jabatan).Include(x => x.Unit.Kelompok).Include(x => x.StatusPegawai).SingleOrDefault(x => x.Id == UserId);
            result.SettingRole = _context.SettingRole.Include(x => x.MappingRole.Jabatan).Include(x => x.MappingRole.Unit.Kelompok).Where(x => x.UserId == UserId && x.IsDelete == false).ToList();
            return View(result);
        }
        public JsonResult Save(SettingRole Setting, MappingRole Mapping)
        {
            bool result = false;
            var Users = GetUser();
            var GetMapping = _context.MappingRole.FirstOrDefault(x => x.JabatanId == Mapping.JabatanId && x.UnitId == Mapping.UnitId);
            if (GetMapping != null)
            {
                Setting.MappingRoleId = GetMapping.Id;
                Setting.CreaterId = Users.Id;
                Setting.CreateDate = DateTime.Now;
                Setting.IsDelete = false;

                _context.SettingRole.Add(Setting);
                _context.SaveChanges();
                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Mapping()
        {
            var result = _context.MappingRole.Include(x => x.Jabatan).Include(x => x.Unit.Kelompok).ToList();
            
            return View(result);
        }
        public JsonResult SaveMapping(MappingRoleVM Item)
        {
            bool result = false;
            if (Item.Id == 0)
            {
                MappingRole MappingRole = new MappingRole();
                MappingRole.JabatanId = Item.JabatanId;
                MappingRole.UnitId = Item.UnitId;
                _context.MappingRole.Add(MappingRole);
                _context.SaveChanges();

                if (Item.RoleId != null)
                {
                    foreach (var item in Item.RoleId)
                    {
                        MappingDetailRole Detail = new MappingDetailRole();
                        Detail.MappingRoleId = MappingRole.Id;
                        Detail.RoleId = item;
                        _context.MappingDetailRole.Add(Detail);
                        _context.SaveChanges();
                    }
                }

                result = true;
            }
            else
            {
                var Mapping = _context.MappingRole.SingleOrDefault(x => x.Id == Item.Id);
                Mapping.JabatanId = Item.JabatanId;
                Mapping.UnitId = Item.UnitId;
                _context.Entry(Mapping).State = EntityState.Modified;
                _context.SaveChanges();

                var delete = _context.MappingDetailRole.Where(x => x.MappingRoleId == Item.Id).ToList();
                _context.MappingDetailRole.RemoveRange(delete);
                _context.SaveChanges();

                if (Item.RoleId != null)
                {
                    foreach (var item in Item.RoleId)
                    {
                        MappingDetailRole Detail = new MappingDetailRole();
                        Detail.MappingRoleId = Item.Id;
                        Detail.RoleId = item;
                        _context.MappingDetailRole.Add(Detail);
                        _context.SaveChanges();
                    }
                }

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetMappingById(int Id)
        {
            var Trans = _context.MappingRole.Include(x => x.Unit.Kelompok).SingleOrDefault(x => x.Id == Id);
            var Detail = _context.MappingDetailRole.Where(x => x.MappingRoleId == Id).ToList();

            return Json(new { Trans = Trans, Detail = Detail }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteMapping(int Id)
        {
            bool result = false;
            if (Id != 0)
            {
                var delete = _context.MappingRole.SingleOrDefault(x => x.Id == Id);
                _context.MappingRole.Remove(delete);
                _context.SaveChanges();

                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetJabatan()
        {
            List<int> JabId = new List<int>() { 1, 10 };
            var result = _context.Jabatan.Where(x => !JabId.Contains(x.Id)).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetKelompok(int JabId)
        {
            var Kelo = _context.MappingRole.Include(x => x.Unit).Where(x => x.JabatanId == JabId).ToList().GroupBy(x => x.Unit.KelompokId).Select(x => x.FirstOrDefault().Unit.KelompokId).ToList();
            var result = _context.Kelompok.Where(x => Kelo.Contains(x.Id)).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetUnit(int KelId, int JabId)
        {
            var UnitId = _context.MappingRole.Where(x => x.JabatanId == JabId && x.Unit.KelompokId == KelId).Select(x => x.UnitId).ToList();
            var result = _context.Unit.Where(x => UnitId.Contains(x.Id)).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}