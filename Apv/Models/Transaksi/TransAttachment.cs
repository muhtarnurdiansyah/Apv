using Apv.Models.Master;
using System;

namespace Apv.Models.Transaksi
{
    public class TransAttachment
    {
        public int Id { get; set; }
        public string Nomor { get; set; }
        public SubJenisAttch SubJenisAttch { get; set; }
        public int SubJenisAttchId { get; set; }
        public int Jumlah { get; set; }
        public string Path { get; set; }
        public DateTime DocDate { get; set; }
        public Trans Trans { get; set; }
        public int TransId { get; set; }
        public OutputAttch OutputAttch { get; set; }
        public int OutputAttchId { get; set; }
    }
}