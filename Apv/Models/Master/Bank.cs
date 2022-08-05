using Apv.Models.Core;

namespace Apv.Models.Master
{
    public class Bank : BaseModel
    {
        public int Id { get; set; }
        public string Nama { get; set; }
        public string KodeBIC { get; set; }
        public string Singkatan { get; set; }
    }
}