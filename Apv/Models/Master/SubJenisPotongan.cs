namespace Apv.Models.Master
{
    public class SubJenisPotongan
    {
        public int Id { get; set; }
        public string Nama { get; set; }
        public string Nama2 { get; set; }
        public string NoRek { get; set; }
        public string NoRek2 { get; set; }
        public decimal Nilai { get; set; }
        public JenisPotongan JenisPotongan { get; set; }
        public int JenisPotonganId { get; set; }
    }
}