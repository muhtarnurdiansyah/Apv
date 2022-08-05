using Apv.Models.Core;
using Apv.Models.Master;
using System;

namespace Apv.Models.Transaksi
{
    public class Main : BaseModel
    {
        public int Id { get; set; }
        public string Uraian { get; set; }
        public bool IsActive { get; set; }
        public Vendor Vendor { get; set; }
        public int VendorId { get; set; }
    }
}