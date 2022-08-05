namespace Apv.Models.Master
{
    public class SubJenisAttch
    {
        public int Id { get; set; }
        public string Nama { get; set; }
        public JenisAttch JenisAttch { get; set; }
        public int JenisAttchId { get; set; }
    }
}