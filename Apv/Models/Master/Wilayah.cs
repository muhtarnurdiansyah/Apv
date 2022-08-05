namespace Apv.Models.Master
{
    public class Wilayah
    {
        public int Id { get; set; }
        public string Nama { get; set; }
        public string Singkatan { get; set; }
        public Divisi Divisi { get; set; }
        public int DivisiId { get; set; }
    }
}