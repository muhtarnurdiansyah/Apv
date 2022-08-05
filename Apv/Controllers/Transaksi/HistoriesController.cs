using Apv.Models;
using Apv.Models.Transaksi;
using Apv.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Apv.Controllers.Transaksi
{
    public class HistoriesController : Controller
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
        // GET: Histories
        public ActionResult Index()
        {
            FilterList result = new FilterList();
            result.ListKodeSurat = _context.KodeSurat.ToList();
            result.ListStatus = _context.Status.Where(x => x.Id >= 2).ToList();
            return View(result);
        }
        public JsonResult GetFilterList(FilterData Data, bool UseFilter)
        {
            var User = GetUser();
            List<TransViewVM> result = new List<TransViewVM>();
            List<int> TransIds = new List<int>();
            if (UseFilter)
            {
                #region Use Filter
                List<int> StatusId = _context.Status.Where(x => x.Id >= 2).Select(x => x.Id).ToList();
                DateTime StartCreateDate = new DateTime();
                DateTime EndCreateDate = DateTime.Now;
                DateTime StartDocDate = new DateTime();
                DateTime EndDocDate = DateTime.Now;

                if (Data.OptionCreateDate)
                {
                    StartCreateDate = Data.StartCreateDate;
                    EndCreateDate = Data.EndCreateDate;
                }
                if (Data.OptionDocDate)
                {
                    StartDocDate = Data.StartDocDate;
                    EndDocDate = Data.EndDocDate;
                }
                if (Data.OptionStatus)
                {
                    StatusId.Clear();
                    StatusId.Add(Data.Status);
                }


                if (Data.OptionNomor)
                {
                    TransIds = _context.Trans.Where(x => x.IsDelete == false
                 && DbFunctions.TruncateTime(x.CreateDate) >= DbFunctions.TruncateTime(StartCreateDate)
                 && DbFunctions.TruncateTime(x.CreateDate) <= DbFunctions.TruncateTime(EndCreateDate)
                 && DbFunctions.TruncateTime(x.DocDate) >= DbFunctions.TruncateTime(StartDocDate)
                 && DbFunctions.TruncateTime(x.DocDate) <= DbFunctions.TruncateTime(EndDocDate)
                 && x.KodeSuratId == Data.KodeSurat
                 && x.Nomor == Data.Nomor
                 && StatusId.Contains(x.StatusId)).Select(x => x.Id).ToList();
                }
                else
                {
                    TransIds = _context.Trans.Where(x => x.IsDelete == false
                && DbFunctions.TruncateTime(x.CreateDate) >= DbFunctions.TruncateTime(StartCreateDate)
                && DbFunctions.TruncateTime(x.CreateDate) <= DbFunctions.TruncateTime(EndCreateDate)
                && DbFunctions.TruncateTime(x.DocDate) >= DbFunctions.TruncateTime(StartDocDate)
                && DbFunctions.TruncateTime(x.DocDate) <= DbFunctions.TruncateTime(EndDocDate)
                && StatusId.Contains(x.StatusId)).Select(x => x.Id).ToList();
                }

                #endregion
            }
            else
            {
                #region Not Use Filter
                TransIds = _context.Trans.Where(x => x.IsDelete == false && x.StatusId >= 2 && DbFunctions.TruncateTime(x.CreateDate) == DbFunctions.TruncateTime(DateTime.Now)).Select(x => x.Id).ToList();
                #endregion
            }

            result = _context.TransMainDetail.Include(x => x.Trans.Status).Include(x => x.Trans.KodeSurat).Include(x => x.MainDetail.Main.Vendor).Where(x => TransIds.Contains(x.TransId) && x.MainDetail.Main.IsDelete == false && x.MainDetail.IsDelete == false).GroupBy(x => x.TransId).Select(x => new TransViewVM
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
                Vendor = x.FirstOrDefault().MainDetail.Main.Vendor,
                IsTakeBack = false
            }).OrderBy(x => x.Status.Id).ThenBy(x => x.Id).ToList();

            foreach (var item in result)
            {
                if (item.Status.Id == 3 && !UseFilter)
                {
                    var InputerId = _context.TransTracking.Where(x => x.TransId == item.Id).OrderBy(x => x.Id).FirstOrDefault().ReceiverId;
                    if (InputerId == User.Id)
                    {
                        item.IsTakeBack = true;
                    }
                }
            }

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
                TotalRealisasi = _context.TransMainDetail.Where(z => z.MainDetailId == x.MainDetailId && z.Trans.IsDelete == false && z.Trans.StatusId == 2).Select(z => z.TotalNominal).DefaultIfEmpty().Sum()
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
        public JsonResult GetHistoryById(int Id)
        {
            var result = _context.TransTracking.Include(x => x.Sender).Include(x => x.Receiver).Where(x => x.TransId == Id).OrderBy(x => x.Id).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetLastKeteranganById(int Id)
        {
            var Keterangan = "";
            Keterangan = _context.TransTracking.Where(x => x.TransId == Id && x.SenderKeterangan != null).OrderByDescending(x => x.Id).FirstOrDefault().SenderKeterangan;

            var Status = _context.Trans.Include(x => x.Status).SingleOrDefault(x => x.Id == Id).Status;

            return Json(new { Keterangan = Keterangan, Status = Status }, JsonRequestBehavior.AllowGet);
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
        public JsonResult TakeBack(int Id)
        {
            bool result = false;
            var User = GetUser();

            #region Get Last Receiver
            var LastTrack = _context.TransTracking.Include(x => x.Receiver).Where(x => x.TransId == Id).OrderByDescending(x => x.Id).FirstOrDefault();
            #endregion

            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            if (manager.IsInRole(User.Id, "Modul Inputer"))
            {
                #region Inputer
                if (LastTrack.Receiver.JabatanId <= User.JabatanId)
                {
                    #region Edit Tracking Before to Add Sender
                    var OldTrack = _context.TransTracking.Where(x => x.TransId == Id).OrderByDescending(x => x.Id).FirstOrDefault();
                    OldTrack.SendDate = DateTime.Now;
                    OldTrack.SenderId = OldTrack.ReceiverId;
                    _context.Entry(OldTrack).State = EntityState.Modified;
                    _context.SaveChanges();
                    #endregion

                    #region Add New Tracking for Receiver
                    TransTracking NewTrack = new TransTracking();
                    NewTrack.TransId = Id;
                    NewTrack.ReceiveDate = DateTime.Now;
                    NewTrack.ReceiverId = User.Id;
                    NewTrack.ReceiverActivity = "take back data to";
                    NewTrack.ReceiverIcon = "mail-reply";
                    NewTrack.ReceiverColorIcon = "red";
                    _context.TransTracking.Add(NewTrack);
                    _context.SaveChanges();
                    #endregion

                    #region Edit Trans
                    var trans = _context.Trans.SingleOrDefault(x => x.Id == Id);
                    trans.StatusId = 2;
                    _context.Entry(trans).State = EntityState.Modified;
                    _context.SaveChanges();
                    #endregion

                    result = true;
                }
                #endregion

            }

            #region Message for View
            var title = "";
            var text = "";
            var type = "";
            if (result)
            {
                #region Success
                title = "Success!";
                text = "That data has been take back!";
                type = "success";
                #endregion
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
        public void DownloadPdf(int Id)
        {
            var User = GetUser();

            #region Create PDF Document
            Document pdfDoc = new Document();
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            //var pathfile = Server.MapPath("~/Files/Attachment/") + " " + DateTime.Now.ToString("ddMMyyyy ", new System.Globalization.CultureInfo("id-ID")) + " - " + string.Format(@"{0}", DateTime.Now.Ticks) + ".pdf";
            //PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(pathfile, FileMode.Create));

            //PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(@":D\Net Language\" + pathfile, FileMode.Create));
            //PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(@":D/Net Language/Eva2/Eva/Files/" + pathfile, FileMode.Create));
            //var pathfile = Server.MapPath("/DevEva/Files/Attch/") + "PDF Debit " + DateTime.Now.ToString("ddMMyyyy ", new System.Globalization.CultureInfo("id-ID")) + " - " + string.Format(@"{0}", DateTime.Now.Ticks) + ".pdf";
            //var a = Directory.GetDirectories(Server.MapPath("~"));
            //var a2 = @":D\Net Language\";
            //var a3 = Directory.GetDirectories(@":D\Net Language\");

            #endregion

            #region Style Font
            var ArialNormal = FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK);
            var ArialBold = FontFactory.GetFont("Arial", 8, Font.BOLD, BaseColor.BLACK);
            Font zapfdingbats = new Font(Font.FontFamily.ZAPFDINGBATS, 8);
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
                cell = new PdfPCell(new Phrase(String.Format("NOMOR"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(": " + Trans.KodeSurat.Nama + Trans.Nomor), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 9;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("TANGGAL : " + Trans.DocDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID"))), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Kepada
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("KEPADA"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(": DIVISI OPERASIONAL"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Dari
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("DARI"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(": DIVISI PENGELOLAAN ASET DAN PENGADAAN"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Hal
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("HAL"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(": PERINTAH PEMBAYARAN"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Kontrak / Adendum / SPK
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

                    cell = new PdfPCell(new Phrase(String.Format(dokumen), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 4;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("No. " + item.MainDetail.Nomor), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 4;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Tanggal : " + item.MainDetail.DocDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID"))), ArialNormal));
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
                cell = new PdfPCell(new Phrase(String.Format("I."), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Nama Rekanan"), ArialBold));
                cell.Border = 0;
                cell.Colspan = 4;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TransMainDetail[0].MainDetail.Main.Vendor.Nama), ArialBold));
                cell.Border = 0;
                cell.Colspan = 8;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region II. Jenis Pekerjaan
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("II."), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_TOP;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Jenis Pekerjaan"), ArialBold));
                cell.Border = 0;
                cell.Colspan = 4;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_TOP;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_TOP;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(Trans.Uraian), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 8;
                cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                cell.VerticalAlignment = Element.ALIGN_TOP;
                table.AddCell(cell);
                #endregion

                #region III. Nilai Pengadaan
                #region Judul
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("III."), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Nilai Pengadaan"), ArialBold));
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
                    cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
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

                    cell = new PdfPCell(new Phrase(String.Format(item.Nama), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 6;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Nominal.ToString("n0")), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 3;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                }
                #endregion

                #region Total
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("Total Nilai Pengadaan Exclusive PPN"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPengadaan.ToString("n0")), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion
                #endregion

                #region IV. Pajak Pertambahan Nilai
                #region Judul                
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("IV."), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Pajak Pertambahan Nilai"), ArialBold));
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
                    cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
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

                    cell = new PdfPCell(new Phrase(String.Format(item.SubJenisPotongan.Nilai.ToString() + "%"), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 3;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format("X"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Nominal.ToString("n0")), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Total.ToString("n0")), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 3;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                }
                #endregion

                #region Total
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("Total Pajak Pertambahan Nilai"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPPN.ToString("n0")), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion
                #endregion

                #region V. Nilai Pengadaan Inclusive PPN
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("V."), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Nilai Pengadaan Inclusive PPN"), ArialBold));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPengadaanPPN.ToString("n0")), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region VI. Biaya Materai
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("VI."), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Biaya Materai"), ArialBold));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("0"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region VII. Nilai Pengadaan Inclusive PPN & Biaya Materai
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("VII."), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Nilai Pengadaan Inclusive PPN & Biaya Materai"), ArialBold));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPengadaanPPN.ToString("n0")), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region VIII. Pajak Penghasilan yang harus dipotong
                #region Judul
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("VIII."), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Pajak Penghasilan yang harus dipotong"), ArialBold));
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
                    cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
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

                    cell = new PdfPCell(new Phrase(String.Format(item.SubJenisPotongan.Nama + " :"), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 2;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.SubJenisPotongan.Nilai.ToString() + "%"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format("X"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Nominal.ToString("n0")), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Total.ToString("n0")), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 3;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                }
                #endregion

                #region Total
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("Total Pajak Penghasilan yang harus dipotong"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPPH.ToString("n0")), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion
                #endregion

                #region IX. Nilai Pengadaan Setelah dipotong PPH
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("IX."), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Nilai Pengadaan Setelah dipotong PPH"), ArialBold));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPengadaanPPH.ToString("n0")), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region X. Pembayaran
                #region Judul
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("X."), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Pembayaran"), ArialBold));
                cell.Border = 0;
                cell.Colspan = 13;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region a. Harap dibayarkan
                #region Judul
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("a"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Harap dibayarkan kepada " + TransRekeningMain.Nama + " sebagai berikut"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Jumlah Bruto
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
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

                cell = new PdfPCell(new Phrase(String.Format("Jumlah Bruto"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPengadaanPPH.ToString("n0")), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region PPN Yang Dipotong
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
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

                cell = new PdfPCell(new Phrase(String.Format("PPN Yang Dipotong"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TotPPN.ToString("n0")), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Denda Keterlambatan
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
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

                cell = new PdfPCell(new Phrase(String.Format("Denda Keterlambatan"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(TransPotonganDenda.Nominal.ToString("n0")), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 3;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Total
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("Jumlah bersih yang dibayarkan kepada rekanan"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 10;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(JumlahPembayaran.ToString("n0")), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #endregion

                #region b. Untuk keuntungan
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("b"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                var txt = new Phrase(new Chunk("Untuk keuntungan rekening ", ArialNormal));
                txt.Add(new Chunk(TransRekeningMain.NoRek, ArialBold));
                txt.Add(new Chunk(" di ", ArialNormal));
                txt.Add(new Chunk(TransRekeningMain.Bank.Singkatan, ArialBold));
                txt.Add(new Chunk(" Cabang ", ArialNormal));
                txt.Add(new Chunk(TransRekeningMain.Cabang, ArialBold));

                cell = new PdfPCell(txt);
                //cell = new PdfPCell(new Phrase(String.Format("Untuk keuntungan rekening " + TransRekeningMain.NoRek + " di " + TransRekeningMain.Bank.Singkatan + " Cabang " + TransRekeningMain.Cabang), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region c. Uraian
                #region Judul
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("c"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Uraian"), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion

                #region Item
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 2;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(Trans.Uraian), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
                cell.VerticalAlignment = Element.ALIGN_TOP;
                table.AddCell(cell);
                #endregion
                #endregion

                #region d. Prestasi Pekerjaan
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("d"), ArialNormal));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Prestasi Pekerjaan " + Trans.Prestasi), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 6;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Angsuran / termin ke " + Trans.Termin), ArialNormal));
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
                cell = new PdfPCell(new Phrase(String.Format("XI."), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Debit"), ArialBold));
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
                        cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(item.Nama), ArialNormal));
                        cell.Border = 0;
                        cell.Colspan = 7;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell();
                        cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(item.NoRek), ArialNormal));
                        cell.Border = 0;
                        cell.Colspan = 2;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialBold));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(item.Nominal.ToString("n0")), ArialBold));
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
                        cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
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

                        cell = new PdfPCell(new Phrase(String.Format(item.Nama), ArialNormal));
                        cell.Border = 0;
                        cell.Colspan = 6;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell();
                        cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(item.NoRek), ArialNormal));
                        cell.Border = 0;
                        cell.Colspan = 2;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);

                        cell = new PdfPCell(new Phrase(String.Format(item.Nominal.ToString("n0")), ArialNormal));
                        cell.Border = 0;
                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);
                    }
                    #endregion

                    #region Total Debit
                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 12;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialBold));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(Debit.ToString("n0")), ArialBold));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                    #endregion
                    #endregion
                }


                #region Judul Kredit
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Kredit"), ArialBold));
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
                    cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
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

                    cell = new PdfPCell(new Phrase(String.Format(item.Nama), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 6;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.NoRek), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 2;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Nominal.ToString("n0")), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                }
                #endregion

                #region Total Kredit
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
                cell.Border = 0;
                cell.Colspan = 12;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Rp"), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format(Kredit.ToString("n0")), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);
                #endregion
                #endregion

                #region XII. Lampiran
                #region Judul
                cell = new PdfPCell();
                cell = new PdfPCell(new Phrase(String.Format("XII."), ArialBold));
                cell.Border = 0;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(String.Format("Lampiran"), ArialBold));
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
                    cell = new PdfPCell(new Phrase(String.Format(""), ArialNormal));
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
                    cell = new PdfPCell(new Phrase(String.Format(output), ArialNormal));
                    cell.Border = Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER;
                    cell.Colspan = 2;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.SubJenisAttch.Nama), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 5;
                    cell.PaddingLeft = 5;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell = new PdfPCell(new Phrase(String.Format(":"), ArialNormal));
                    cell.Border = 0;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format(item.Jumlah + " Lb No. " + item.Nomor), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 2;
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(String.Format("Tanggal : " + item.DocDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID"))), ArialNormal));
                    cell.Border = 0;
                    cell.Colspan = 3;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);

                    //var otp = Server.MapPath("~/Files/Attachment/");

                    //cell = new PdfPCell();
                    //cell = new PdfPCell(new Phrase(String.Format(otp), ArialNormal));
                    //cell.Border = Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER;
                    //cell.Colspan = 2;
                    //cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    //cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //table.AddCell(cell);

                    //string[] source = System.IO.Directory.GetFiles(Server.MapPath("~/Files/Attachment/"), ".pdf");
                    //string.Copy(Convert.ToString(source));
                    //cell = new PdfPCell(new Phrase(String.Format(item.Path)));
                    //cell = new PdfPCell();
                    //cell.Border = 0;
                    //cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    //cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //table.AddCell(cell);
                }
                #endregion
                #endregion

                pdfDoc.Add(table);
                #endregion
            }
            else
            {
                Chunk chunk = new Chunk("Data tidak tersedia.", ArialBold);
                Paragraph para = new Paragraph();
                para.Add(chunk);
                para.Alignment = Element.ALIGN_CENTER;
                para.SpacingBefore = 40f;
                pdfDoc.Add(para);
            }

            #region Close Document & Create Name
            pdfWriter.CloseStream = false;
            pdfDoc.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            //Response.AddHeader("content-disposition", "inline;filename=Memo" + DateTime.Now.ToShortDateString() + ".pdf");
            Response.AddHeader("content-disposition", "attachment;filename=Memo" + DateTime.Now.ToShortDateString() + ".pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();

            //Response.Close();
            //pdfDoc.Close();
            //System.Diagnostics.Process.Start(pathfile);

            #endregion
        }
    }
}

