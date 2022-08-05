using Apv.Models.Core;
using Apv.Models.Master;
using System;

namespace Apv.Models.Transaksi
{
    public class TransRekening
    {
        public int Id { get; set; }
        public string NoRek { get; set; }
        public string NoRek2 { get; set; }
        public string Nama { get; set; }
        public Bank Bank { get; set; }
        public int? BankId { get; set; }
        public string Cabang { get; set; }
        public Currency Currency { get; set; }
        public int CurrencyId { get; set; }
        public decimal Nominal { get; set; }        
        public bool IsMain { get; set; }
        public bool IsDebit { get; set; }
        public Trans Trans { get; set; }
        public int TransId { get; set; }
    }
}