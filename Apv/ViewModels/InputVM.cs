using Apv.Models.Master;
using Apv.Models.Setting;
using Apv.Models.Transaksi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Apv.ViewModels
{
    public class MainVM
    {
        public Main Main { get; set; }
        public MainDetail MainDetail { get; set; }
    }
    public class AddVM
    {
        public int Id { get; set; }
        public List<KodeSurat> KodeSurat { get; set; }
        public List<MainDetail> Rekanan { get; set; }
        public List<SubJenisPotongan> PPN { get; set; }
        public List<SubJenisPotongan> PPH { get; set; }
        public SubJenisPotongan Denda { get; set; }
        public List<Bank> Bank { get; set; }
    }
    public class TransRekeningVM
    {
        public string NoRek { get; set; }
        public string Nama { get; set; }
        public decimal Nominal { get; set; }
    }
    public class TransVM
    {
        public Trans Trans { get; set; }
        public IEnumerable<MainDetail> MainDetail { get; set; }
        public IEnumerable<TransMainDetail> TransMainDetail { get; set; }
        public IEnumerable<TransMainDetailVM> TransMainDetailVM { get; set; }
        public IEnumerable<TransPengadaan> TransPengadaan { get; set; }
        public TransPotongan TransPotonganMaterai { get; set; }
        public TransPotongan TransPotonganDenda { get; set; }
        public IEnumerable<TransPotongan> TransPotonganPPN { get; set; }
        public IEnumerable<TransPotongan> TransPotonganPPH { get; set; }
        public IEnumerable<TransPotongan> TransPotongan { get; set; }
        public TransRekening TransRekeningMain { get; set; }
        public IEnumerable<TransRekening> TransRekeningDebit { get; set; }
        public IEnumerable<TransRekening> TransRekeningKredit { get; set; }
        public IEnumerable<TransRekening> TransRekening { get; set; }
        public List<TransAttachment> TransAttachment { get; set; }
        public IEnumerable<TransTracking> TransTracking { get; set; }
        public List<SubJenisAttch> SubJenisAttch { get; set; }
        public List<OutputAttch> OutputAttch { get; set; }
    }
    public class TransAttchVM
    {
        public int Id { get; set; }
        public string Nomor { get; set; }
        //public string Nama { get; set; }
        public int Jumlah { get; set; }
        public string Path { get; set; }
        public DateTime DocDate { get; set; }
        public int SubJenisAttchId { get; set; }
        public int OutputAttchId { get; set; }
        public string KeyFile { get; set; }
    }
    public class ContractVM
    {
        public List<Vendor> Vendor { get; set; }
        public List<Bank> Bank { get; set; }
    }
    public class TransViewVM
    {
        public int Id { get; set; }        
        public KodeSurat KodeSurat { get; set; }
        public string Nomor { get; set; }
        public string Uraian { get; set; }
        public DateTime DocDate { get; set; }
        public string Prestasi { get; set; }
        public string Termin { get; set; }
        public decimal TotalNominal { get; set; }
        public decimal DetailTotalNominal { get; set; }
        public decimal SisaNominal { get; set; }
        public Status Status { get; set; }
        public Vendor Vendor { get; set; }
        public SubJenisPotongan SubJenisPotongan { get; set; }
        public MainDetail MainDetail { get; set; }
        public bool IsTakeBack { get; set; }
    }
    public class TransMainDetailVM
    {
        public int Id { get; set; }
        public MainDetail MainDetail { get; set; }
        public decimal TotalRealisasi { get; set; }
        public decimal TotalNominal { get; set; }
    }
    public class TransSlipsVM
    {
        public Trans Trans { get; set; }        
        public IEnumerable<TransSlip> TransSlip { get; set; }
        public IEnumerable<SettingSlip> SettingSlip { get; set; }
    }
    public class InputSlipVM
    {
        public int Id { get; set; }
        public TransSlip TransSlip { get; set; }
        public IEnumerable<Currency> Currency { get; set; }
        public IEnumerable<Bank> Bank { get; set; }
        public IEnumerable<JenisRekening> JenisRekening { get; set; }
        public IEnumerable<SettingSlip> SettingSlips { get; set; }
        public SettingSlip SettingSlip { get; set; }
    }
    public class FilterList
    {
        public List<KodeSurat> ListKodeSurat { get; set; }
        public List<Status> ListStatus { get; set; }
    }
    public class FilterData
    {
        public DateTime StartCreateDate { get; set; }
        public DateTime EndCreateDate { get; set; }
        public bool OptionCreateDate { get; set; }
        public DateTime StartDocDate { get; set; }
        public DateTime EndDocDate { get; set; }
        public bool OptionDocDate { get; set; }
        public int KodeSurat { get; set; }
        public string Nomor { get; set; }
        public bool OptionNomor { get; set; }  
        public int Status { get; set; }
        public bool OptionStatus { get; set; }
    }
}