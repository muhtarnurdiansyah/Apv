using Apv.Models.Core;
using Apv.Models.Master;
using System;

namespace Apv.Models.Transaksi
{
    public class MainDetail : BaseModel
    {
        public int Id { get; set; }
        public string Nomor { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal TotalNominal { get; set; }
        public string TotalTermin { get; set; }
        public JenisDokumen JenisDokumen { get; set; }
        public int JenisDokumenId { get; set; }
        public int Index { get; set; }
        public string Path { get; set; }
        public string NoRek { get; set; }
        public string NamaRek { get; set; }
        public Bank Bank { get; set; }
        public int BankId { get; set; }
        public string Cabang { get; set; }
        public Main Main { get; set; }
        public int MainId { get; set; }
        public string NPWP { get; set; }
        public string Alamat { get; set; }
        public bool IsActive { get; set; }
    }
}