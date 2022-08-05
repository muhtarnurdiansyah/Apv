using Apv.Models;
using Apv.Models.Transaksi;
using Apv.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using System.Data.SqlClient;
using System.Configuration;
using Apv.Models.Master;

namespace Apv.Controllers.Transaksi
{
    public class InputsController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        SqlConnection _con = new SqlConnection(ConfigurationManager.ConnectionStrings["Eva"].ToString());
        private ApplicationUser GetUser()
        {
            ApplicationUser result = new ApplicationUser();
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());

            result = _context.Users.Include(x => x.Unit).SingleOrDefault(x => x.Id == currentUser.Id);

            return result;
        }
        // GET: Inputs
        public ActionResult Index()
        {
            //var ven = _context.Vendor.ToList();
            //foreach (var item in ven)
            //{
            //    char[] separators = new char[] { ' ' };

            //    string[] subs = item.Nama.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            //    string gelar = ", PT";
            //    bool edit = false;

            //    if (subs[0] == "PT.")
            //    {
            //        gelar = ", PT";
            //        edit = true;
            //    }
            //    else if (subs[0] == "CV.")
            //    {
            //        gelar = ", CV";
            //        edit = true;
            //    }

            //    if (edit)
            //    {
            //        string Nama = "";
            //        for (int i = 1; i < subs.Count(); i++)
            //        {
            //            if (i != 1)
            //            {
            //                Nama += " ";
            //            }
            //            Nama += subs[i];
            //        }
            //        Nama += gelar;

            //        item.Nama = Nama;
            //        _context.Entry(item).State = EntityState.Modified;
            //        _context.SaveChanges();
            //    }


            //}
            return View();
        }
        public JsonResult GetList()
        {
            var User = GetUser();
            List<int> StatusId = new List<int> { 1, 2, 4 };

            var IdTrans = _context.TransTracking.Where(x => x.Trans.IsDelete == false && StatusId.Contains(x.Trans.StatusId) && DbFunctions.TruncateTime(x.Trans.CreateDate) == DbFunctions.TruncateTime(DateTime.Now) && x.ReceiverId == User.Id).Select(x => x.TransId).Distinct();

            List<TransViewVM> result = _context.TransMainDetail.Include(x => x.Trans.Status).Include(x => x.Trans.KodeSurat).Include(x => x.MainDetail.Main.Vendor).Where(x => IdTrans.Contains(x.TransId) && x.MainDetail.Main.IsDelete == false && x.MainDetail.IsDelete == false).GroupBy(x => x.TransId).Select(x => new TransViewVM
            {
                Id = x.FirstOrDefault().TransId,
                KodeSurat = x.FirstOrDefault().Trans.KodeSurat,
                Nomor = x.FirstOrDefault().Trans.Nomor,
                Uraian = x.FirstOrDefault().Trans.Uraian,
                DocDate = x.FirstOrDefault().Trans.DocDate,
                Prestasi = x.FirstOrDefault().Trans.Prestasi,
                Termin = x.FirstOrDefault().Trans.Termin,
                TotalNominal = x.FirstOrDefault().Trans.TotalNominal,
                Status = x.FirstOrDefault().Trans.Status,
                Vendor = x.FirstOrDefault().MainDetail.Main.Vendor
            }).OrderBy(x => x.Status.Id).ThenBy(x => x.Id).ToList();

            var JsonResult = Json(new { data = result }, JsonRequestBehavior.AllowGet);
            JsonResult.MaxJsonLength = int.MaxValue;
            return JsonResult;
        }
        public JsonResult GetSendReceiver()
        {
            List<SelectBootboxVM> result = new List<SelectBootboxVM>();

            List<string> ListUser = new List<string>();
            var User = GetUser();
            var kelompok = _context.Kelompok.SingleOrDefault(x => x.Id == User.Unit.KelompokId);

            List<int> jab1 = new List<int> { 6, 7, 8 };
            List<int> jab2 = new List<int>();
            if (kelompok.WilayahId == 2)
            {
                jab2 = new List<int> { 2, 3 };
            }
            else
            {
                jab2 = new List<int> { 2, 4 };
            }
            ListUser.AddRange(_context.Users.Where(x => jab1.Contains(x.JabatanId) && x.UnitId == User.UnitId).Select(x => x.Id).ToList());
            ListUser.AddRange(_context.Users.Where(x => x.JabatanId == 5 && x.Unit.KelompokId == User.Unit.KelompokId).Select(x => x.Id).ToList());
            ListUser.AddRange(_context.Users.Where(x => jab2.Contains(x.JabatanId)).Select(x => x.Id).ToList());

            var RoleSpvId = _context.Roles.SingleOrDefault(m => m.Name == "Modul Verificator").Id;
            var UserLogin = _context.LogUser.Where(x => x.IsLogin == true && x.User.Roles.Any(y => y.RoleId == RoleSpvId) && ListUser.Contains(x.UserId) && DbFunctions.TruncateTime(x.LastLogin) == DbFunctions.TruncateTime(DateTime.Now)).Include(x => x.User.Jabatan).Select(x => x.User).OrderByDescending(x => x.JabatanId).ThenBy(x => x.Nama).ToList();
            var ListJabatan = _context.Jabatan.ToList();

            result.Add(new SelectBootboxVM { text = "--Choose Supervisor--", value = "" });
            foreach (var item in UserLogin)
            {
                var Jabatan = ListJabatan.SingleOrDefault(x => x.Id == item.JabatanId).Nama;
                result.Add(new SelectBootboxVM { text = Jabatan + " - " + item.Nama, value = item.Id });
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Send(List<int> Ids, string ReceiverId)
        {
            bool result = false;
            var success = 0;
            var fail = 0;
            var User = GetUser();

            foreach (var id in Ids)
            {
                var OldTrack = _context.TransTracking.Where(x => x.TransId == id).OrderByDescending(x => x.Id).FirstOrDefault();
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
                    NewTrack.TransId = id;
                    NewTrack.ReceiveDate = DateTime.Now;
                    NewTrack.ReceiverId = ReceiverId;
                    NewTrack.ReceiverActivity = "send data to";
                    NewTrack.ReceiverIcon = "send";
                    NewTrack.ReceiverColorIcon = "blue";
                    _context.TransTracking.Add(NewTrack);
                    _context.SaveChanges();
                    #endregion

                    #region Edit Trans
                    var trans = _context.Trans.SingleOrDefault(x => x.Id == id);
                    trans.StatusId = 3;
                    _context.Entry(trans).State = EntityState.Modified;
                    _context.SaveChanges();
                    #endregion                    

                    success++;

                    result = true;
                }
                else
                {
                    fail++;
                }
            }

            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>(); //Call NotificationHub
            //context.Clients.User(ReceiverId).SendNotif(ReceiverId, 1); //call batching Hub

            #region Message for View
            var title = "";
            var text = "";
            var type = "";
            if (result)
            {
                if (success > 0 && fail == 0)
                {
                    #region Success
                    title = "Success!";
                    text = "The data has been saved!";
                    type = "success";
                    #endregion
                }
                else if (success > 0 && fail > 0)
                {
                    #region Success
                    title = "Success!";
                    text = success + "data success and " + fail + " data failed to send!";
                    type = "success";
                    #endregion
                }
                else
                {
                    #region Eror
                    title = "Failed!";
                    text = fail + " data couldn't process!";
                    type = "error";
                    #endregion
                }
            }
            else
            {
                #region Eror
                title = "Failed!";
                text = "We couldn't process the data!";
                type = "error";
                #endregion
            }
            #endregion

            return Json(new { title = title, text = text, type = type }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult View(int Id)
        {
            TransSlipsVM result = new TransSlipsVM();
            result.Trans = _context.Trans.FirstOrDefault(x => x.Id == Id);
            result.SettingSlip = _context.SettingSlip.Include(x => x.JenisSlip).ToList();

            return View(result);
        }
        public ActionResult Add()
        {
            AddVM result = new AddVM();
            result.KodeSurat = _context.KodeSurat.Where(x => x.IsActive == true).ToList();
            result.Rekanan = _context.MainDetail.Include(x => x.Main.Vendor).Where(x => x.IsDelete == false && x.Main.IsDelete == false && x.Main.IsActive == true && x.IsActive == true && x.JenisDokumenId == 1).ToList();
            result.PPN = _context.SubJenisPotongan.Where(x => x.JenisPotonganId == 3).ToList();
            result.PPH = _context.SubJenisPotongan.Where(x => x.JenisPotonganId == 4).ToList();
            result.Denda = _context.SubJenisPotongan.FirstOrDefault(x => x.JenisPotonganId == 2);
            result.Bank = _context.Bank.ToList();

            return View(result);
        }
        public ActionResult Edit(int Id)
        {
            AddVM result = new AddVM();
            result.Id = Id;
            result.KodeSurat = _context.KodeSurat.Where(x => x.IsActive == true).ToList();
            result.Rekanan = _context.MainDetail.Include(x => x.Main.Vendor).Where(x => x.IsDelete == false && x.Main.IsDelete == false && x.Main.IsActive == true && x.JenisDokumenId == 1).ToList();
            result.PPN = _context.SubJenisPotongan.Where(x => x.JenisPotonganId == 3).ToList();
            result.PPH = _context.SubJenisPotongan.Where(x => x.JenisPotonganId == 4).ToList();
            result.Bank = _context.Bank.ToList();

            return View(result);
        }
        public JsonResult Auto(string Key)
        {
            var result = _context.NoRekMCOA.Where(x => x.No.ToUpper().StartsWith(Key)).Select(x => x.No).Distinct().ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AutoSelected(string Key, string Key2)
        {
            var cabang = _context.NoRekCabang.Where(x => x.No == Key).OrderByDescending(x => x.Id).FirstOrDefault();
            var mcoa = _context.NoRekMCOA.Where(x => x.No == Key2).OrderByDescending(x => x.Id).FirstOrDefault();

            return Json(new { cabang = cabang, mcoa = mcoa }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetById(int Id)
        {
            TransVM result = new TransVM();
            result.Trans = _context.Trans.Include(x => x.KodeSurat).FirstOrDefault(x => x.Id == Id);
            var TransMainDetail = _context.TransMainDetail.Include(x => x.MainDetail.Main.Vendor).Where(x => x.TransId == Id).ToList();
            result.TransMainDetail = TransMainDetail;

            var MainId = TransMainDetail.FirstOrDefault().MainDetail.MainId;
            result.MainDetail = _context.MainDetail.Include(x => x.JenisDokumen).Where(x => x.MainId == MainId && x.IsDelete == false).ToList();
            result.TransPengadaan = _context.TransPengadaan.Where(x => x.TransId == Id).ToList();
            result.TransPotonganMaterai = _context.TransPotongan.FirstOrDefault(x => x.TransId == Id && x.SubJenisPotongan.JenisPotonganId == 1);
            result.TransPotonganDenda = _context.TransPotongan.FirstOrDefault(x => x.TransId == Id && x.SubJenisPotongan.JenisPotonganId == 2);
            result.TransPotonganPPN = _context.TransPotongan.Include(x => x.SubJenisPotongan).Where(x => x.TransId == Id && x.SubJenisPotongan.JenisPotonganId == 3).ToList();
            result.TransPotonganPPH = _context.TransPotongan.Include(x => x.SubJenisPotongan).Where(x => x.TransId == Id && x.SubJenisPotongan.JenisPotonganId == 4).ToList();
            result.TransRekeningMain = _context.TransRekening.Include(x => x.Bank).FirstOrDefault(x => x.TransId == Id && x.IsMain == true);
            result.TransRekeningDebit = _context.TransRekening.Where(x => x.TransId == Id && x.IsMain == false && x.IsDebit == true).ToList();
            result.TransRekeningKredit = _context.TransRekening.Where(x => x.TransId == Id && x.IsMain == false && x.IsDebit == false).ToList();
            result.TransAttachment = _context.TransAttachment.Include(x => x.SubJenisAttch.JenisAttch).Include(x => x.OutputAttch).Where(x => x.TransId == Id).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UploadDebit(HttpPostedFileBase file)
        {
            List<TransRekeningVM> Datas = new List<TransRekeningVM>();
            var result = false;

            if (file != null)
            {
                if (file.FileName.EndsWith("xlsx") || file.FileName.EndsWith("XLS"))
                {
                    var ext = Path.GetExtension(file.FileName);
                    var pathfile = "Data Debit " + DateTime.Now.ToString("ddMMyyyy ", new System.Globalization.CultureInfo("id-ID")) + " - " + string.Format(@"{0}", DateTime.Now.Ticks) + ext;
                    string path = Server.MapPath("~/Files/Rekening Debit/" + pathfile);
                    file.SaveAs(path);

                    FileInfo existingFile = new FileInfo(path);
                    using (ExcelPackage package = new ExcelPackage(existingFile))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(f => f.View.TabSelected);
                        if (worksheet != null)
                        {
                            var rowCount = worksheet.Dimension.End.Row;

                            for (int i = 2; i <= rowCount; i++)
                            {
                                TransRekeningVM data = new TransRekeningVM();
                                data.Nama = worksheet.Cells[i, 1].Text;
                                data.NoRek = worksheet.Cells[i, 2].Text;
                                data.Nominal = Convert.ToDecimal(worksheet.Cells[i, 3].Value);

                                Datas.Add(data);
                            }

                            System.IO.File.Delete(path);

                            result = true;
                        }
                        else
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                }
            }

            return Json(new { result = result, data = Datas }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Submit(TransVM TransVM)
        {
            bool result = false;
            var Users = GetUser();
            var TransId = 0;

            if (TransVM.Trans.Id == 0)
            {
                #region Add
                Trans trans = new Trans();
                trans = TransVM.Trans;
                trans.StatusId = 1;
                trans.CreateDate = DateTime.Now;
                trans.IsDelete = false;
                _context.Trans.Add(trans);
                _context.SaveChanges();

                #region TransMainDetail
                foreach (var item in TransVM.TransMainDetail)
                {
                    item.TransId = trans.Id;
                    _context.TransMainDetail.Add(item);
                    _context.SaveChanges();
                }
                #endregion

                #region TransPengadaan
                foreach (var item in TransVM.TransPengadaan)
                {
                    item.TransId = trans.Id;
                    _context.TransPengadaan.Add(item);
                    _context.SaveChanges();
                }
                #endregion

                #region TransPotongan
                foreach (var item in TransVM.TransPotongan)
                {
                    item.IsDone = false;
                    item.TransId = trans.Id;
                    _context.TransPotongan.Add(item);
                    _context.SaveChanges();
                }
                #endregion

                #region TransRekening
                foreach (var item in TransVM.TransRekening)
                {
                    item.TransId = trans.Id;
                    _context.TransRekening.Add(item);
                    _context.SaveChanges();
                }
                #endregion

                #region Tracking Transaksi
                var cek = _context.TransTracking.Where(x => x.TransId == trans.Id).Count();
                if (cek == 0)
                {
                    TransTracking Track = new TransTracking();
                    Track.TransId = trans.Id;
                    Track.ReceiveDate = DateTime.Now;
                    Track.ReceiverId = Users.Id;
                    Track.ReceiverActivity = "creating document";
                    Track.ReceiverIcon = "pencil";
                    Track.ReceiverColorIcon = "yellow";
                    _context.TransTracking.Add(Track);
                    _context.SaveChanges();
                }
                #endregion

                result = true;
                TransId = trans.Id;
                #endregion
            }
            else
            {
                #region Edit
                var trans = _context.Trans.SingleOrDefault(x => x.Id == TransVM.Trans.Id);
                if (trans != null)
                {
                    trans.Nomor = TransVM.Trans.Nomor;
                    trans.Uraian = TransVM.Trans.Uraian;
                    trans.DocDate = TransVM.Trans.DocDate;
                    trans.Prestasi = TransVM.Trans.Prestasi;
                    trans.Termin = TransVM.Trans.Termin;
                    trans.TotalNominal = TransVM.Trans.TotalNominal;
                    trans.StatusId = 1;
                    trans.CreateDate = DateTime.Now;
                    trans.IsDelete = false;
                    _context.Entry(trans).State = EntityState.Modified;
                    _context.SaveChanges();

                    #region TransMainDetail
                    #region Remove TransMainDetail
                    var MainDetail = _context.TransMainDetail.Where(x => x.TransId == trans.Id).ToList();
                    if (MainDetail != null)
                    {
                        _context.TransMainDetail.RemoveRange(MainDetail);
                        _context.SaveChanges();
                    }
                    #endregion
                    foreach (var item in TransVM.TransMainDetail)
                    {
                        item.TransId = trans.Id;
                        _context.TransMainDetail.Add(item);
                        _context.SaveChanges();
                    }
                    #endregion

                    #region TransPengadaan
                    #region Remove TransPengadaan
                    var Pengadaan = _context.TransPengadaan.Where(x => x.TransId == trans.Id).ToList();
                    if (Pengadaan != null)
                    {
                        _context.TransPengadaan.RemoveRange(Pengadaan);
                        _context.SaveChanges();
                    }
                    #endregion
                    foreach (var item in TransVM.TransPengadaan)
                    {
                        item.TransId = trans.Id;
                        _context.TransPengadaan.Add(item);
                        _context.SaveChanges();
                    }
                    #endregion

                    #region TransPotongan
                    #region Remove TransPotongan
                    var Potongan = _context.TransPotongan.Where(x => x.TransId == trans.Id).ToList();
                    if (Pengadaan != null)
                    {
                        _context.TransPotongan.RemoveRange(Potongan);
                        _context.SaveChanges();
                    }
                    #endregion
                    foreach (var item in TransVM.TransPotongan)
                    {
                        item.IsDone = false;
                        item.TransId = trans.Id;
                        _context.TransPotongan.Add(item);
                        _context.SaveChanges();
                    }
                    #endregion

                    #region TransRekening
                    #region Remove TransRekening
                    var Rekening = _context.TransRekening.Where(x => x.TransId == trans.Id).ToList();
                    if (Pengadaan != null)
                    {
                        _context.TransRekening.RemoveRange(Rekening);
                        _context.SaveChanges();
                    }
                    #endregion
                    foreach (var item in TransVM.TransRekening)
                    {
                        item.TransId = trans.Id;
                        _context.TransRekening.Add(item);
                        _context.SaveChanges();
                    }
                    #endregion

                    result = true;
                    TransId = trans.Id;
                }
                #endregion
            }

            #region Contekan
            //try
            //{
            //    var getTrans = _context.Trans.Where(x => x.Id == Id).FirstOrDefault();
            //    var AsistenDE = _context.TransTracking.Where(x => x.TransId == Id).OrderByDescending(x => x.Id).FirstOrDefault();

            //    if (getTrans.StatusId == 11)
            //    {
            //        #region Request
            //        #region Trans
            //        getTrans.StatusId = 8;
            //        getTrans.IsDownload = false;
            //        _context.Entry(getTrans).State = EntityState.Modified;
            //        _context.SaveChanges();
            //        #endregion

            //        #region Hanya Untuk Output Upload
            //        var Slips = _context.TransDetail.Include(x => x.Slip).Where(x => x.TransId == Id).Select(x => new { Id = x.SlipId, OutputSlipId = x.Slip.OutputSlipId }).ToList();
            //        if (Slips.FirstOrDefault().OutputSlipId == 2)
            //        {
            //            foreach (var item in Slips)
            //            {
            //                #region Remove Rekon dan TXT                        
            //                var rekon = _context.Rekon.Where(x => x.SlipId == item.Id).FirstOrDefault();
            //                if (rekon != null)
            //                {
            //                    var txt = _context.TXT.Where(x => x.Id == rekon.TXTId).FirstOrDefault();
            //                    _context.TXT.Remove(txt);
            //                    _context.SaveChanges();
            //                }
            //                #endregion

            //                #region Edit Slip
            //                Slip Slip = _context.Slip.Where(x => x.Id == item.Id).FirstOrDefault();
            //                Slip.StatusSlipId = 5;
            //                _context.Entry(Slip).State = EntityState.Modified;
            //                _context.SaveChanges();
            //                #endregion
            //            }

            //            #region Remove TXT Sisa
            //            List<TXT> TXTSisa = _context.TXT.Where(x => x.NoBatch == getTrans.NoBatch && EntityFunctions.TruncateTime(x.CreateDate) == EntityFunctions.TruncateTime(getTrans.CreateDate)).ToList();
            //            _context.TXT.RemoveRange(TXTSisa);
            //            _context.SaveChanges();
            //            #endregion
            //        }

            //        #endregion                    

            //        #region Add 2 New Tracking
            //        TransTracking NewTrack = new TransTracking();
            //        NewTrack.TransId = Id;
            //        NewTrack.ReceiveDate = AsistenDE.SendDate ?? DateTime.Now;
            //        NewTrack.SendDate = DateTime.Now;
            //        NewTrack.ReceiverId = Users.Id;
            //        NewTrack.SenderId = Users.Id;
            //        NewTrack.ReceiverActivity = "request process data to";
            //        NewTrack.ReceiverIcon = "exclamation-circle";
            //        NewTrack.ReceiverColorIcon = "yellow";
            //        _context.TransTracking.Add(NewTrack);
            //        _context.SaveChanges();

            //        TransTracking NewTrack2 = new TransTracking();
            //        NewTrack2.TransId = Id;
            //        NewTrack2.ReceiveDate = DateTime.Now;
            //        NewTrack2.ReceiverId = AsistenDE.SenderId;
            //        NewTrack2.ReceiverActivity = "accept request process data from";
            //        NewTrack2.ReceiverIcon = "check-circle";
            //        NewTrack2.ReceiverColorIcon = "green";
            //        _context.TransTracking.Add(NewTrack2);
            //        _context.SaveChanges();
            //        #endregion

            //        result = true;
            //        #endregion
            //    }
            //    else if (getTrans.StatusId == 7)
            //    {
            //        #region Request Reject
            //        var SlipId = _context.TransDetail.Where(x => x.TransId == Id).Select(x => x.SlipId).ToList();
            //        var AllSlips = _context.Slip.Where(x => SlipId.Contains(x.Id)).ToList();
            //        var RejectSlips = AllSlips.Where(x => x.StatusSlipId == 3).ToList();
            //        var Inputer = _context.TransTracking.Where(x => x.TransId == Id).OrderBy(x => x.Id).FirstOrDefault().ReceiverId;

            //        if (RejectSlips.Count() == AllSlips.Count())
            //        {
            //            #region Reject Batch
            //            #region Slip                        
            //            foreach (var item in AllSlips)
            //            {
            //                var Slip = _context.Slip.SingleOrDefault(x => x.Id == item.Id);
            //                Slip.ApproverSlipId = null;
            //                Slip.ApprovalDate = null;
            //                Slip.StatusSlipId = 4;
            //                Slip.KeteranganReject = Keterangan;
            //                _context.Entry(Slip).State = EntityState.Modified;
            //                _context.SaveChanges();
            //            }
            //            #endregion

            //            #region Trans                        
            //            getTrans.StatusId = 6;
            //            _context.Entry(getTrans).State = EntityState.Modified;
            //            _context.SaveChanges();
            //            #endregion

            //            #region Add 2 New Tracking
            //            TransTracking NewTrack = new TransTracking();
            //            NewTrack.TransId = Id;
            //            NewTrack.ReceiveDate = AsistenDE.SendDate ?? DateTime.Now;
            //            NewTrack.SendDate = DateTime.Now;
            //            NewTrack.ReceiverId = Users.Id;
            //            NewTrack.SenderId = Users.Id;
            //            NewTrack.ReceiverActivity = "request reject data to";
            //            NewTrack.ReceiverIcon = "close";
            //            NewTrack.ReceiverColorIcon = "red";
            //            NewTrack.SenderKeterangan = Keterangan;
            //            _context.TransTracking.Add(NewTrack);
            //            _context.SaveChanges();

            //            TransTracking NewTrack2 = new TransTracking();
            //            NewTrack2.TransId = Id;
            //            NewTrack2.ReceiveDate = DateTime.Now;
            //            NewTrack2.ReceiverId = Inputer;
            //            NewTrack2.ReceiverActivity = "send reject data to";
            //            NewTrack2.ReceiverIcon = "close";
            //            NewTrack2.ReceiverColorIcon = "red";
            //            _context.TransTracking.Add(NewTrack2);
            //            _context.SaveChanges();
            //            #endregion
            //            #endregion
            //        }
            //        else
            //        {
            //            #region Reject Slip
            //            foreach (var item in RejectSlips)
            //            {
            //                #region Delete slip di TransDetail
            //                var delete = _context.TransDetail.FirstOrDefault(x => x.SlipId == item.Id);
            //                _context.TransDetail.Remove(delete);
            //                _context.SaveChanges();
            //                #endregion

            //                #region Slip
            //                var Slip = _context.Slip.SingleOrDefault(x => x.Id == item.Id);
            //                Slip.StatusSlipId = 4;
            //                Slip.ApproverSlipId = null;
            //                Slip.ApprovalDate = null;
            //                Slip.KeteranganReject = Keterangan;
            //                _context.Entry(Slip).State = EntityState.Modified;
            //                _context.SaveChanges();
            //                #endregion

            //            }

            //            #region Trans
            //            var Cek = _context.TransDetail.Where(x => x.TransId == Id && x.Slip.StatusSlipId != 5).Count();
            //            if (Cek == 0)
            //            {
            //                //Semua No Jurnal Telah Terisi
            //                getTrans.StatusId = 10;
            //            }
            //            else
            //            {
            //                getTrans.StatusId = 9;
            //            }
            //            _context.Entry(getTrans).State = EntityState.Modified;
            //            _context.SaveChanges();
            //            #endregion

            //            #region Add 2 New Tracking
            //            TransTracking NewTrack = new TransTracking();
            //            NewTrack.TransId = Id;
            //            NewTrack.ReceiveDate = AsistenDE.SendDate ?? DateTime.Now;
            //            NewTrack.SendDate = DateTime.Now;
            //            NewTrack.ReceiverId = Users.Id;
            //            NewTrack.SenderId = Users.Id;
            //            NewTrack.ReceiverActivity = "request reject data to";
            //            NewTrack.ReceiverIcon = "close";
            //            NewTrack.ReceiverColorIcon = "red";
            //            _context.TransTracking.Add(NewTrack);
            //            _context.SaveChanges();

            //            TransTracking NewTrack2 = new TransTracking();
            //            NewTrack2.TransId = Id;
            //            NewTrack2.ReceiveDate = DateTime.Now;
            //            NewTrack2.ReceiverId = AsistenDE.SenderId;
            //            NewTrack2.ReceiverActivity = "accept request reject data from";
            //            NewTrack2.ReceiverIcon = "check-circle";
            //            NewTrack2.ReceiverColorIcon = "green";
            //            _context.TransTracking.Add(NewTrack2);
            //            _context.SaveChanges();
            //            #endregion
            //            #endregion
            //        }

            //        result = true;

            //        #region Notif di Asisten Inputer
            //        var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>(); //Call NotificationHub
            //        context.Clients.User(Inputer).SendNotif(Inputer, 1); //call batching Hub
            //        #endregion
            //        #endregion
            //    }

            //    #region Notif di Asisten DE
            //    var context2 = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>(); //Call NotificationHub
            //    context2.Clients.User(AsistenDE.SenderId).SendNotif(AsistenDE.SenderId, 1); //call batching Hub
            //    #endregion
            //}
            //catch
            //{
            //    result = false;
            //}
            #endregion

            return Json(new { result = result, Id = TransId }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Add2(int Id)
        {
            TransVM result = new TransVM();
            result.Trans = _context.Trans.Include(x => x.KodeSurat).FirstOrDefault(x => x.Id == Id);
            result.TransMainDetail = _context.TransMainDetail.Include(x => x.MainDetail.Main.Vendor).Include(x => x.MainDetail.JenisDokumen).Where(x => x.TransId == Id).ToList();
            result.TransAttachment = _context.TransAttachment.Where(x => x.TransId == Id).ToList();
            result.SubJenisAttch = _context.SubJenisAttch.ToList();
            result.OutputAttch = _context.OutputAttch.ToList();

            return View(result);
        }
        public JsonResult Submit2(Trans Trans, List<TransAttchVM> TransAttchs)
        {
            bool result = false;
            var Users = GetUser();

            if (Trans.Id != 0)
            {
                #region Edit Trans
                var trans = _context.Trans.Include(x => x.KodeSurat).SingleOrDefault(x => x.Id == Trans.Id);
                trans.NomorReg = Trans.NomorReg;
                trans.NomorCN = Trans.NomorCN;
                trans.NomorCNPPN = Trans.NomorCNPPN;
                trans.NomorPP = Trans.NomorPP;
                _context.Entry(trans).State = EntityState.Modified;
                _context.SaveChanges();
                #endregion

                #region Add Edit Attch
                List<int> IdAttch = new List<int>();
                foreach (var item in TransAttchs)
                {
                    string pathfile = null;
                    HttpPostedFileBase File = Request.Files.Get(item.KeyFile);
                    if (File != null)
                    {
                        var ext = Path.GetExtension(File.FileName);
                        pathfile = Trans.Id + "-" + DateTime.Now.ToString("ddMMyyyy", new System.Globalization.CultureInfo("id-ID")) + " - " + string.Format(@"{0}", DateTime.Now.Ticks) + ext;
                        //string path = Server.MapPath("~/Files/Attachment/" + pathfile);
                        string path = ("C:/KERJAAN/BNI/EVA2/Eva/Files/Attch/" + pathfile);
                        File.SaveAs(path);
                    }

                    TransAttachment TransAttachment = new TransAttachment();
                    if (item.Id != 0)
                    {
                        #region Get for Edit
                        TransAttachment = _context.TransAttachment.FirstOrDefault(x => x.Id == item.Id);
                        #endregion
                    }

                    TransAttachment.DocDate = item.DocDate;
                    TransAttachment.Jumlah = item.Jumlah;
                    TransAttachment.Nomor = item.Nomor;
                    TransAttachment.SubJenisAttchId = item.SubJenisAttchId;
                    TransAttachment.OutputAttchId = item.OutputAttchId;
                    TransAttachment.TransId = Trans.Id;

                    if (item.Id == 0)
                    {
                        #region Add
                        TransAttachment.Path = pathfile;
                        _context.TransAttachment.Add(TransAttachment);
                        #endregion
                    }
                    else
                    {
                        #region Edit
                        #region File Baru
                        if (File != null || item.OutputAttchId == 1 || item.OutputAttchId == 3)
                        {
                            if (TransAttachment.Path != null)
                            {
                                #region File Lama Dihapus
                                string paths = Server.MapPath("~/Files/Attachment/" + TransAttachment.Path);
                                System.IO.File.Delete(paths);
                                #endregion
                            }

                            TransAttachment.Path = pathfile;
                        }
                        #endregion

                        _context.Entry(TransAttachment).State = EntityState.Modified;
                        #endregion
                    }

                    _context.SaveChanges();
                    IdAttch.Add(TransAttachment.Id);
                }
                #endregion

                #region Hapus data Attch
                var delete = _context.TransAttachment.Where(x => x.TransId == Trans.Id && !IdAttch.Contains(x.Id)).ToList();
                if (delete.Count > 0)
                {
                    foreach (var item in delete)
                    {
                        if (item.Path != null)
                        {
                            string paths = Server.MapPath("~/Files/Attachment/" + item.Path);
                            System.IO.File.Delete(paths);
                        }
                    }

                    _context.TransAttachment.RemoveRange(delete);
                    _context.SaveChanges();
                }
                #endregion                

                #region Hapus data Slip
                var delete2 = _context.TransSlip.Where(x => x.TransId == Trans.Id).ToList();
                if (delete2.Count > 0)
                {
                    _context.TransSlip.RemoveRange(delete2);
                    _context.SaveChanges();
                }
                #endregion

                #region Add Slip
                List<TransSlip> Slips = new List<TransSlip>();
                var rekening = _context.TransRekening.Where(x => x.TransId == Trans.Id).ToList();

                var mainRek = rekening.FirstOrDefault(x => x.IsMain == true);
                var debitRek = rekening.Where(x => x.IsMain == false && x.IsDebit == true).ToList();
                var kreditfirstRek = rekening.Where(x => x.IsMain == false && x.IsDebit == false).OrderBy(x => x.Id).FirstOrDefault();
                var kreditRek = rekening.Where(x => x.IsMain == false && x.IsDebit == false && x.Id != kreditfirstRek.Id).ToList();

                #region Slip Tampungan - dari Semua Debit ke Satu Norek Tampungan
                foreach (var item in debitRek)
                {
                    TransSlip Slip = new TransSlip();
                    Slip.NoRekDebit = item.NoRek;
                    Slip.NamaRekDebit = item.Nama;
                    Slip.NominalDebit = item.Nominal;
                    Slip.CurrencyDebitId = item.CurrencyId;
                    Slip.JenisRekeningDebitId = 3;
                    Slip.IsNoRekDebitVA = false;
                    Slip.NoRekKredit = kreditfirstRek.NoRek;
                    Slip.NamaRekKredit = kreditfirstRek.Nama;
                    Slip.JenisRekeningKreditId = 3;
                    Slip.JenisSlipId = 1;
                    Slip.OutputSlipId = 2;
                    Slip.IsNoRekKreditVA = false;
                    Slip.Keterangan1 = trans.Uraian;
                    Slip.Keterangan2 = trans.KodeSurat.Nama + "" + trans.Nomor + " Tgl. " + trans.DocDate.ToString("dd/MM/yyyy");
                    Slip.Keterangan3 = "PYN/760/" + trans.DocDate.ToString("MMyyyy") + "/" + trans.NomorReg + " PP : " + trans.DocDate.ToString("yyyy") + "/OPR/4.6/" + trans.NomorPP;
                    Slip.TransId = Trans.Id;
                    Slip.Tanggal = DateTime.Now;

                    Slips.Add(Slip);
                }
                #endregion

                #region Slip dari Tampungan ke Kredit masing-masing
                foreach (var item in kreditRek)
                {
                    if (item.Nominal > 30000)
                    {
                        TransSlip Slip = new TransSlip();
                        Slip.NoRekDebit = kreditfirstRek.NoRek;
                        Slip.NamaRekDebit = kreditfirstRek.Nama;
                        Slip.NominalDebit = item.Nominal;
                        Slip.CurrencyDebitId = item.CurrencyId;
                        Slip.JenisRekeningDebitId = 3;
                        Slip.IsNoRekDebitVA = false;
                        Slip.NoRekKredit = item.NoRek;
                        Slip.NamaRekKredit = item.Nama;
                        Slip.JenisRekeningKreditId = 3;
                        Slip.JenisSlipId = 1;
                        Slip.OutputSlipId = 2;
                        Slip.IsNoRekKreditVA = false;
                        Slip.Keterangan1 = trans.Uraian;
                        Slip.Keterangan2 = trans.KodeSurat.Nama + "" + trans.Nomor + " Tgl. " + trans.DocDate.ToString("dd/MM/yyyy");
                        Slip.Keterangan3 = "PYN/760/"+ trans.DocDate.ToString("MMyyyy") +"/"+ trans.NomorReg + " PP : " + trans.DocDate.ToString("yyyy") + "/OPR/4.6/" + trans.NomorPP;
                        Slip.TransId = Trans.Id;
                        Slip.Tanggal = DateTime.Now;

                        Slips.Add(Slip);

                        #region Khusus PPN WAPU
                        var Norek2 = _context.SubJenisPotongan.FirstOrDefault(x => x.NoRek == item.NoRek && x.JenisPotonganId == 3 && x.NoRek2 != null);
                        if (Norek2 != null)
                        {
                            TransSlip Slip2 = new TransSlip();
                            Slip2.NoRekDebit = item.NoRek;
                            Slip2.NamaRekDebit = item.Nama;
                            Slip2.NominalDebit = item.Nominal;
                            Slip2.CurrencyDebitId = item.CurrencyId;
                            Slip2.JenisRekeningDebitId = 3;
                            Slip2.IsNoRekDebitVA = false;
                            Slip2.NoRekKredit = Norek2.NoRek2;
                            Slip2.NamaRekKredit = "GIRO INTERNAL LAINNYA";
                            Slip2.JenisRekeningKreditId = 1;
                            Slip2.JenisSlipId = 1;
                            Slip2.OutputSlipId = 2;
                            Slip2.IsNoRekKreditVA = false;
                            Slip2.Keterangan1 = trans.Uraian;
                            Slip2.Keterangan2 = trans.Nomor + " Tgl. " + trans.DocDate.ToString("dd/MM/yyyy");
                            Slip2.Keterangan3 = "PYN/760/" + trans.DocDate.ToString("MMyyyy") + "/" + trans.NomorReg + " CN : " + trans.DocDate.ToString("yyyy") + "/OPR/4.6/" + trans.NomorCNPPN;
                            Slip2.TransId = Trans.Id;
                            Slip2.Tanggal = DateTime.Now;

                            Slips.Add(Slip2);
                        }
                        #endregion
                    }
                }
                #endregion

                #region Slip Main
                TransSlip TransSlip = new TransSlip();
                TransSlip.NoRekDebit = kreditfirstRek.NoRek;
                TransSlip.NamaRekDebit = kreditfirstRek.Nama;
                TransSlip.NominalDebit = kreditfirstRek.Nominal;
                TransSlip.CurrencyDebitId = kreditfirstRek.CurrencyId;
                TransSlip.IsNoRekDebitVA = false;
                TransSlip.IsNoRekKreditVA = false;
                TransSlip.Keterangan1 = trans.Uraian;
                TransSlip.Keterangan2 = trans.KodeSurat.Nama + "" + trans.Nomor + " Tgl. " + trans.DocDate.ToString("dd/MM/yyyy");
                TransSlip.Keterangan3 = "PYN/760/" + trans.DocDate.ToString("MMyyyy") + "/" + trans.NomorReg + " CN : " + trans.DocDate.ToString("yyyy") + "/OPR/4.6/" + trans.NomorCN;
                TransSlip.TransId = Trans.Id;
                TransSlip.Tanggal = DateTime.Now;
                TransSlip.JenisRekeningDebitId = 3;
                TransSlip.JenisRekeningKreditId = 1;
                TransSlip.JenisSlipId = 1;
                TransSlip.OutputSlipId = 2;

                if (mainRek.BankId == 3)
                {
                    #region Langsung ke rekening sebenarnya
                    TransSlip.NoRekKredit = mainRek.NoRek;
                    TransSlip.NamaRekKredit = mainRek.Nama;

                    Slips.Add(TransSlip);
                    #endregion
                }
                else
                {
                    #region Masuk ke rekening penampung SS Phone Plus
                    TransSlip.NoRekKredit = "SS PHONE PLUS";
                    TransSlip.NamaRekKredit = "7600001014";

                    Slips.Add(TransSlip);


                    TransSlip TransSlip2 = new TransSlip();
                    TransSlip2.NoRekDebit = "7600001014";
                    TransSlip2.NamaRekDebit = "SS PHONE PLUS";
                    TransSlip2.NominalDebit = kreditfirstRek.Nominal;
                    TransSlip2.CurrencyDebitId = kreditfirstRek.CurrencyId;
                    TransSlip2.IsNoRekDebitVA = false;
                    TransSlip2.NoRekKredit = mainRek.NoRek;
                    TransSlip2.NamaRekKredit = mainRek.Nama;
                    TransSlip2.BankKreditId = mainRek.BankId;
                    TransSlip2.OutputSlipId = 1;
                    TransSlip2.IsNoRekKreditVA = false;
                    TransSlip2.TransId = Trans.Id;
                    TransSlip2.Tanggal = DateTime.Now;
                    if (kreditfirstRek.Nominal < 100000000)
                    {
                        #region Kliring
                        TransSlip2.JenisSlipId = 3;
                        TransSlip2.PesanDebit = trans.Nomor + " Tgl. " + trans.DocDate.ToString("dd/MM/yyyy");
                        #endregion
                    }
                    else
                    {
                        #region RTGS
                        TransSlip2.JenisSlipId = 4;
                        TransSlip2.Biaya = 30000;
                        TransSlip2.Keterangan1 = trans.Uraian;
                        TransSlip2.Keterangan2 = trans.Nomor + " Tgl. " + trans.DocDate.ToString("dd/MM/yyyy");
                        #endregion
                    }

                    Slips.Add(TransSlip2);
                    #endregion
                }

                #endregion

                #region Change Keterangan and Output Because Other Pinbuk
                var selainpinbuk = Slips.Where(x => x.JenisSlipId != 1).Count();
                if (selainpinbuk > 0)
                {
                    foreach (var item in Slips)
                    {
                        if (item.JenisSlipId != 3)
                        {
                            item.OutputSlipId = 1;
                            item.Keterangan1 = item.Keterangan2;
                            item.Keterangan2 = item.Keterangan3;
                            item.Keterangan3 = null;
                        }
                    }
                }
                #endregion

                _context.TransSlip.AddRange(Slips);
                _context.SaveChanges();
                #endregion                               

                result = true;
            }

            return Json(new { result = result }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Add3(int Id)
        {
            TransSlipsVM result = new TransSlipsVM();
            result.Trans = _context.Trans.FirstOrDefault(x => x.Id == Id);
            result.SettingSlip = _context.SettingSlip.Include(x => x.JenisSlip).ToList();

            return View(result);
        }
        public ActionResult AddSlip(int Id)
        {
            InputSlipVM result = GetMaster();
            result.Id = Id;
            return View(result);
        }
        public ActionResult EditSlip(int Id)
        {
            InputSlipVM result = GetMaster();
            var slip = _context.TransSlip.SingleOrDefault(x => x.Id == Id);
            result.Id = slip.TransId;
            result.TransSlip = slip;
            result.SettingSlip = _context.SettingSlip.Include(x => x.JenisSlip).Include(x => x.OutputSlip).FirstOrDefault(x => x.OutputSlipId == slip.OutputSlipId && x.JenisSlipId == slip.JenisSlipId);
            return View(result);
        }
        public InputSlipVM GetMaster()
        {
            InputSlipVM result = new InputSlipVM();
            result.Currency = _context.Currency.Where(x => x.IsDelete == false).ToList();
            result.Bank = _context.Bank.Where(x => x.IsDelete == false).ToList();
            result.JenisRekening = _context.JenisRekening.Where(x => x.Id != 4).ToList();
            result.SettingSlips = _context.SettingSlip.Include(x => x.JenisSlip).Where(x => x.OutputSlipId == 1).OrderBy(x => x.JenisSlipId).ToList();
            return result;
        }
        public JsonResult SaveSlip(TransSlip Slip, int TransId)
        {
            var result = false;
            var User = GetUser();

            if (Slip.Id == 0)
            {
                #region Insert
                //Slip.CreateDate = DateTime.Now;
                //Slip.CreaterId = User.Id;
                //Slip.IsDelete = false;
                //Slip.KelompokId = User.Unit.KelompokId;
                if (Slip.JenisSlipId == 1)
                {
                    Slip.OutputSlipId = 2;
                }
                else
                {
                    Slip.OutputSlipId = 1;
                }
                if (Slip.CurrencyDebitId == 0 && Slip.CurrencyKreditId == 0)
                {
                    Slip.CurrencyDebitId = 1;
                }
                //Slip.StatusSlipId = 1;

                _context.TransSlip.Add(Slip);
                #endregion
            }
            else
            {
                #region Edit
                var edit = _context.TransSlip.SingleOrDefault(x => x.Id == Slip.Id);

                edit.Tanggal = Slip.Tanggal;
                edit.NoReferensi = Slip.NoReferensi;
                edit.NamaRekDebit = Slip.NamaRekDebit;
                edit.NamaRekDebit2 = Slip.NamaRekDebit2;
                edit.NoRekDebit = Slip.NoRekDebit;
                edit.NoRekDebit2 = Slip.NoRekDebit2;
                edit.IsNoRekDebitVA = Slip.IsNoRekDebitVA;
                edit.NamaCabangDebit = Slip.NamaCabangDebit;
                edit.JenisRekeningDebitId = Slip.JenisRekeningDebitId;
                edit.PesanDebit = Slip.PesanDebit;
                edit.PesanDebit2 = Slip.PesanDebit2;
                edit.CurrencyDebitId = Slip.CurrencyDebitId;
                edit.NominalDebit = Slip.NominalDebit;

                edit.NamaRekKredit = Slip.NamaRekKredit;
                edit.NoRekKredit = Slip.NoRekKredit;
                edit.NoRekKredit2 = Slip.NoRekKredit2;
                edit.IsNoRekKreditVA = Slip.IsNoRekKreditVA;
                edit.NamaCabangKredit = Slip.NamaCabangKredit;
                edit.JenisRekeningKreditId = Slip.JenisRekeningKreditId;
                edit.BankKreditId = Slip.BankKreditId;
                edit.CurrencyKreditId = Slip.CurrencyKreditId;
                edit.NominalKredit = Slip.NominalKredit;
                edit.AddKredit = Slip.AddKredit;
                edit.AddKredit2 = Slip.AddKredit2;
                edit.PhoneKredit = Slip.PhoneKredit;
                edit.CityCodeKredit = Slip.CityCodeKredit;
                edit.IdKredit = Slip.IdKredit;
                edit.IdTypeKredit = Slip.IdTypeKredit;
                edit.SandiTXN = Slip.SandiTXN;
                edit.Keterangan1 = Slip.Keterangan1;
                edit.Keterangan2 = Slip.Keterangan2;
                edit.Keterangan3 = Slip.Keterangan3;
                edit.Biaya = Slip.Biaya;
                edit.Kurs = Slip.Kurs;
                edit.JenisSlipId = Slip.JenisSlipId;
                _context.Entry(edit).State = EntityState.Modified;
                #endregion
            }
            _context.SaveChanges();

            if (Slip.JenisSlipId != 1)
            {
                #region Ubah semua menjadi slip input
                var transslip = _context.TransSlip.Where(x => x.TransId == TransId).ToList();
                foreach (var item in transslip)
                {
                    item.OutputSlipId = 1;
                    _context.Entry(item).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                #endregion
            }

            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>(); //Call NotificationHub
            //context.Clients.User(User.Id).SendNotif(User.Id, 1);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteSlip(List<int> Ids)
        {
            bool result = false;
            var User = GetUser();
            foreach (var id in Ids)
            {
                var delete = _context.TransSlip.SingleOrDefault(x => x.Id == id);
                _context.TransSlip.Remove(delete);
                _context.SaveChanges();
                result = true;
            }

            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>(); //Call NotificationHub
            //context.Clients.User(User.Id).SendNotif(User.Id, 1); //call CreateSlip
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Submit3(int Id)
        {
            bool result = false;
            var Users = GetUser();

            if (Id != 0)
            {
                #region Edit Trans
                var trans = _context.Trans.Include(x => x.KodeSurat).SingleOrDefault(x => x.Id == Id);

                trans.StatusId = 2;
                _context.Entry(trans).State = EntityState.Modified;
                _context.SaveChanges();
                #endregion                

                #region Tracking Transaksi
                var Track = _context.TransTracking.Where(x => x.TransId == trans.Id).OrderBy(x => x.Id).FirstOrDefault();

                Track.ReceiveDate = DateTime.Now;
                Track.ReceiverId = Users.Id;
                _context.Entry(Track).State = EntityState.Modified;
                _context.SaveChanges();
                #endregion

                result = true;
            }

            return Json(new { result = result }, JsonRequestBehavior.AllowGet);
        }
    }
}