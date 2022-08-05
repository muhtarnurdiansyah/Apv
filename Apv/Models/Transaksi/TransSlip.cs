using Apv.Models.Master;
using System;

namespace Apv.Models.Transaksi
{
    public class TransSlip
    {
        public int Id { get; set; }
        public DateTime Tanggal { get; set; }
        public string NoReferensi { get; set; }
        public string NamaRekDebit { get; set; }
        public string NamaRekDebit2 { get; set; }
        public string NoRekDebit { get; set; }
        public string NoRekDebit2 { get; set; }
        public bool IsNoRekDebitVA { get; set; }
        public string NamaCabangDebit { get; set; }
        public JenisRekening JenisRekeningDebit { get; set; }
        public int? JenisRekeningDebitId { get; set; }
        public string PesanDebit { get; set; }
        public string PesanDebit2 { get; set; }
        public Currency CurrencyDebit { get; set; }
        public int? CurrencyDebitId { get; set; }
        public decimal NominalDebit { get; set; }
        public string NamaRekKredit { get; set; }
        public string NoRekKredit { get; set; }
        public string NoRekKredit2 { get; set; }
        public bool IsNoRekKreditVA { get; set; }
        public string NamaCabangKredit { get; set; }
        public JenisRekening JenisRekeningKredit { get; set; }
        public int? JenisRekeningKreditId { get; set; }
        public Bank BankKredit { get; set; }
        public int? BankKreditId { get; set; }
        public Currency CurrencyKredit { get; set; }
        public int? CurrencyKreditId { get; set; }
        public decimal NominalKredit { get; set; }
        public string AddKredit { get; set; }
        public string AddKredit2 { get; set; }
        public string PhoneKredit { get; set; }
        public string CityCodeKredit { get; set; }
        public string IdKredit { get; set; }
        public string IdTypeKredit { get; set; }
        public string SandiTXN { get; set; }
        public string NoJurnal { get; set; }
        public string Keterangan1 { get; set; }
        public string Keterangan2 { get; set; }
        public string Keterangan3 { get; set; }        
        public decimal Biaya { get; set; }
        public decimal Kurs { get; set; }
        public JenisSlip JenisSlip { get; set; }
        public int? JenisSlipId { get; set; }
        public OutputSlip OutputSlip { get; set; }
        public int? OutputSlipId { get; set; }
        public Trans Trans { get; set; }
        public int TransId { get; set; }        
    }
}