using Apv.Models.Core;

namespace Apv.Models.Master
{
    public class Vendor : BaseModel
    {
        public int Id { get; set; }
        public string Nama { get; set; }
        public string NoRek { get; set; }
        public string NamaRek { get; set; }
        public Bank Bank { get; set; }
        public int? BankId { get; set; }
        public string Cabang { get; set; }
        public string NPWP { get; set; }
        public string Alamat { get; set; }
    }
}