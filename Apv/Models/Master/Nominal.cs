using Apv.Models.Core;
using System;

namespace Apv.Models.Master
{
    public class Nominal : BaseModel
    {
        public int Id { get; set; }
        public Kelompok Kelompok { get; set; }
        public int KelompokId { get; set; }
        public Jabatan Jabatan { get; set; }
        public int JabatanId { get; set; }
        public Int64 Min { get; set; }
        public Int64 Max { get; set; }
    }
}