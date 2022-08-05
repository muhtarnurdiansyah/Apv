using System;

namespace Apv.Models.Transaksi
{
    public class TransPengadaan
    {
        public int Id { get; set; }
        public string Nama { get; set; }
        public decimal Nominal { get; set; }
        public Trans Trans { get; set; }
        public int TransId { get; set; }
    }
}