using Apv.Models.Core;
using Apv.Models.Master;
using System;

namespace Apv.Models.Transaksi
{
    public class TransMainDetail
    {
        public int Id { get; set; }
        public decimal TotalNominal { get; set; }
        public MainDetail MainDetail { get; set; }
        public int MainDetailId { get; set; }
        public Trans Trans { get; set; }
        public int TransId { get; set; }
    }
}