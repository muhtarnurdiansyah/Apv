using Apv.Models.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Apv.Models.Setting
{
    public class SettingSlip
    {
        public int Id { get; set; }
        public bool Tanggal { get; set; }
        public bool NoReferensi { get; set; }
        public bool NamaRekDebit { get; set; }
        public bool NamaRekDebit2 { get; set; }
        public bool NoRekDebit { get; set; }
        public bool NoRekDebit2 { get; set; }
        public bool NoRekDebitText { get; set; }
        public bool NamaCabangDebit { get; set; }
        public bool JenisRekeningDebit { get; set; }
        public bool PesanDebit { get; set; }
        public bool PesanDebit2 { get; set; }
        public bool CurrencyDebit { get; set; }
        public bool NominalDebit { get; set; }
        public bool NamaRekKredit { get; set; }
        public bool NoRekKredit { get; set; }
        public bool NoRekKredit2 { get; set; }
        public bool NoRekKreditText { get; set; }
        public bool NamaCabangKredit { get; set; }
        public bool JenisRekeningKredit { get; set; }
        public bool BankKredit { get; set; }
        public bool CurrencyKredit { get; set; }
        public bool NominalKredit { get; set; }
        public bool AddKredit { get; set; }
        public bool AddKredit2 { get; set; }
        public bool PhoneKredit { get; set; }
        public bool CityCodeKredit { get; set; }
        public bool IdKredit { get; set; }
        public bool IdTypeKredit { get; set; }
        public bool SandiTXN { get; set; }
        public bool Keterangan1 { get; set; }
        public bool Keterangan2 { get; set; }
        public bool Keterangan3 { get; set; }       
        public bool Biaya { get; set; }
        public bool Kurs { get; set; }
        public JenisSlip JenisSlip { get; set; }
        public int JenisSlipId { get; set; }
        public OutputSlip OutputSlip { get; set; }
        public int OutputSlipId { get; set; }
        public Kelompok Kelompok { get; set; }
        public int KelompokId { get; set; }
        public virtual ApplicationUser Creater { get; set; }
        public string CreaterId { get; set; }
    }
}