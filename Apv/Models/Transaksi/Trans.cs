using Apv.Models.Core;
using Apv.Models.Master;
using System;

namespace Apv.Models.Transaksi
{
    public class Trans : BaseModel
    {
        public int Id { get; set; }
        public KodeSurat KodeSurat { get; set; }
        public int KodeSuratId { get; set; }
        public string Nomor { get; set; }
        public string Uraian { get; set; }
        public DateTime DocDate { get; set; }
        public string Prestasi { get; set; }
        public string Termin { get; set; }
        public decimal TotalNominal { get; set; }
        public Status Status { get; set; }
        public int StatusId { get; set; }
        public DateTime? UploadTax { get; set; }
        public string PathTax { get; set; }
        public virtual ApplicationUser UploaderTax { get; set; }
        public string UploaderTaxId { get; set; }
        public string NomorReg { get; set; }
        public string NomorCN { get; set; }
        public string NomorCNPPN { get; set; }
        public string NomorPP { get; set; }
    }
}