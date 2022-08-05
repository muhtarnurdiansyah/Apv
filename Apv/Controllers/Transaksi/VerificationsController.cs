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
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Apv.Controllers.Transaksi
{
    public class VerificationsController : Controller
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
        // GET: Verifications
        public ActionResult Index()
        {
            //var c = 1;
            //var a = _con.QuerySingle<int>(@"INSERT INTO [Wilayahs] ([Nama],[Singkatan]) OUTPUT INSERTED.Id VALUES (@Nama, @Singkatan);", new { Nama = "APAaaaaaa", Singkatan = "A" });
            //var a = _con.Query<Status>("EXEC SP_GetStatus @Id", new { Id = c }).ToList();
            //var a = _con.Query<Status>("SELECT * FROM Status WHERE Id = @Id", new { Id = c }).ToList();
            return View();
        }
        public JsonResult GetList()
        {
            var User = GetUser();
            var IdTrans = _context.TransTracking.Where(x => x.Trans.IsDelete == false && x.Trans.StatusId == 3 && DbFunctions.TruncateTime(x.Trans.CreateDate) == DbFunctions.TruncateTime(DateTime.Now)).GroupBy(x => x.TransId).Select(x => new { transid = x.FirstOrDefault().TransId, receiverid = x.OrderByDescending(y => y.Id).FirstOrDefault().ReceiverId }).ToList().Where(x => x.receiverid == User.Id).Select(x => x.transid).ToList();

            //result = _context.TransMainDetail.Include(x => x.Trans.Status).Include(x => x.MainDetail.Main.Vendor).Where(x => x.Trans.IsDelete == false && x.MainDetail.Main.IsDelete == false && x.MainDetail.IsDelete == false && StatusId.Contains(x.Trans.StatusId)).GroupBy(x => x.TransId).Select(x => new TransViewVM
            List<TransViewVM> result = _context.TransMainDetail.Include(x => x.Trans.Status).Include(x => x.Trans.KodeSurat).Include(x => x.MainDetail.Main.Vendor).Where(x => IdTrans.Contains(x.TransId)).GroupBy(x => x.TransId).Select(x => new TransViewVM
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
        public ActionResult View(int Id)
        {
            AddVM result = new AddVM();
            result.Id = Id;
            result.PPN = _context.SubJenisPotongan.Where(x => x.JenisPotonganId == 3).ToList();
            result.PPH = _context.SubJenisPotongan.Where(x => x.JenisPotonganId == 4).ToList();
            result.Bank = _context.Bank.ToList();

            return View(result);
        }
        public ActionResult Verified(int Id)
        {
            TransSlipsVM result = new TransSlipsVM();
            result.Trans = _context.Trans.FirstOrDefault(x => x.Id == Id);
            result.SettingSlip = _context.SettingSlip.Include(x => x.JenisSlip).ToList();

            return View(result);
        }
        public JsonResult GetById(int Id)
        {
            TransVM result = new TransVM();
            result.Trans = _context.Trans.Include(x => x.KodeSurat).FirstOrDefault(x => x.Id == Id);
            var TransMainDetail = _context.TransMainDetail.Include(x => x.MainDetail.Main.Vendor).Include(x => x.MainDetail.JenisDokumen).Where(x => x.TransId == Id).ToList();
            result.TransMainDetailVM = TransMainDetail.Select(x => new TransMainDetailVM
            {
                Id = x.Id,
                MainDetail = x.MainDetail,
                TotalNominal = x.TotalNominal,
                TotalRealisasi = _context.TransMainDetail.Where(z => z.MainDetailId == x.MainDetailId && z.Trans.IsDelete == false && z.Trans.StatusId == 6).Select(z => z.TotalNominal).DefaultIfEmpty().Sum()
            }).ToList();
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
        public JsonResult Approve(int Id, List<TransMainDetail> TransMainDetail)
        {
            bool result = false;
            var User = GetUser();
            var InputerId = _context.TransTracking.Where(x => x.TransId == Id).OrderBy(x => x.Id).FirstOrDefault().ReceiverId;

            if (Id != 0)
            {
                var Loguser = _con.Query<LogUser>("SELECT * FROM LogUsers WHERE IsLogin = @IsLogin AND UserId = @UserId AND cast (LastLogin as date) = @LastLogin ORDER BY Id", new { IsLogin = true, UserId = InputerId, LastLogin = DateTime.Now.ToString("MM/dd/yyyy") }).FirstOrDefault();
                if (Loguser != null)
                {
                    #region TransMainDetail
                    foreach (var item in TransMainDetail)
                    {
                        var edit = _context.TransMainDetail.FirstOrDefault(x => x.Id == item.Id);
                        edit.TotalNominal = item.TotalNominal;
                        _context.Entry(edit).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                    #endregion  

                    List<int> SlipId = new List<int>();
                    int StatusId = 1;
                    #region Cek Penyelia
                    //var Penyelia = _con.Query<LogUser>("SELECT * FROM LogUsers WHERE IsLogin = @IsLogin AND UserId in (SELECT Id FROM AspNetUsers Where UnitId = @UnitId AND JabatanId = @JabatanId) AND cast (LastLogin as date) = @LastLogin ORDER BY Id", new { IsLogin = true, UnitId = User.UnitId, JabatanId = 7, LastLogin = DateTime.Now.ToString("MM/dd/yyyy") }).FirstOrDefault();
                    //if (Penyelia != null)
                    //{
                    //    StatusId = 2;
                    //}
                    #endregion

                    #region Create Slip
                    var Slip = _context.TransSlip.Where(x => x.TransId == Id).ToList();
                    foreach (var item in Slip)
                    {
                        var id = _con.QuerySingle<int>(@"INSERT INTO [dbo].[Slips] ([Tanggal],[NoReferensi],[NamaRekDebit],[NoRekDebit],[NamaCabangDebit],[JenisRekeningDebitId],[PesanDebit],[CurrencyDebitId]
                ,[NominalDebit],[NamaRekKredit],[NoRekKredit],[NamaCabangKredit],[JenisRekeningKreditId],[BankKreditId],[CurrencyKreditId],[NominalKredit],[Keterangan1],[Keterangan2],[Keterangan3]
                ,[JenisSlipId],[OutputSlipId],[KelompokId],[CreaterId],[CreateDate],[IsDelete],[NamaRekDebit2],[PesanDebit2],[AddKredit],[AddKredit2],[NominalOverride]
                ,[PhoneKredit],[CityCodeKredit],[IdKredit],[SandiTXN],[Biaya],[IdTypeKredit],[Kurs],[StatusSlipId],[NoRekDebit2],[IsNoRekDebitVA],[NoRekKredit2],[IsNoRekKreditVA]
                ) OUTPUT INSERTED.Id VALUES (@Tanggal,@NoReferensi,@NamaRekDebit,@NoRekDebit,@NamaCabangDebit,@JenisRekeningDebitId,@PesanDebit,@CurrencyDebitId
                ,@NominalDebit,@NamaRekKredit,@NoRekKredit,@NamaCabangKredit,@JenisRekeningKreditId,@BankKreditId,@CurrencyKreditId,@NominalKredit,@Keterangan1,@Keterangan2,@Keterangan3
                ,@JenisSlipId,@OutputSlipId,@KelompokId,@CreaterId,@CreateDate,@IsDelete,@NamaRekDebit2,@PesanDebit2,@AddKredit,@AddKredit2,@NominalOverride
                ,@PhoneKredit,@CityCodeKredit,@IdKredit,@SandiTXN,@Biaya,@IdTypeKredit,@Kurs,@StatusSlipId,@NoRekDebit2,@IsNoRekDebitVA,@NoRekKredit2,@IsNoRekKreditVA);", new
                        {
                            Tanggal = item.Tanggal,
                            NoReferensi = item.NoReferensi,
                            NamaRekDebit = item.NamaRekDebit,
                            NoRekDebit = item.NoRekDebit,
                            NamaCabangDebit = item.NamaCabangDebit,
                            JenisRekeningDebitId = item.JenisRekeningDebitId,
                            PesanDebit = item.PesanDebit,
                            CurrencyDebitId = item.CurrencyDebitId,
                            NominalDebit = item.NominalDebit,
                            NamaRekKredit = item.NamaRekKredit,
                            NoRekKredit = item.NoRekKredit,
                            NamaCabangKredit = item.NamaCabangKredit,
                            JenisRekeningKreditId = item.JenisRekeningKreditId,
                            BankKreditId = item.BankKreditId,
                            CurrencyKreditId = item.CurrencyKreditId,
                            NominalKredit = item.NominalKredit,
                            Keterangan1 = item.Keterangan1,
                            Keterangan2 = item.Keterangan2,
                            Keterangan3 = item.Keterangan3,
                            JenisSlipId = item.JenisSlipId,
                            OutputSlipId = item.OutputSlipId,
                            KelompokId = User.Unit.KelompokId,
                            CreaterId = InputerId,
                            CreateDate = DateTime.Now,
                            IsDelete = false,
                            NamaRekDebit2 = item.NamaRekDebit2,
                            PesanDebit2 = item.PesanDebit2,
                            AddKredit = item.AddKredit,
                            AddKredit2 = item.AddKredit2,
                            NominalOverride = 0,
                            PhoneKredit = item.PhoneKredit,
                            CityCodeKredit = item.CityCodeKredit,
                            IdKredit = item.IdKredit,
                            SandiTXN = item.SandiTXN,
                            Biaya = item.Biaya,
                            IdTypeKredit = item.IdTypeKredit,
                            Kurs = item.Kurs,
                            StatusSlipId = 1,
                            NoRekDebit2 = item.NoRekDebit2,
                            IsNoRekDebitVA = item.IsNoRekDebitVA,
                            NoRekKredit2 = item.NoRekKredit2,
                            IsNoRekKreditVA = item.IsNoRekKreditVA
                        });

                        SlipId.Add(id);
                    }
                    #endregion

                    #region Create Batch
                    #region No Batch
                    var NoBatch = 1;

                    var transdata = _con.Query<int>("SELECT DISTINCT [NoBatch] FROM Trans WHERE KelompokId = @KelompokId AND IsParent = @IsParent AND cast (CreateDate as date) = @CreateDate ORDER BY NoBatch", new { KelompokId = User.Unit.KelompokId, IsParent = true, CreateDate = DateTime.Now.ToString("MM/dd/yyyy") }).ToList();
                    //var transdata = _context.Trans.Where(x => x.IsDelete == false && x.KelompokId == User.Unit.KelompokId && EntityFunctions.TruncateTime(x.CreateDate) == EntityFunctions.TruncateTime(DateTime.Now) && x.IsParent == true).GroupBy(x => x.NoBatch).Select(x => x.FirstOrDefault().NoBatch).OrderBy(x => x).ToList();
                    if (transdata.Count() > 0)
                    {
                        //Memastikan sudah ada transaksi pada hari itu
                        foreach (var item in transdata)
                        {
                            if (item == NoBatch)
                            {
                                //No batch yang di DB sama seperti looping
                                NoBatch = NoBatch + 1;
                            }
                            else
                            {
                                //Terdapat no batch yang belum terpakai
                                break;
                            }
                        }
                    }
                    #endregion

                    var TransId = _con.QuerySingle<int>(@"INSERT INTO [Trans] ([NoBatch],[KelompokId],[StatusId],[CreateDate],[IsDelete],[IsUrgent],[IsParent],[IsDownload]) OUTPUT INSERTED.Id VALUES (@NoBatch,@KelompokId,@StatusId,@CreateDate,@IsDelete,@IsUrgent,@IsParent,@IsDownload);", new
                    {
                        NoBatch = NoBatch,
                        KelompokId = User.Unit.KelompokId,
                        StatusId = StatusId,
                        CreateDate = DateTime.Now,
                        IsDelete = false,
                        IsUrgent = false,
                        IsParent = true,
                        IsDownload = false
                    });

                    foreach (var item in SlipId)
                    {
                        var SlipIns = _con.QuerySingle<int>(@"INSERT INTO [TransDetails] ([TransId],[SlipId]) OUTPUT INSERTED.Id VALUES (@TransId,@SlipId);", new { TransId = TransId, SlipId = item });
                    }
                    #endregion

                    #region Tracking
                    //if (Penyelia != null)
                    //{
                    //    #region Tracking Ke Penyelia
                    //    var Track = _con.QuerySingle<int>(@"INSERT INTO [TransTrackings] ([TransId],[ReceiveDate],[ReceiverId],[ReceiverActivity],[ReceiverIcon],[ReceiverColorIcon],[SendDate],[SenderId]) OUTPUT INSERTED.Id VALUES (@TransId,@ReceiveDate,@ReceiverId,@ReceiverActivity,@ReceiverIcon,@ReceiverColorIcon,@SendDate,@SenderId);", new
                    //    {
                    //        TransId = TransId,
                    //        ReceiveDate = DateTime.Now,
                    //        ReceiverId = User.Id,
                    //        ReceiverActivity = "creating batch",
                    //        ReceiverIcon = "pencil",
                    //        ReceiverColorIcon = "yellow",
                    //        SendDate = DateTime.Now,
                    //        SenderId = User.Id
                    //    });

                    //    var Track2 = _con.QuerySingle<int>(@"INSERT INTO [TransTrackings] ([TransId],[ReceiveDate],[ReceiverId],[ReceiverActivity],[ReceiverIcon],[ReceiverColorIcon]) OUTPUT INSERTED.Id VALUES (@TransId,@ReceiveDate,@ReceiverId,@ReceiverActivity,@ReceiverIcon,@ReceiverColorIcon);", new
                    //    {
                    //        TransId = TransId,
                    //        ReceiveDate = DateTime.Now,
                    //        ReceiverId = Penyelia.UserId,
                    //        ReceiverActivity = "send data to",
                    //        ReceiverIcon = "send",
                    //        ReceiverColorIcon = "blue"
                    //    });
                    //    #endregion
                    //}
                    //else
                    //{                    
                    //}
                    #endregion

                    #region Tracking hanya di inputer
                    var Track = _con.QuerySingle<int>(@"INSERT INTO [TransTrackings] ([TransId],[ReceiveDate],[ReceiverId],[ReceiverActivity],[ReceiverIcon],[ReceiverColorIcon]) OUTPUT INSERTED.Id VALUES (@TransId,@ReceiveDate,@ReceiverId,@ReceiverActivity,@ReceiverIcon,@ReceiverColorIcon);", new
                    {
                        TransId = TransId,
                        ReceiveDate = DateTime.Now,
                        ReceiverId = InputerId,
                        ReceiverActivity = "creating batch",
                        ReceiverIcon = "pencil",
                        ReceiverColorIcon = "yellow"
                    });
                    #endregion

                    #region Attachment Transaksi
                    var AttachmentLampiranPath = _context.TransAttachment.Include(x => x.SubJenisAttch.JenisAttch).Include(x => x.OutputAttch).Where(x => x.TransId == Id).ToList();

                    string pathfile = TransId + "-" + DateTime.Now.ToString("ddMMyyyy", new System.Globalization.CultureInfo("id-ID")) + " - " + string.Format(@"{0}", DateTime.Now.Ticks) + ".pdf";

                    var Attch = _con.QuerySingle<int>(@"INSERT INTO [TransAttachments] ([TransId],[Path],[CreateDate]) OUTPUT INSERTED.Id VALUES (@TransId,@Path,@CreateDate);", new
                    {
                        TransId = TransId,
                        Path = pathfile,
                        CreateDate = DateTime.Now
                    });

                    if (AttachmentLampiranPath.Count() > 0)
                    {
                        foreach (var item in AttachmentLampiranPath)
                        {
                            //string pathfile2 = TransId + "-" + item.Path;

                            var Attch1 = _con.Query<int>(@"INSERT INTO [TransAttachments] ([TransId],[Path],[CreateDate]) OUTPUT INSERTED.Id VALUES (@TransId,@Path,@CreateDate);", new
                            {
                                TransId = TransId,
                                Path = item.Path,
                                CreateDate = DateTime.Now
                            });

                            //string path = Server.MapPath("Eva2/Eva/Files/Attch/" + item.Path);
                            string path = ("C:/KERJAAN/BNI/EVA2/Eva/Files/Attch/"+ item.Path);
                            //string path = Server.MapPath("/DevEva/Files/Attch/" + item.Path);
                        }
                    }

                    MemoPDF(Id, pathfile);

                    #endregion

                    var OldTrack = _context.TransTracking.Where(x => x.TransId == Id).OrderByDescending(x => x.Id).FirstOrDefault();
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
                        NewTrack.TransId = Id;
                        NewTrack.ReceiveDate = DateTime.Now;
                        NewTrack.ReceiverId = User.Id;
                        NewTrack.ReceiverActivity = "verified and create batch on E-Voucher";
                        NewTrack.ReceiverIcon = "check";
                        NewTrack.ReceiverColorIcon = "green";
                        _context.TransTracking.Add(NewTrack);
                        _context.SaveChanges();
                        #endregion

                        #region Edit Trans
                        var trans = _context.Trans.SingleOrDefault(x => x.Id == Id);
                        trans.StatusId = 5;
                        _context.Entry(trans).State = EntityState.Modified;
                        _context.SaveChanges();
                        #endregion
                    }
                    
                    result = true;
                }
            }

            return Json(new { result = result }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Reject(List<int> Ids, string Keterangan)
        {
            bool result = false;
            var success = 0;
            var fail = 0;
            var User = GetUser();
            var Inputer = "";

            foreach (var id in Ids)
            {
                var OldTrack = _context.TransTracking.Where(x => x.TransId == id).OrderByDescending(x => x.Id).FirstOrDefault();
                if (OldTrack.ReceiverId == User.Id)
                {
                    #region Edit Tracking Before to Add Sender                    
                    OldTrack.SendDate = DateTime.Now;
                    OldTrack.SenderId = User.Id;
                    OldTrack.SenderKeterangan = Keterangan;
                    _context.Entry(OldTrack).State = EntityState.Modified;
                    _context.SaveChanges();
                    #endregion

                    #region Add New Tracking for Receiver
                    TransTracking NewTrack2 = new TransTracking();
                    NewTrack2.TransId = id;
                    NewTrack2.ReceiveDate = DateTime.Now;
                    Inputer = _context.TransTracking.Where(x => x.TransId == id).OrderBy(x => x.Id).FirstOrDefault().ReceiverId;
                    NewTrack2.ReceiverId = Inputer;
                    NewTrack2.ReceiverActivity = "send reject data to";
                    NewTrack2.ReceiverIcon = "close";
                    NewTrack2.ReceiverColorIcon = "red";
                    _context.TransTracking.Add(NewTrack2);
                    _context.SaveChanges();
                    #endregion

                    #region Edit Trans
                    var trans = _context.Trans.SingleOrDefault(x => x.Id == id);
                    trans.StatusId = 4;
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

            #region Message for View
            var title = "";
            var text = "";
            var type = "";
            if (result)
            {
                if (success > 0 && fail == 0)
                {
                    #region Success
                    title = "Reject!";
                    text = "The data has been saved!";
                    type = "success";
                    #endregion
                }
                else if (success > 0 && fail > 0)
                {
                    #region Success
                    title = "Success!";
                    text = success + "data success and " + fail + " data failed to reject!";
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

            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>(); //Call NotificationHub
            //context.Clients.User(Inputer).SendNotif(Inputer, 1); //call batching Hub

            return Json(new { title = title, text = text, type = type }, JsonRequestBehavior.AllowGet);
        }
        public void MemoPDF(int Id, string Path)
        {
            var User = GetUser();

            #region Create PDF Document
            Document pdfDoc = new Document();
            var pathfile1 = /*Server.MapPath("Eva2/Eva/Files/Attch/") + Path;*/ ("C:/KERJAAN/BNI/EVA2/Eva/Files/Attch/"+ Path);
            //var pathfile1 = Server.MapPath("/EVA2/Eva/Files/Attch/") + Path; (hasil pdf keluar)
            //var pathfile1 = Server.MapPath("/DevEva/Files/Attch/") + Path;

            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(pathfile1, FileMode.Create));
            #endregion

            var Trans = _context.Trans.Include(x => x.KodeSurat).SingleOrDefault(x => x.Id == Id);

            if (Trans != null)
            {
                #region Query
                var TransMainDetail = _context.TransMainDetail.Include(x => x.MainDetail.Main.Vendor).Include(x => x.MainDetail.JenisDokumen).Where(x => x.TransId == Id).ToList();
                var TransMainDetailVM = TransMainDetail.Select(x => new TransMainDetailVM
                {
                    Id = x.Id,
                    MainDetail = x.MainDetail,
                    TotalNominal = x.TotalNominal,
                    TotalRealisasi = _context.TransMainDetail.Where(z => z.MainDetailId == x.MainDetailId && z.Trans.IsDelete == false && z.Trans.StatusId == 2).Select(z => z.TotalNominal).DefaultIfEmpty().Sum()
                }).ToList();
                var TransPengadaan = _context.TransPengadaan.Where(x => x.TransId == Id).ToList();
                var TotPengadaan = TransPengadaan.Sum(x => x.Nominal);
                var TransPotonganMaterai = _context.TransPotongan.FirstOrDefault(x => x.TransId == Id && x.SubJenisPotongan.JenisPotonganId == 1);
                var TransPotonganDenda = _context.TransPotongan.FirstOrDefault(x => x.TransId == Id && x.SubJenisPotongan.JenisPotonganId == 2);
                var TransPotonganPPN = _context.TransPotongan.Include(x => x.SubJenisPotongan).Where(x => x.TransId == Id && x.SubJenisPotongan.JenisPotonganId == 3).ToList();
                var TotPPN = TransPotonganPPN.Sum(x => x.Total);
                var TotPengadaanPPN = TotPengadaan + TotPPN;
                var TransPotonganPPH = _context.TransPotongan.Include(x => x.SubJenisPotongan).Where(x => x.TransId == Id && x.SubJenisPotongan.JenisPotonganId == 4).ToList();
                var TotPPH = TransPotonganPPH.Sum(x => x.Total);
                var TotPengadaanPPH = TotPengadaanPPN - TotPPH;
                var JumlahPembayaran = TotPengadaanPPH - TotPPN - TransPotonganDenda.Nominal;

                var TransRekeningMain = _context.TransRekening.Include(x => x.Bank).FirstOrDefault(x => x.TransId == Id && x.IsMain == true);
                var TransRekeningDebit = _context.TransRekening.Where(x => x.TransId == Id && x.IsMain == false && x.IsDebit == true).ToList();
                var Debit = TransRekeningDebit.Sum(x => x.Nominal);
                var TransRekeningKredit = _context.TransRekening.Where(x => x.TransId == Id && x.IsMain == false && x.IsDebit == false).ToList();
                var Kredit = TransRekeningKredit.Sum(x => x.Nominal);
                var TransAttachment = _context.TransAttachment.Include(x => x.SubJenisAttch.JenisAttch).Include(x => x.OutputAttch).Where(x => x.TransId == Id).ToList();
                #endregion

                #region Memo
                pdfDoc.SetPageSize(PageSize.A4);
                pdfDoc.SetMargins(40, 40, 40, 40); //( point marginLeft, point marginRight, point marginTop, point marginBottom )
                pdfDoc.Open();

                #region Create Tabel
                //Table 1
                PdfPTable table = new PdfPTable(14);
                float[] widths = new float[] { 5f, 4f, 5f, 9f, 6f, 3f, 4f, 16f, 2f, 7f, 23f, 9f, 4f, 15f };
                //float[] widths = new float[] { 30, 25, 40, 65, 45, 25, 25, 115, 20, 50, 165, 65, 30, 105};
                table.SetWidths(widths);
                table.WidthPercentage = 100;
                table.HorizontalAlignment = Element.ALIGN_CENTER;
                //table.SpacingBefore = 20f;
                //table.SpacingAfter = 30f;
                #endregion

                #region Memo dan Logo
                PdfPCell cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("Memo"), FontFactory.GetFont("Arial", 24, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 11;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell.PaddingBottom = 15f;
                cell.Border = 0;
                Image image = Image.GetInstance(Server.MapPath("~/Content/img/BNI_logo.png"));
                //image.ScaleAbsolute(200, 150);
                image.ScalePercent(10);
                cell.AddElement(image);
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_BOTTOM;
                table.AddCell(cell);
                #endregion

                #region Nomor dan Tanggal
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("NOMOR"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(": " + Trans.KodeSurat.Nama + Trans.Nomor), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 9;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("TANGGAL : " + Trans.DocDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID"))), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Kepada
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("KEPADA"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(": DIVISI OPERASIONAL"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Dari
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("DARI"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(": DIVISI PENGELOLAAN ASET DAN PENGADAAN"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Hal
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("HAL"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(": PERINTAH PEMBAYARAN"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Kontrak / Adendum / SPK
                Font zapfdingbats = new Font(Font.FontFamily.ZAPFDINGBATS);
                foreach (var item in TransMainDetail)
                {
                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(new Chunk("\u0076", zapfdingbats)));
                    //cell = new PdfPCell(new Phrase(new Chunk("\u0033\u0038", zapfdingbats))); 45, 46, 78, 79
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    var dokumen = item.MainDetail.JenisDokumen.Nama;
                    if (item.MainDetail.JenisDokumenId == 3)
                    {
                        dokumen = item.MainDetail.JenisDokumen.Nama + " " + item.MainDetail.Index;
                    }

                    cell = new PdfPCell(new Phrase(String.Format(dokumen), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 4;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("No. " + item.MainDetail.Nomor), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 4;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Tanggal : " + item.MainDetail.DocDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID"))), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 4;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                }
                #endregion

                #region Spasi
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(" "), FontFactory.GetFont("Arial", 16, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 14;
                cell.Rowspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region I. Nama Rekanan
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("I."), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Nama Rekanan"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 4;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TransMainDetail[0].MainDetail.Main.Vendor.Nama), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 8;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region II. Jenis Pekerjaan
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("II."), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_TOP;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Jenis Pekerjaan"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 4;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_TOP;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_TOP;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(Trans.Uraian), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 8;
                cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                cell.VerticalAlignment = Element.ALIGN_TOP;
                table.AddCell(cell);
                #endregion

                #region III. Nilai Pengadaan
                #region Judul
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("III."), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Nilai Pengadaan"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 13;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Item
                foreach (var item in TransPengadaan)
                {
                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(new Chunk("\u0076", zapfdingbats)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Nama), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 6;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Nominal.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 3;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                }
                #endregion

                #region Total
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("Total Nilai Pengadaan Exclusive PPN"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPengadaan.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion
                #endregion

                #region IV. Pajak Pertambahan Nilai
                #region Judul                
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("IV."), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Pajak Pertambahan Nilai"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 13;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Item
                foreach (var item in TransPotonganPPN)
                {
                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(new Chunk("\u0076", zapfdingbats)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.SubJenisPotongan.Nilai.ToString() + "%"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 3;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format("X"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Nominal.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Total.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 3;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                }
                #endregion

                #region Total
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("Total Pajak Pertambahan Nilai"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPPN.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion
                #endregion

                #region V. Nilai Pengadaan Inclusive PPN
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("V."), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Nilai Pengadaan Inclusive PPN"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPengadaanPPN.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region VI. Biaya Materai
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("VI."), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Biaya Materai"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("0"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region VII. Nilai Pengadaan Inclusive PPN & Biaya Materai
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("VII."), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Nilai Pengadaan Inclusive PPN & Biaya Materai"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPengadaanPPN.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region VIII. Pajak Penghasilan yang harus dipotong
                #region Judul
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("VIII."), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Pajak Penghasilan yang harus dipotong"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 13;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Item
                foreach (var item in TransPotonganPPH)
                {
                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(new Chunk("\u0076", zapfdingbats)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.SubJenisPotongan.Nama + " :"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 2;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.SubJenisPotongan.Nilai.ToString() + "%"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format("X"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Nominal.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Total.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 3;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                }
                #endregion

                #region Total
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("Total Pajak Penghasilan yang harus dipotong"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPPH.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion
                #endregion

                #region IX. Nilai Pengadaan Setelah dipotong PPH
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("IX."), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Nilai Pengadaan Setelah dipotong PPH"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPengadaanPPH.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region X. Pembayaran
                #region Judul
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("X."), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Pembayaran"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 13;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region a. Harap dibayarkan
                #region Judul
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("a"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Harap dibayarkan kepada " + TransRekeningMain.Nama + " sebagai berikut"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Jumlah Bruto
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(new Chunk("\u0076", zapfdingbats)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Jumlah Bruto"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPengadaanPPH.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region PPN Yang Dipotong
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(new Chunk("\u0076", zapfdingbats)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("PPN Yang Dipotong"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPPN.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Denda Keterlambatan
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(new Chunk("\u0076", zapfdingbats)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Denda Keterlambatan"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TransPotonganDenda.Nominal.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Total
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("Jumlah bersih yang dibayarkan kepada rekanan"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(JumlahPembayaran.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #endregion

                #region b. Untuk keuntungan
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("b"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Untuk keuntungan rekening " + TransRekeningMain.NoRek + " di " + TransRekeningMain.Bank.Nama + " Cabang " + TransRekeningMain.Cabang), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region c. Uraian
                #region Judul
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("c"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Uraian"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Item
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(Trans.Uraian), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                cell.VerticalAlignment = Element.ALIGN_TOP;
                table.AddCell(cell);
                #endregion
                #endregion

                #region d. Prestasi Pekerjaan
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("d"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Prestasi Pekerjaan " + Trans.Prestasi), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 6;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Angsuran / termin ke " + Trans.Termin), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 6;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #endregion

                #region XI. Debit
                #region Judul Debit
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("XI."), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Debit"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 13;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                if (TransRekeningDebit.Count() == 1)
                {
                    #region Data debit satu
                    #region Item Debit
                    foreach (var item in TransRekeningDebit)
                    {
                        cell = new PdfPCell();
                        cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(item.Nama), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.Colspan = 7;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell();
                        cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(item.NoRek), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.Colspan = 2;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(item.Nominal.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);
                    }
                    #endregion
                    #endregion
                }
                else
                {
                    #region Data debit lebih dari satu
                    #region Item Debit
                    foreach (var item in TransRekeningDebit)
                    {
                        cell = new PdfPCell();
                        cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell();
                        cell = new PdfPCell(new Phrase(new Chunk("\u0076", zapfdingbats)));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(item.Nama), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.Colspan = 6;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell();
                        cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(item.NoRek), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.Colspan = 2;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(item.Nominal.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);
                    }
                    #endregion

                    #region Total Debit
                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 12;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(Debit.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                    #endregion
                    #endregion
                }


                #region Judul Kredit
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Kredit"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 13;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Item Kredit
                foreach (var item in TransRekeningKredit)
                {
                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(new Chunk("\u0076", zapfdingbats)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Nama), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 6;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.NoRek), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 2;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Nominal.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                }
                #endregion

                #region Total Kredit
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(Kredit.ToString("n0")), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion
                #endregion

                #region XII. Lampiran
                #region Judul
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("XII."), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Lampiran"), FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK)));
                cell.Border = 0;
                cell.Colspan = 13;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Item
                foreach (var item in TransAttachment)
                {
                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format(""), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    var output = "";
                    if (item.OutputAttchId != 1)
                    {
                        output = "X";
                    }

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format(output), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER;
                    cell.Colspan = 2;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.SubJenisAttch.Nama), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 5;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format(":"), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Jumlah + " Lb No. " + item.Nomor), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 2;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Tanggal : " + item.DocDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID"))), FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK)));
                    cell.Border = 0;
                    cell.Colspan = 3;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                }
                #endregion
                #endregion

                pdfDoc.Add(table);
                #endregion
            }
            else
            {
                Chunk chunk = new Chunk("Data tidak tersedia.", FontFactory.GetFont("Calibri", 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK));
                Paragraph para = new Paragraph();
                para.Add(chunk);
                para.Alignment = Element.ALIGN_CENTER;
                para.SpacingBefore = 40f;
                pdfDoc.Add(para);
            }

            #region Close Document & Create Name
            pdfDoc.Close();
            System.Diagnostics.Process.Start(pathfile1);
            #endregion
        }


        public ActionResult ListSlip(int Id)
        {
            TransSlipsVM result = new TransSlipsVM();
            result.Trans = _context.Trans.FirstOrDefault(x => x.Id == Id);
            result.SettingSlip = _context.SettingSlip.Include(x => x.JenisSlip).ToList();

            return View(result);
        }
        public ActionResult Add(int Id)
        {
            InputSlipVM result = GetMaster();
            result.Id = Id;
            return View(result);
        }
        public ActionResult Edit(int Id)
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
        public JsonResult GetListSlip(int Id)
        {
            var User = GetUser();

            List<TransSlip> result = _context.TransSlip.Include(x => x.JenisSlip).Include(x => x.OutputSlip).Include(x => x.CurrencyDebit).Include(x => x.CurrencyKredit).Where(x => x.TransId == Id).ToList();

            var JsonResult = Json(new { data = result }, JsonRequestBehavior.AllowGet);
            JsonResult.MaxJsonLength = int.MaxValue;
            return JsonResult;
        }
        public JsonResult GetSlipById(int Id)
        {
            var result = _context.TransSlip.Include(x => x.JenisSlip).Include(x => x.JenisRekeningDebit).Include(x => x.JenisRekeningKredit).Include(x => x.BankKredit).Include(x => x.CurrencyDebit).Include(x => x.CurrencyKredit).SingleOrDefault(x => x.Id == Id);

            return Json(result, JsonRequestBehavior.AllowGet);
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
        public JsonResult SendingSlip(int Id, bool IsUrgent)
        {
            bool result = false;
            var User = GetUser();

            var Loguser = _con.Query<LogUser>("SELECT * FROM LogUsers WHERE IsLogin = @IsLogin AND UserId = @UserId AND cast (LastLogin as date) = @LastLogin ORDER BY Id", new { IsLogin = true, UserId = User.Id, LastLogin = DateTime.Now.ToString("MM/dd/yyyy") }).FirstOrDefault();
            if (Loguser != null)
            {
                List<int> SlipId = new List<int>();
                int StatusId = 1;
                #region Cek Penyelia
                var Penyelia = _con.Query<LogUser>("SELECT * FROM LogUsers WHERE IsLogin = @IsLogin AND UserId in (SELECT Id FROM AspNetUsers Where UnitId = @UnitId AND JabatanId = @JabatanId) AND cast (LastLogin as date) = @LastLogin ORDER BY Id", new { IsLogin = true, UnitId = User.UnitId, JabatanId = 7, LastLogin = DateTime.Now.ToString("MM/dd/yyyy") }).FirstOrDefault();
                if (Penyelia != null)
                {
                    StatusId = 2;
                }
                #endregion

                #region Create Slip
                var Slip = _context.TransSlip.Where(x => x.TransId == Id).ToList();
                foreach (var item in Slip)
                {
                    var id = _con.QuerySingle<int>(@"INSERT INTO [dbo].[Slips] ([Tanggal],[NoReferensi],[NamaRekDebit],[NoRekDebit],[NamaCabangDebit],[JenisRekeningDebitId],[PesanDebit],[CurrencyDebitId]
                ,[NominalDebit],[NamaRekKredit],[NoRekKredit],[NamaCabangKredit],[JenisRekeningKreditId],[BankKreditId],[CurrencyKreditId],[NominalKredit],[Keterangan1],[Keterangan2],[Keterangan3]
                ,[JenisSlipId],[OutputSlipId],[KelompokId],[CreaterId],[CreateDate],[IsDelete],[NamaRekDebit2],[PesanDebit2],[AddKredit],[AddKredit2],[NominalOverride]
                ,[PhoneKredit],[CityCodeKredit],[IdKredit],[SandiTXN],[Biaya],[IdTypeKredit],[Kurs],[StatusSlipId],[NoRekDebit2],[IsNoRekDebitVA],[NoRekKredit2],[IsNoRekKreditVA]
                ) OUTPUT INSERTED.Id VALUES (@Tanggal,@NoReferensi,@NamaRekDebit,@NoRekDebit,@NamaCabangDebit,@JenisRekeningDebitId,@PesanDebit,@CurrencyDebitId
                ,@NominalDebit,@NamaRekKredit,@NoRekKredit,@NamaCabangKredit,@JenisRekeningKreditId,@BankKreditId,@CurrencyKreditId,@NominalKredit,@Keterangan1,@Keterangan2,@Keterangan3
                ,@JenisSlipId,@OutputSlipId,@KelompokId,@CreaterId,@CreateDate,@IsDelete,@NamaRekDebit2,@PesanDebit2,@AddKredit,@AddKredit2,@NominalOverride
                ,@PhoneKredit,@CityCodeKredit,@IdKredit,@SandiTXN,@Biaya,@IdTypeKredit,@Kurs,@StatusSlipId,@NoRekDebit2,@IsNoRekDebitVA,@NoRekKredit2,@IsNoRekKreditVA);", new
                    {
                        Tanggal = item.Tanggal,
                        NoReferensi = item.NoReferensi,
                        NamaRekDebit = item.NamaRekDebit,
                        NoRekDebit = item.NoRekDebit,
                        NamaCabangDebit = item.NamaCabangDebit,
                        JenisRekeningDebitId = item.JenisRekeningDebitId,
                        PesanDebit = item.PesanDebit,
                        CurrencyDebitId = item.CurrencyDebitId,
                        NominalDebit = item.NominalDebit,
                        NamaRekKredit = item.NamaRekKredit,
                        NoRekKredit = item.NoRekKredit,
                        NamaCabangKredit = item.NamaCabangKredit,
                        JenisRekeningKreditId = item.JenisRekeningKreditId,
                        BankKreditId = item.BankKreditId,
                        CurrencyKreditId = item.CurrencyKreditId,
                        NominalKredit = item.NominalKredit,
                        Keterangan1 = item.Keterangan1,
                        Keterangan2 = item.Keterangan2,
                        Keterangan3 = item.Keterangan3,
                        JenisSlipId = item.JenisSlipId,
                        OutputSlipId = item.OutputSlipId,
                        KelompokId = User.Unit.KelompokId,
                        CreaterId = User.Id,
                        CreateDate = DateTime.Now,
                        IsDelete = false,
                        NamaRekDebit2 = item.NamaRekDebit2,
                        PesanDebit2 = item.PesanDebit2,
                        AddKredit = item.AddKredit,
                        AddKredit2 = item.AddKredit2,
                        NominalOverride = 0,
                        PhoneKredit = item.PhoneKredit,
                        CityCodeKredit = item.CityCodeKredit,
                        IdKredit = item.IdKredit,
                        SandiTXN = item.SandiTXN,
                        Biaya = item.Biaya,
                        IdTypeKredit = item.IdTypeKredit,
                        Kurs = item.Kurs,
                        StatusSlipId = 1,
                        NoRekDebit2 = item.NoRekDebit2,
                        IsNoRekDebitVA = item.IsNoRekDebitVA,
                        NoRekKredit2 = item.NoRekKredit2,
                        IsNoRekKreditVA = item.IsNoRekKreditVA
                    });

                    SlipId.Add(id);
                }
                #endregion

                #region Create Batch
                #region No Batch
                var NoBatch = 1;

                var transdata = _con.Query<int>("SELECT DISTINCT [NoBatch] FROM Trans WHERE KelompokId = @KelompokId AND IsParent = @IsParent AND cast (CreateDate as date) = @CreateDate ORDER BY NoBatch", new { KelompokId = User.Unit.KelompokId, IsParent = true, CreateDate = DateTime.Now.ToString("MM/dd/yyyy") }).ToList();
                //var transdata = _context.Trans.Where(x => x.IsDelete == false && x.KelompokId == User.Unit.KelompokId && EntityFunctions.TruncateTime(x.CreateDate) == EntityFunctions.TruncateTime(DateTime.Now) && x.IsParent == true).GroupBy(x => x.NoBatch).Select(x => x.FirstOrDefault().NoBatch).OrderBy(x => x).ToList();
                if (transdata.Count() > 0)
                {
                    //Memastikan sudah ada transaksi pada hari itu
                    foreach (var item in transdata)
                    {
                        if (item == NoBatch)
                        {
                            //No batch yang di DB sama seperti looping
                            NoBatch = NoBatch + 1;
                        }
                        else
                        {
                            //Terdapat no batch yang belum terpakai
                            break;
                        }
                    }
                }
                #endregion

                var TransId = _con.QuerySingle<int>(@"INSERT INTO [Trans] ([NoBatch],[KelompokId],[StatusId],[CreateDate],[IsDelete],[IsUrgent],[IsParent],[IsDownload]) OUTPUT INSERTED.Id VALUES (@NoBatch,@KelompokId,@StatusId,@CreateDate,@IsDelete,@IsUrgent,@IsParent,@IsDownload);", new
                {
                    NoBatch = NoBatch,
                    KelompokId = User.Unit.KelompokId,
                    StatusId = StatusId,
                    CreateDate = DateTime.Now,
                    IsDelete = false,
                    IsUrgent = IsUrgent,
                    IsParent = true,
                    IsDownload = false
                });

                foreach (var item in SlipId)
                {
                    var SlipIns = _con.QuerySingle<int>(@"INSERT INTO [TransDetails] ([TransId],[SlipId]) OUTPUT INSERTED.Id VALUES (@TransId,@SlipId);", new { TransId = TransId, SlipId = item });
                }
                #endregion

                #region Tracking
                if (Penyelia != null)
                {
                    #region Tracking Ke Penyelia
                    var Track = _con.QuerySingle<int>(@"INSERT INTO [TransTrackings] ([TransId],[ReceiveDate],[ReceiverId],[ReceiverActivity],[ReceiverIcon],[ReceiverColorIcon],[SendDate],[SenderId]) OUTPUT INSERTED.Id VALUES (@TransId,@ReceiveDate,@ReceiverId,@ReceiverActivity,@ReceiverIcon,@ReceiverColorIcon,@SendDate,@SenderId);", new
                    {
                        TransId = TransId,
                        ReceiveDate = DateTime.Now,
                        ReceiverId = User.Id,
                        ReceiverActivity = "creating batch",
                        ReceiverIcon = "pencil",
                        ReceiverColorIcon = "yellow",
                        SendDate = DateTime.Now,
                        SenderId = User.Id
                    });

                    var Track2 = _con.QuerySingle<int>(@"INSERT INTO [TransTrackings] ([TransId],[ReceiveDate],[ReceiverId],[ReceiverActivity],[ReceiverIcon],[ReceiverColorIcon]) OUTPUT INSERTED.Id VALUES (@TransId,@ReceiveDate,@ReceiverId,@ReceiverActivity,@ReceiverIcon,@ReceiverColorIcon);", new
                    {
                        TransId = TransId,
                        ReceiveDate = DateTime.Now,
                        ReceiverId = Penyelia.UserId,
                        ReceiverActivity = "send data to",
                        ReceiverIcon = "send",
                        ReceiverColorIcon = "blue"
                    });
                    #endregion
                }
                else
                {
                    #region Tracking hanya di inputer
                    var Track = _con.QuerySingle<int>(@"INSERT INTO [TransTrackings] ([TransId],[ReceiveDate],[ReceiverId],[ReceiverActivity],[ReceiverIcon],[ReceiverColorIcon]) OUTPUT INSERTED.Id VALUES (@TransId,@ReceiveDate,@ReceiverId,@ReceiverActivity,@ReceiverIcon,@ReceiverColorIcon);", new
                    {
                        TransId = TransId,
                        ReceiveDate = DateTime.Now,
                        ReceiverId = User.Id,
                        ReceiverActivity = "creating batch",
                        ReceiverIcon = "pencil",
                        ReceiverColorIcon = "yellow"
                    });
                    #endregion
                }
                #endregion

                var OldTrack = _context.TransTracking.Where(x => x.TransId == Id).OrderByDescending(x => x.Id).FirstOrDefault();
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
                    NewTrack.TransId = Id;
                    NewTrack.ReceiveDate = DateTime.Now;
                    NewTrack.ReceiverId = User.Id;
                    NewTrack.ReceiverActivity = "create batch on E-Voucher";
                    NewTrack.ReceiverIcon = "pencil";
                    NewTrack.ReceiverColorIcon = "green";
                    _context.TransTracking.Add(NewTrack);
                    _context.SaveChanges();
                    #endregion

                    #region Edit Trans
                    var trans = _context.Trans.SingleOrDefault(x => x.Id == Id);
                    trans.StatusId = 8;
                    _context.Entry(trans).State = EntityState.Modified;
                    _context.SaveChanges();
                    #endregion
                }

                //DownloadPdf(Id);

                result = true;
            }

            //var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>(); //Call NotificationHub
            //context.Clients.User(User.Id).SendNotif(User.Id, 1); //call CreateSlip
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}