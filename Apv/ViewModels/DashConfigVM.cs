using Apv.Models.Transaksi;
using System.Collections.Generic;

namespace Apv.ViewModels
{
    public class DashConfigVM
    {
        public string Judul { get; set; }
        public int Count { get; set; }
        public string Controller { get; set; }
        public string Method { get; set; }
        public string Warna { get; set; }
        public string Icon { get; set; }

    }
    public class DashVM
    {
        public int Id { get; set; }
        public string Judul { get; set; }
        public string Judul2 { get; set; }
        public string Controller { get; set; }
        public string Warna { get; set; }
        public string Icon { get; set; }
        public IEnumerable<DashItemVM> DashItemVM { get; set; }
    }
    public class DashItemVM
    {
        public string Judul { get; set; }
        public string Count { get; set; }
    }
    public class DetailDashVM
    {
        public int Id { get; set; }
        public MainDetail MainDetail { get; set; }
        public decimal Terbayar { get; set; }
        public decimal Outstanding { get; set; }
    }
}