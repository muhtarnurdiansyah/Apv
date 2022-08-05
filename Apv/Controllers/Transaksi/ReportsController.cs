using Apv.Models;
using Apv.Models.Master;
using Apv.Models.Transaksi;
using Apv.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Apv.Controllers.Transaksi
{
    public class ReportsController : Controller
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
        // GET: Reports
        public ActionResult Index()
        {
            return View();
        }

        public ExcelWorksheet PPN(ExcelPackage pck, DateTime StartDate, DateTime EndDate, string sheet)
        {
            #region Create Excel
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add(sheet);
            ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
            ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
            if (System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft)   // Right to Left for Arabic lang
            {
                ExcelWorksheetView wv = ws.View;
                wv.ZoomScale = 70;
                wv.RightToLeft = true;
                ws.PrinterSettings.Orientation = eOrientation.Landscape;
                ws.Cells.AutoFitColumns();
            }
            else
            {
                ExcelWorksheetView wv = ws.View;
                wv.ZoomScale = 70;
                wv.RightToLeft = false;
                ws.PrinterSettings.Orientation = eOrientation.Landscape;
                ws.Cells.AutoFitColumns();
            }
            #endregion

            #region Create Judul
            ws.Cells[1, 1].Value = "REKAP " + sheet.ToUpper();
            ws.Cells[2, 1].Value = StartDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID")) + " - " + EndDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID"));

            ws.Cells[1, 1, 1, 11].Merge = true;
            ws.Cells[2, 1, 2, 11].Merge = true;
            ws.Cells[1, 1, 2, 11].Style.Numberformat.Format = "@";
            ws.Cells[1, 1, 2, 11].Style.Font.Size = 14;
            ws.Cells[1, 1, 2, 11].Style.Font.Bold = true;
            
            ws.Cells[1, 1, 2, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[1, 1, 2, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            #endregion

            #region Create Thead Tabel                
            ws.Cells[4, 1].Value = "NO";
            ws.Cells[4, 2].Value = "NO MEMO";
            ws.Cells[4, 3].Value = "TANGGAL MEMO";
            ws.Cells[4, 4].Value = "NPWP";
            ws.Cells[4, 5].Value = "NAMA REKANAN";
            ws.Cells[4, 6].Value = "NO FAKTUR";
            ws.Cells[4, 7].Value = "TANGGAL FAKTUR";
            ws.Cells[4, 8].Value = "TARIF";
            ws.Cells[4, 9].Value = "PPN";
            ws.Cells[4, 10].Value = "DPP";
            ws.Cells[4, 11].Value = "KETERANGAN";

            #region Style Thead
            ws.Cells[4, 1, 4, 11].Style.Numberformat.Format = "@";
            ws.Cells[4, 1, 4, 11].Style.Font.Size = 11;
            ws.Cells[4, 1, 4, 11].Style.Font.Bold = true;
            ws.Cells[4, 1, 4, 11].Style.Font.Color.SetColor(Color.White);
            ws.Cells[4, 1, 4, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[4, 1, 4, 11].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#31869B"));
            ws.Cells[4, 1, 4, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[4, 1, 4, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[4, 1, 4, 11].Style.WrapText = true;

            ws.Column(1).Width = 5;
            ws.Column(2).Width = 20;
            ws.Column(3).Width = 13;
            ws.Column(4).Width = 18;
            ws.Column(5).Width = 36;
            ws.Column(6).Width = 36;
            ws.Column(7).Width = 36;
            ws.Column(8).Width = 8;
            ws.Column(9).Width = 14;
            ws.Column(10).Width = 17;
            ws.Column(11).Width = 160;
            #endregion
            #endregion

            #region Create Content Tabel
            var data = _context.TransPotongan.Include(x => x.Trans.KodeSurat).Include(x => x.SubJenisPotongan).Where(x => x.SubJenisPotongan.JenisPotonganId == 3 && x.SubJenisPotongan.Nama2 == sheet && DbFunctions.TruncateTime(x.Trans.DocDate) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.Trans.DocDate) <= DbFunctions.TruncateTime(EndDate)).OrderBy(x => x.SubJenisPotongan.Nilai).ThenBy(x => x.Trans.DocDate).ToList();

            if (data.Count() > 0)
            {
                #region Data Tersedia
                int tr = 1, no = 1, indx = 1;
                decimal tarif = 0;

                foreach (var item in data)
                {
                    if (tarif != item.SubJenisPotongan.Nilai && tarif != 0)
                    {
                        #region Sub Total
                        ws.Cells[4 + tr, 1].Value = "Sub Total";
                        ws.Cells[4 + tr, 9].Formula = "=SUM(" + ws.Cells[4 + indx, 9].Address + ":" + ws.Cells[4 + tr - 1, 9].Address + ")";
                        ws.Cells[4 + tr, 10].Formula = "=SUM(" + ws.Cells[4 + indx, 10].Address + ":" + ws.Cells[4 + tr - 1, 10].Address + ")";

                        #region Style
                        ws.Cells[4 + tr, 1, 4 + tr, 8].Merge = true;
                        ws.Cells[4 + tr, 1, 4 + tr, 8].Style.Numberformat.Format = "@";
                        ws.Cells[4 + tr, 1, 4 + tr, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[4 + tr, 9, 4 + tr, 10].Style.Numberformat.Format = "_-* #,##0_-;-* #,##0_-;_-* \"-\"??_-;_-@_-";
                        ws.Cells[4 + tr, 9, 4 + tr, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        ws.Cells[4 + tr, 1, 4 + tr, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        #endregion

                        tr++;
                        indx = tr;
                        #endregion
                    }

                    ws.Cells[4 + tr, 1].Value = no;
                    ws.Cells[4 + tr, 2].Value = item.Trans.KodeSurat.Nama + item.Trans.Nomor;
                    ws.Cells[4 + tr, 3].Value = item.Trans.DocDate.ToShortDateString();

                    #region NPWP dan Rekanan
                    TransMainDetail maindetail = _context.TransMainDetail.Include(x => x.MainDetail.Main.Vendor).FirstOrDefault(x => x.TotalNominal > 0 && x.TransId == item.TransId);
                    if (maindetail == null)
                    {
                        maindetail = _context.TransMainDetail.Include(x => x.MainDetail.Main.Vendor).FirstOrDefault(x => x.TransId == item.TransId);
                    }
                    #endregion

                    ws.Cells[4 + tr, 4].Value = maindetail.MainDetail.NPWP;
                    ws.Cells[4 + tr, 5].Value = maindetail.MainDetail.Main.Vendor.Nama;

                    #region MyRegion
                    TransAttachment attch = _context.TransAttachment.FirstOrDefault(x => x.TransId == item.TransId && x.SubJenisAttchId == 1);
                    #endregion

                    if (attch != null)
                    {
                        ws.Cells[4 + tr, 6].Value = attch.Nomor;
                        ws.Cells[4 + tr, 7].Value = attch.DocDate.ToShortDateString();

                        ws.Cells[4 + tr, 7].Style.Numberformat.Format = "dd/MM/yyyy";
                    }

                    ws.Cells[4 + tr, 8].Value = item.SubJenisPotongan.Nilai;
                    ws.Cells[4 + tr, 9].Value = item.Total;
                    ws.Cells[4 + tr, 10].Value = item.Nominal;
                    //ws.Cells[4 + tr, 7].Formula = "=SUM(" + ws.Cells[4 + tr, 3].Address + "*" + ws.Cells[4 + tr, 6].Address+")";
                    ws.Cells[4 + tr, 11].Value = item.Trans.Uraian;

                    #region Style
                    ws.Cells[4 + tr, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[4 + tr, 2].Style.Numberformat.Format = "@";
                    ws.Cells[4 + tr, 3].Style.Numberformat.Format = "dd/MM/yyyy";
                    ws.Cells[4 + tr, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells[4 + tr, 4, 4 + tr, 6].Style.Numberformat.Format = "@";
                    ws.Cells[4 + tr, 8].Style.Numberformat.Format = "#0\\.00%";
                    ws.Cells[4 + tr, 9, 4 + tr, 10].Style.Numberformat.Format = "_-* #,##0_-;-* #,##0_-;_-* \"-\"??_-;_-@_-";
                    ws.Cells[4 + tr, 8, 4 + tr, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells[4 + tr, 1, 4 + tr, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[4 + tr, 11].Style.WrapText = true;
                    #endregion

                    tarif = item.SubJenisPotongan.Nilai;
                    no++;
                    tr++;
                }

                #region Sub Total
                ws.Cells[4 + tr, 1].Value = "Sub Total";
                ws.Cells[4 + tr, 9].Formula = "=SUM(" + ws.Cells[4 + indx, 9].Address + ":" + ws.Cells[4 + tr - 1, 9].Address + ")";
                ws.Cells[4 + tr, 10].Formula = "=SUM(" + ws.Cells[4 + indx, 10].Address + ":" + ws.Cells[4 + tr - 1, 10].Address + ")";

                #region Style
                ws.Cells[4 + tr, 1, 4 + tr, 8].Merge = true;
                ws.Cells[4 + tr, 1, 4 + tr, 8].Style.Numberformat.Format = "@";
                ws.Cells[4 + tr, 1, 4 + tr, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[4 + tr, 9, 4 + tr, 10].Style.Numberformat.Format = "_-* #,##0_-;-* #,##0_-;_-* \"-\"??_-;_-@_-";
                ws.Cells[4 + tr, 9, 4 + tr, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells[4 + tr, 1, 4 + tr, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells[4, 1, 4 + tr, 11].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[4, 1, 4 + tr, 11].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[4, 1, 4 + tr, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[4, 1, 4 + tr, 11].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                #endregion                
                #endregion

                #endregion
            }
            else
            {
                #region Data Tidak Tersedia
                ws.Cells[5, 1, 6, 11].Value = "DATA TIDAK TERSEDIA";
                ws.Cells[5, 1, 6, 11].Merge = true;

                ws.Cells[4, 1, 6, 11].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[4, 1, 6, 11].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[4, 1, 6, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[4, 1, 6, 11].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                ws.Cells[4, 1, 6, 11].Style.Numberformat.Format = "@";
                ws.Cells[4, 1, 6, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[4, 1, 6, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                #endregion
            }
            #endregion

            return ws;
        }

        public ExcelWorksheet PPH(ExcelPackage pck, DateTime StartDate, DateTime EndDate, string sheet)
        {
            #region Create Excel
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add(sheet);
            ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
            ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
            if (System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft)   // Right to Left for Arabic lang
            {
                ExcelWorksheetView wv = ws.View;
                wv.ZoomScale = 70;
                wv.RightToLeft = true;
                ws.PrinterSettings.Orientation = eOrientation.Landscape;
                ws.Cells.AutoFitColumns();
            }
            else
            {
                ExcelWorksheetView wv = ws.View;
                wv.ZoomScale = 70;
                wv.RightToLeft = false;
                ws.PrinterSettings.Orientation = eOrientation.Landscape;
                ws.Cells.AutoFitColumns();
            }
            #endregion

            #region Create Judul
            ws.Cells[1, 1].Value = "REKAP " + sheet.ToUpper();
            ws.Cells[2, 1].Value = StartDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID")) + " - " + EndDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID"));

            ws.Cells[1, 1, 1, 9].Merge = true;
            ws.Cells[2, 1, 2, 9].Merge = true;
            ws.Cells[1, 1, 2, 9].Style.Numberformat.Format = "@";
            ws.Cells[1, 1, 2, 9].Style.Font.Size = 14;
            ws.Cells[1, 1, 2, 9].Style.Font.Bold = true;

            ws.Cells[1, 1, 2, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[1, 1, 2, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            #endregion

            #region Create Thead Tabel                
            ws.Cells[4, 1].Value = "NO";
            ws.Cells[4, 2].Value = "NO MEMO";
            ws.Cells[4, 3].Value = "TANGGAL MEMO";
            ws.Cells[4, 4].Value = "NPWP";
            ws.Cells[4, 5].Value = "NAMA REKANAN";
            ws.Cells[4, 6].Value = "TARIF";
            ws.Cells[4, 7].Value = "PPH";
            ws.Cells[4, 8].Value = "DPP";
            ws.Cells[4, 9].Value = "KETERANGAN";

            #region Style Thead
            ws.Cells[4, 1, 4, 9].Style.Numberformat.Format = "@";
            ws.Cells[4, 1, 4, 9].Style.Font.Size = 11;
            ws.Cells[4, 1, 4, 9].Style.Font.Bold = true;
            ws.Cells[4, 1, 4, 9].Style.Font.Color.SetColor(Color.White);
            ws.Cells[4, 1, 4, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[4, 1, 4, 9].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#31869B"));
            ws.Cells[4, 1, 4, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[4, 1, 4, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[4, 1, 4, 9].Style.WrapText = true;

            ws.Column(1).Width = 5;
            ws.Column(2).Width = 20;
            ws.Column(3).Width = 13;
            ws.Column(4).Width = 18;
            ws.Column(5).Width = 36;
            ws.Column(6).Width = 8;
            ws.Column(7).Width = 14;
            ws.Column(8).Width = 17;
            ws.Column(9).Width = 160;
            #endregion
            #endregion

            #region Create Content Tabel
            var data = _context.TransPotongan.Include(x => x.Trans.KodeSurat).Include(x => x.SubJenisPotongan).Where(x => x.SubJenisPotongan.JenisPotonganId == 4 && x.SubJenisPotongan.Nama2 == sheet && DbFunctions.TruncateTime(x.Trans.DocDate) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.Trans.DocDate) <= DbFunctions.TruncateTime(EndDate)).OrderBy(x => x.SubJenisPotongan.Nilai).ThenBy(x => x.Trans.DocDate).ToList();

            if (data.Count() > 0)
            {
                #region Data Tersedia
                int tr = 1, no = 1, indx = 1;
                decimal tarif = 0;

                foreach (var item in data)
                {
                    if (tarif != item.SubJenisPotongan.Nilai && tarif != 0)
                    {
                        #region Sub Total
                        ws.Cells[4 + tr, 1].Value = "Sub Total";
                        ws.Cells[4 + tr, 7].Formula = "=SUM(" + ws.Cells[4 + indx, 7].Address + ":" + ws.Cells[4 + tr - 1, 7].Address + ")";
                        ws.Cells[4 + tr, 8].Formula = "=SUM(" + ws.Cells[4 + indx, 8].Address + ":" + ws.Cells[4 + tr - 1, 8].Address + ")";

                        #region Style
                        ws.Cells[4 + tr, 1, 4 + tr, 6].Merge = true;
                        ws.Cells[4 + tr, 1, 4 + tr, 6].Style.Numberformat.Format = "@";
                        ws.Cells[4 + tr, 1, 4 + tr, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[4 + tr, 7, 4 + tr, 8].Style.Numberformat.Format = "_-* #,##0_-;-* #,##0_-;_-* \"-\"??_-;_-@_-";
                        ws.Cells[4 + tr, 7, 4 + tr, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        ws.Cells[4 + tr, 1, 4 + tr, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        #endregion

                        tr++;
                        indx = tr;
                        #endregion
                    }

                    ws.Cells[4 + tr, 1].Value = no;
                    ws.Cells[4 + tr, 2].Value = item.Trans.KodeSurat.Nama + item.Trans.Nomor;
                    ws.Cells[4 + tr, 3].Value = item.Trans.DocDate.ToShortDateString();

                    #region NPWP dan Rekanan
                    TransMainDetail maindetail = _context.TransMainDetail.Include(x => x.MainDetail.Main.Vendor).FirstOrDefault(x => x.TotalNominal > 0 && x.TransId == item.TransId);
                    if (maindetail == null)
                    {
                        maindetail = _context.TransMainDetail.Include(x => x.MainDetail.Main.Vendor).FirstOrDefault(x => x.TransId == item.TransId);
                    }
                    #endregion

                    ws.Cells[4 + tr, 4].Value = maindetail.MainDetail.NPWP;
                    ws.Cells[4 + tr, 5].Value = maindetail.MainDetail.Main.Vendor.Nama;
                    ws.Cells[4 + tr, 6].Value = item.SubJenisPotongan.Nilai;
                    ws.Cells[4 + tr, 7].Value = item.Total;
                    ws.Cells[4 + tr, 8].Value = item.Nominal;
                    //ws.Cells[4 + tr, 7].Formula = "=SUM(" + ws.Cells[4 + tr, 3].Address + "*" + ws.Cells[4 + tr, 6].Address+")";
                    ws.Cells[4 + tr, 9].Value = item.Trans.Uraian;

                    #region Style
                    ws.Cells[4 + tr, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[4 + tr, 2].Style.Numberformat.Format = "@";
                    ws.Cells[4 + tr, 3].Style.Numberformat.Format = "dd/MM/yyyy";
                    ws.Cells[4 + tr, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells[4 + tr, 4, 4 + tr, 5].Style.Numberformat.Format = "@";
                    ws.Cells[4 + tr, 6].Style.Numberformat.Format = "#0\\.00%";
                    ws.Cells[4 + tr, 7, 4 + tr, 8].Style.Numberformat.Format = "_-* #,##0_-;-* #,##0_-;_-* \"-\"??_-;_-@_-";
                    ws.Cells[4 + tr, 6, 4 + tr, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells[4 + tr, 1, 4 + tr, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[4 + tr, 11].Style.WrapText = true;
                    #endregion

                    tarif = item.SubJenisPotongan.Nilai;
                    no++;
                    tr++;
                }

                #region Sub Total
                ws.Cells[4 + tr, 1].Value = "Sub Total";
                ws.Cells[4 + tr, 7].Formula = "=SUM(" + ws.Cells[4 + indx, 7].Address + ":" + ws.Cells[4 + tr - 1, 7].Address + ")";
                ws.Cells[4 + tr, 8].Formula = "=SUM(" + ws.Cells[4 + indx, 8].Address + ":" + ws.Cells[4 + tr - 1, 8].Address + ")";

                #region Style
                ws.Cells[4 + tr, 1, 4 + tr, 6].Merge = true;
                ws.Cells[4 + tr, 1, 4 + tr, 6].Style.Numberformat.Format = "@";
                ws.Cells[4 + tr, 1, 4 + tr, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[4 + tr, 7, 4 + tr, 8].Style.Numberformat.Format = "_-* #,##0_-;-* #,##0_-;_-* \"-\"??_-;_-@_-";
                ws.Cells[4 + tr, 7, 4 + tr, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                ws.Cells[4 + tr, 1, 4 + tr, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ws.Cells[4, 1, 4 + tr, 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[4, 1, 4 + tr, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[4, 1, 4 + tr, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[4, 1, 4 + tr, 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                #endregion                
                #endregion

                #endregion
            }
            else
            {
                #region Data Tidak Tersedia
                ws.Cells[5, 1, 6, 9].Value = "DATA TIDAK TERSEDIA";
                ws.Cells[5, 1, 6, 9].Merge = true;

                ws.Cells[4, 1, 6, 9].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[4, 1, 6, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[4, 1, 6, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[4, 1, 6, 9].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                ws.Cells[4, 1, 6, 9].Style.Numberformat.Format = "@";
                ws.Cells[4, 1, 6, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[4, 1, 6, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                #endregion
            }
            #endregion

            return ws;
        }

        public ExcelWorksheet KartuChecklist(ExcelPackage pck, int Id)
        {
            #region Create Excel
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("sheet1");
            ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
            ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
            if (System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft)   // Right to Left for Arabic lang
            {
                ExcelWorksheetView wv = ws.View;
                wv.ZoomScale = 70;
                wv.RightToLeft = true;
                ws.PrinterSettings.Orientation = eOrientation.Portrait;
                ws.Cells.AutoFitColumns();
            }
            else
            {
                ExcelWorksheetView wv = ws.View;
                wv.ZoomScale = 70;
                wv.RightToLeft = false;
                ws.PrinterSettings.Orientation = eOrientation.Portrait;
                ws.Cells.AutoFitColumns();
            }
            #endregion

            #region Create Judul
            var vendor = _context.Main.Include(x => x.Vendor).SingleOrDefault(x => x.Id == Id);

            ws.Cells[2, 2].Value = "KARTU CHECKLIST";
            ws.Cells[2, 7].Value = "NAMA VENDOR";
            ws.Cells[3, 7].Value = vendor.Vendor.Nama;

            ws.Cells[2, 7, 2, 8].Merge = true;
            ws.Cells[3, 7, 3, 8].Merge = true;
            ws.Cells[1, 1, 3, 8].Style.Numberformat.Format = "@";
            ws.Cells[2, 2].Style.Font.Size = 16;
            ws.Cells[2, 7, 2, 8].Style.Font.Size = 11;
            ws.Cells[3, 7, 3, 8].Style.Font.Size = 16;
            ws.Cells[1, 1, 3, 8].Style.Font.Bold = true;
            ws.Cells[2, 7, 2, 8].Style.Font.Color.SetColor(Color.White);
            ws.Cells[2, 7, 2, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[2, 7, 2, 8].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#31869B"));
            ws.Cells[2, 7, 3, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[1, 1, 3, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            ws.Cells[2, 7, 3, 8].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            ws.Cells[2, 7, 3, 8].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            ws.Cells[2, 7, 3, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            ws.Cells[2, 7, 3, 8].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            #endregion

            #region Create Thead Tabel                
            ws.Cells[5, 2].Value = "NO";
            ws.Cells[5, 3].Value = "NO MEMO";
            ws.Cells[5, 4].Value = "NO KONTRAK";
            ws.Cells[5, 5].Value = "NILAI KONTRAK";
            ws.Cells[5, 6].Value = "TERMIN";
            ws.Cells[5, 7].Value = "NOMINAL TERMIN";
            ws.Cells[5, 8].Value = "SISA";

            #region Style Thead
            ws.Cells[5, 2, 5, 8].Style.Numberformat.Format = "@";
            ws.Cells[5, 2, 5, 8].Style.Font.Size = 11;
            ws.Cells[5, 2, 5, 8].Style.Font.Bold = true;
            ws.Cells[5, 2, 5, 8].Style.Font.Color.SetColor(Color.White);
            ws.Cells[5, 2, 5, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[5, 2, 5, 8].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#31869B"));
            ws.Cells[5, 2, 5, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[5, 2, 5, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[5, 2, 5, 8].Style.WrapText = true;

            ws.Column(1).Width = 2;
            ws.Column(2).Width = 5;
            ws.Column(3).Width = 21;
            ws.Column(4).Width = 21;
            ws.Column(5).Width = 36;
            ws.Column(6).Width = 10;
            ws.Column(7).Width = 22;
            ws.Column(8).Width = 22;
            ws.Column(9).Width = 2;
            #endregion
            #endregion

            #region Create Content Tabel
            var last = 0;
            List<TransViewVM> data = _context.TransMainDetail.Include(x => x.Trans.KodeSurat).Where(x => x.Trans.StatusId >= 6 && x.MainDetail.MainId == Id && x.MainDetail.Main.IsDelete == false && x.MainDetail.IsDelete == false && x.TotalNominal > 0).Select(x => new TransViewVM
            {
                Id = x.TransId,
                KodeSurat = x.Trans.KodeSurat,
                MainDetail = x.MainDetail,
                Nomor = x.Trans.Nomor,
                DocDate = x.Trans.DocDate,
                Termin = x.Trans.Termin,
                TotalNominal = x.Trans.TotalNominal,
                DetailTotalNominal = x.TotalNominal
            }).OrderBy(x => x.MainDetail.Id).ThenBy(x => x.Id).ToList();

            if (data.Count() > 0)
            {
                #region Data Tersedia
                int tr = 1, no = 1;

                decimal SisaNominal = 0;
                var MaindetailId = 0;
                foreach (var item in data)
                {
                    if (MaindetailId == 0 || MaindetailId != item.MainDetail.Id)
                    {
                        if (MaindetailId != 0)
                        {
                            var dokumen = _context.JenisDokumen.SingleOrDefault(x => x.Id == item.MainDetail.JenisDokumenId);
                            ws.Cells[5 + tr, 4].Value = "NO " + dokumen.Nama.ToUpper() + " " + item.MainDetail.Index;
                            ws.Cells[5 + tr, 5].Value = "NILAI " + dokumen.Nama.ToUpper() + " " + item.MainDetail.Index;

                            #region Style
                            ws.Cells[5 + tr, 4, 5 + tr, 5].Style.Numberformat.Format = "@";
                            ws.Cells[5 + tr, 4, 5 + tr, 5].Style.Font.Size = 11;
                            ws.Cells[5 + tr, 4, 5 + tr, 5].Style.Font.Bold = true;
                            ws.Cells[5 + tr, 4, 5 + tr, 5].Style.Font.Color.SetColor(Color.White);
                            ws.Cells[5 + tr, 4, 5 + tr, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            ws.Cells[5 + tr, 4, 5 + tr, 5].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#31869B"));
                            ws.Cells[5 + tr, 4, 5 + tr, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[5 + tr, 4, 5 + tr, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            #endregion

                            tr++;
                        }

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

                    ws.Cells[5 + tr, 2].Value = no;
                    ws.Cells[5 + tr, 3].Value = item.KodeSurat.Nama + item.Nomor + " TGL." + item.DocDate.ToShortDateString();
                    ws.Cells[5 + tr, 4].Value = item.MainDetail.Nomor + " TGL." + item.MainDetail.DocDate.ToShortDateString();
                    ws.Cells[5 + tr, 5].Value = item.Uraian;
                    ws.Cells[5 + tr, 6].Value = item.Termin;
                    ws.Cells[5 + tr, 7].Value = item.DetailTotalNominal;
                    ws.Cells[5 + tr, 8].Value = item.SisaNominal;

                    #region Style
                    ws.Cells[5 + tr, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    ws.Cells[5 + tr, 3, 5 + tr, 6].Style.Numberformat.Format = "@";
                    ws.Cells[5 + tr, 3, 5 + tr, 4].Style.WrapText = true;
                    ws.Cells[5 + tr, 7, 5 + tr, 8].Style.Numberformat.Format = "_-* #,##0_-;-* #,##0_-;_-* \"-\"??_-;_-@_-";
                    ws.Cells[5 + tr, 7, 5 + tr, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    ws.Cells[5 + tr, 1, 5 + tr, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    #endregion

                    no++;
                    tr++;

                }

                #region Style
                ws.Cells[5, 2, 4 + tr, 8].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 2, 4 + tr, 8].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 2, 4 + tr, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[5, 2, 4 + tr, 8].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                #endregion
                last = 5 + tr;
                #endregion
            }
            else
            {
                #region Data Tidak Tersedia
                ws.Cells[6, 2].Value = "DATA TIDAK TERSEDIA";
                ws.Cells[6, 2, 7, 8].Merge = true;

                ws.Cells[6, 2, 7, 8].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[6, 2, 7, 8].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[6, 2, 7, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[6, 2, 7, 8].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                ws.Cells[5, 2, 7, 8].Style.Numberformat.Format = "@";
                ws.Cells[5, 2, 7, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[5, 2, 7, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                last = 8;
                #endregion
            }

            #region Style Border
            ws.Cells[1, 1, 1, 9].Style.Border.Top.Style = ExcelBorderStyle.Thick;
            ws.Cells[1, 9, last, 9].Style.Border.Right.Style = ExcelBorderStyle.Thick;
            ws.Cells[last, 1, last, 9].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
            ws.Cells[1, 1, last, 1].Style.Border.Left.Style = ExcelBorderStyle.Thick;
            #endregion
            #endregion

            return ws;
        }

        public ExcelWorksheet KartuKuning(ExcelPackage pck, int Id)
        {
            #region Create Excel
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("sheet1");
            ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
            ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
            if (System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft)   // Right to Left for Arabic lang
            {
                ExcelWorksheetView wv = ws.View;
                wv.ZoomScale = 70;
                wv.RightToLeft = true;
                ws.PrinterSettings.Orientation = eOrientation.Portrait;
                ws.Cells.AutoFitColumns();
            }
            else
            {
                ExcelWorksheetView wv = ws.View;
                wv.ZoomScale = 70;
                wv.RightToLeft = false;
                ws.PrinterSettings.Orientation = eOrientation.Portrait;
                ws.Cells.AutoFitColumns();
            }
            #endregion

            #region Content
            List<TransViewVM> data = _context.TransMainDetail.Include(x => x.Trans.KodeSurat).Where(x => x.Trans.StatusId >= 6 && x.MainDetail.MainId == Id && x.MainDetail.Main.IsDelete == false && x.MainDetail.IsDelete == false && x.TotalNominal > 0).Select(x => new TransViewVM
            {
                Id = x.TransId,
                KodeSurat = x.Trans.KodeSurat,
                MainDetail = x.MainDetail,
                Nomor = x.Trans.Nomor,
                Uraian = x.Trans.Uraian,
                DocDate = x.Trans.DocDate,
                Termin = x.Trans.Termin,
                TotalNominal = x.Trans.TotalNominal,
                DetailTotalNominal = x.TotalNominal,
                Vendor = x.MainDetail.Main.Vendor
            }).OrderBy(x => x.MainDetail.Id).ThenBy(x => x.Id).ToList();

            if (data.Count() > 0)
            {
                #region Data Tersedia
                int tr = 0, no = 1;

                //decimal SisaNominal = 0;
                //var MaindetailId = 0;
                foreach (var item in data)
                {
                    #region Create Judul
                    ws.Cells[1 + tr, 1].Value = "Nama Rekanan";
                    ws.Cells[1 + tr, 3].Value = ": " + item.Vendor.Nama;
                    ws.Cells[1 + tr, 7].Value = "Termin : " + item.Termin;                    
                    ws.Cells[1 + tr, 8].Value = "Dari : ";
                    ws.Cells[1 + tr, 9].Value = "No Rekening";
                    ws.Cells[1 + tr, 10].Value = ": " + item.MainDetail.NoRek;
                    ws.Cells[2 + tr, 1].Value = "Nomor/Tanggal Memo";
                    ws.Cells[2 + tr, 3].Value = ": " + item.KodeSurat.Nama + item.Nomor + " Tgl :" + item.DocDate.ToShortDateString();
                    ws.Cells[2 + tr, 7].Value = "Perjanjian/Kontrak/SPK";
                    ws.Cells[2 + tr, 8].Value = ": No " + item.MainDetail.Nomor + " Tgl :" + item.MainDetail.DocDate.ToShortDateString();
                    #endregion

                    #region Create Thead Tabel                
                    ws.Cells[3 + tr, 1].Value = "NO";
                    ws.Cells[3 + tr, 2].Value = "JUMLAH / NILAI TAGIHAN";
                    ws.Cells[3 + tr, 3].Value = "RINCIAN PEMBAYARAN";
                    ws.Cells[3 + tr, 6].Value = "KETERANGAN";
                    ws.Cells[3 + tr, 10].Value = "PENGESAHAN";
                    ws.Cells[4 + tr, 3].Value = "FORM";
                    ws.Cells[4 + tr, 4].Value = "JUMLAH / NILAI";
                    ws.Cells[4 + tr, 10].Value = "PYN";
                    ws.Cells[4 + tr, 11].Value = "PEMIMPIN";

                    #region Style Thead
                    ws.Cells[3 + tr, 1, 4 + tr, 1].Merge = true;
                    ws.Cells[3 + tr, 2, 4 + tr, 2].Merge = true;
                    ws.Cells[3 + tr, 3, 3 + tr, 5].Merge = true;
                    ws.Cells[3 + tr, 6, 4 + tr, 9].Merge = true;
                    ws.Cells[3 + tr, 10, 3 + tr, 11].Merge = true;
                    ws.Cells[4 + tr, 4, 4 + tr, 5].Merge = true;

                    ws.Cells[3 + tr, 1, 4 + tr, 11].Style.Numberformat.Format = "@";
                    ws.Cells[3 + tr, 1, 4 + tr, 11].Style.Font.Size = 11;
                    ws.Cells[3 + tr, 1, 4 + tr, 11].Style.Font.Bold = true;
                    ws.Cells[3 + tr, 1, 4 + tr, 11].Style.Font.Color.SetColor(Color.White);
                    ws.Cells[3 + tr, 1, 4 + tr, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[3 + tr, 1, 4 + tr, 11].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#31869B"));
                    ws.Cells[3 + tr, 1, 4 + tr, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[3 + tr, 1, 4 + tr, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[3 + tr, 1, 4 + tr, 11].Style.WrapText = true;

                    ws.Column(1).Width = 5;
                    ws.Column(2).Width = 23;
                    ws.Column(3).Width = 9;
                    ws.Column(4).Width = 13;
                    ws.Column(5).Width = 13;
                    ws.Column(6).Width = 5;
                    ws.Column(7).Width = 24;
                    ws.Column(8).Width = 24;
                    ws.Column(9).Width = 24;
                    ws.Column(10).Width = 15;
                    ws.Column(11).Width = 15;
                    #endregion
                    #endregion

                    #region Create Content Tabel
                    var i = 0;
                    var TransRekeningKredit = _context.TransRekening.Where(x => x.TransId == item.Id && x.IsMain == false && x.IsDebit == false).ToList();

                    ws.Cells[5 + tr, 1].Value = no;
                    ws.Cells[5 + tr, 2].Value = item.TotalNominal;
                    ws.Cells[5 + tr, 6].Value = item.Uraian;

                    foreach (var item2 in TransRekeningKredit)
                    {
                        if (item2.Nominal > 0)
                        {
                            if (i == 0)
                            {
                                ws.Cells[5 + tr, 3].Value = "CN";
                            }
                            else
                            {
                                ws.Cells[5 + tr, 3].Value = item2.Nama;
                            }
                            ws.Cells[5 + tr, 4].Value = item2.Nominal;

                            #region Style
                            ws.Cells[5 + tr, 3].Style.Numberformat.Format = "@";
                            ws.Cells[5 + tr, 4, 5 + tr, 5].Merge = true;
                            ws.Cells[5 + tr, 4, 5 + tr, 5].Style.Numberformat.Format = "_-* #,##0_-;-* #,##0_-;_-* \"-\"??_-;_-@_-";
                            ws.Cells[5 + tr, 4, 5 + tr, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            #endregion

                            i++;
                            tr++;
                        }
                    }

                    #region Style
                    ws.Cells[5 + tr - i, 1, 4 + tr, 1].Merge = true;
                    ws.Cells[5 + tr - i, 2, 4 + tr, 2].Merge = true;
                    ws.Cells[5 + tr - i, 6, 4 + tr, 9].Merge = true;

                    ws.Cells[5 + tr - i, 2, 4 + tr, 2].Style.Numberformat.Format = "_-* #,##0_-;-* #,##0_-;_-* \"-\"??_-;_-@_-";
                    ws.Cells[5 + tr - i, 2, 4 + tr, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[5 + tr - i, 6, 4 + tr, 9].Style.Numberformat.Format = "@";
                    #endregion
                    #endregion

                    #region Style
                    ws.Cells[3 + tr - i, 1, 4 + tr, 11].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[3 + tr - i, 1, 4 + tr, 11].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    ws.Cells[3 + tr - i, 1, 4 + tr, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[3 + tr - i, 1, 4 + tr, 11].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                    ws.Cells[3 + tr - i, 1, 4 + tr, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[5 + tr - i, 2, 4 + tr, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[5 + tr - i, 2, 4 + tr, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    #endregion
                    no++;
                    tr++;
                    tr++;
                    tr++;
                    tr++;
                    tr++;
                }
                
                //last = 5 + tr;
                #endregion
            }
            #endregion                        

            return ws;
        }

        public void RekapPPN(DateTime StartDate, DateTime EndDate)
        {

            ExcelPackage pck = new ExcelPackage();

            var sheets = _context.SubJenisPotongan.Where(x => x.JenisPotonganId == 3).GroupBy(x => x.Nama2).Select(x => x.FirstOrDefault().Nama2).ToList();

            foreach (var item in sheets)
            {
                ExcelWorksheet ws = PPN(pck, StartDate, EndDate, item);
            }


            HttpContext.Response.Clear();
            HttpContext.Response.AddHeader("", "");
            HttpContext.Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            HttpContext.Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            HttpContext.Response.AddHeader("content-disposition", "attachment;  filename=Rekap PPN " + StartDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID")) + " - " + EndDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID")) + " .xlsx");
            HttpContext.Response.ContentType = "application/text";
            HttpContext.Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
            HttpContext.Response.End();
        }

        public void RekapPPH(DateTime StartDate, DateTime EndDate)
        {
            ExcelPackage pck = new ExcelPackage();

            var sheets = _context.SubJenisPotongan.Where(x => x.JenisPotonganId == 4).GroupBy(x => x.Nama2).Select(x => x.FirstOrDefault().Nama2).ToList();

            foreach (var item in sheets)
            {
                ExcelWorksheet ws = PPH(pck, StartDate, EndDate, item);
            }


            HttpContext.Response.Clear();
            HttpContext.Response.AddHeader("", "");
            HttpContext.Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            HttpContext.Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            HttpContext.Response.AddHeader("content-disposition", "attachment;  filename=Rekap PPH " + StartDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID")) + " - "+ EndDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID")) + " .xlsx");
            HttpContext.Response.ContentType = "application/text";
            HttpContext.Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
            HttpContext.Response.End();

            //var pathfile = "Data Debit " + DateTime.Now.ToString("ddMMyyyy ", new System.Globalization.CultureInfo("id-ID")) + " - " + string.Format(@"{0}", DateTime.Now.Ticks) + " .xlsx";
            //string path = Server.MapPath("~/Files/Rekening Debit/" + pathfile);

            //pck.SaveAs(new FileInfo(path));
        }

        public void RekapKartuChecklist(int Id)
        {

            ExcelPackage pck = new ExcelPackage();

            ExcelWorksheet ws = KartuChecklist(pck, Id);

            HttpContext.Response.Clear();
            HttpContext.Response.AddHeader("", "");
            HttpContext.Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            HttpContext.Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            HttpContext.Response.AddHeader("content-disposition", "attachment;  filename=Rekap Kartu Checklist " + DateTime.Now.ToShortDateString().ToString() + " .xlsx");
            HttpContext.Response.ContentType = "application/text";
            HttpContext.Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
            HttpContext.Response.End();
        }

        public void RekapKartuKuning(int Id)
        {

            ExcelPackage pck = new ExcelPackage();

            ExcelWorksheet ws = KartuKuning(pck, Id);

            HttpContext.Response.Clear();
            HttpContext.Response.AddHeader("", "");
            HttpContext.Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            HttpContext.Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            HttpContext.Response.AddHeader("content-disposition", "attachment;  filename=Rekap Kartu Kuning " + DateTime.Now.ToShortDateString().ToString() + " .xlsx");
            HttpContext.Response.ContentType = "application/text";
            HttpContext.Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            HttpContext.Response.BinaryWrite(pck.GetAsByteArray());
            HttpContext.Response.End();
        }        

        public ActionResult UploadData()
        {
            //var ven = _context.Vendor.ToList();
            //foreach (var item in ven)
            //{
            //    var trans = _context.Vendor.FirstOrDefault(x => x.Id == item.Id);
            //    if (trans != null)
            //    {
            //        trans.NamaRek = item.Nama;
            //        _context.Entry(trans).State = EntityState.Modified;
            //        _context.SaveChanges();
            //    }
            //}

            return View();
        }

        public JsonResult UploadDebit(HttpPostedFileBase file)
        {            
            var result = false;

            if (file != null)
            {
                if (file.FileName.EndsWith("xlsx") || file.FileName.EndsWith("XLSX"))
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
                                var nama = worksheet.Cells[i, 1].Text.ToUpper();
                                //var nama = worksheet.Cells[i, 2].Text.Replace(" ", "").Replace(",", "").Replace(".", "").Replace("'", "").ToUpper();
                                if (nama.Trim() != "")
                                {
                                    var trans = _context.Vendor.FirstOrDefault(x => nama.Contains(x.NamaRek));
                                    //var trans = _context.Vendor.FirstOrDefault(x => x.NamaRek == nama);
                                    if (trans != null)
                                    {
                                        trans.NoRek = worksheet.Cells[i, 2].Text;
                                        trans.Cabang = worksheet.Cells[i, 3].Text;
                                        trans.BankId = 3;

                                        //trans.NPWP = worksheet.Cells[i, 1].Text;
                                        //trans.Alamat = worksheet.Cells[i, 3].Text;
                                        _context.Entry(trans).State = EntityState.Modified;
                                        _context.SaveChanges();
                                    }                                    
                                }
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

            return Json(new { result = result }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadDebit2(HttpPostedFileBase file)
        {
            //List<SubJenisAttch> Datas = new List<SubJenisAttch>();
            //List<Vendor> Datas = new List<Vendor>();
            List<NoRekMCOA> Datas = new List<NoRekMCOA>();
            //List<NoRekCabang> Datas = new List<NoRekCabang>();
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
                                //Vendor data = new Vendor();
                                //SubJenisAttch data = new SubJenisAttch();
                                //NoRekCabang data = new NoRekCabang();
                                NoRekMCOA data = new NoRekMCOA();

                                //data.Nama = worksheet.Cells[i, 1].Text;
                                //data.JenisAttchId = 6;
                                //Datas.Add(data);

                                //string str = worksheet.Cells[i, 2].Text;

                                //if (str.Trim() != "-")
                                //{
                                //    if (str.Trim().Length == 1)
                                //    {
                                //        str = "00" + str;
                                //    }
                                //    else if (str.Trim().Length == 2)
                                //    {
                                //        str = "0" + str;
                                //    }

                                //    data.No = str;
                                //    data.Nama = worksheet.Cells[i, 1].Text.ToUpper();
                                //    //data.CreateDate = DateTime.Now;
                                //    data.IsActive = true;

                                //    Datas.Add(data);
                                //}                                

                                string str1 = worksheet.Cells[i, 1].Text;
                                string str2 = worksheet.Cells[i, 2].Text;

                                if (str2.Trim() != "")
                                {
                                    //if (str1.Trim() != "")
                                    //{
                                    //    data.No = str1;
                                    //}
                                    //else if (str2.Trim() != "")
                                    //{

                                    //}

                                    data.No = str2;
                                    data.Nama = worksheet.Cells[i, 3].Text;
                                    //data.CreateDate = DateTime.Now;
                                    data.IsActive = true;

                                    Datas.Add(data);
                                }


                            }

                            _context.NoRekMCOA.AddRange(Datas);
                            //_context.NoRekCabang.AddRange(Datas);
                            //_context.SubJenisAttch.AddRange(Datas);
                            //_context.Vendor.AddRange(Datas);
                            _context.SaveChanges();

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
    }
}