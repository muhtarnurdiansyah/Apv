namespace Apv.Models.Master
{
    public class Unit
    {
        public int Id { get; set; }
        public string Nama { get; set; }
        public Kelompok Kelompok { get; set; }
        public int KelompokId { get; set; }
    }
}