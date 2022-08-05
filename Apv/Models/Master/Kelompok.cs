namespace Apv.Models.Master
{
    public class Kelompok
    {
        public int Id { get; set; }
        public string Singkatan { get; set; }
        public string Nama { get; set; }
        public Wilayah Wilayah { get; set; }
        public int WilayahId { get; set; }
    }
}