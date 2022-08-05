using Apv.Models.Master;
using System;

namespace Apv.Models.Transaksi
{
    public class TransPotongan
    {
        public int Id { get; set; }
        public decimal Nominal { get; set; }
        public decimal Total { get; set; }
        public Trans Trans { get; set; }
        public int TransId { get; set; }
        public SubJenisPotongan SubJenisPotongan { get; set; }
        public int SubJenisPotonganId { get; set; }
        public bool IsDone { get; set; }
    }
}