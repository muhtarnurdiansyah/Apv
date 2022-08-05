using Apv.Models.Master;

namespace Apv.Models.Setting
{
    public class MappingRole
    {
        public int Id { get; set; }
        public Unit Unit { get; set; }
        public int UnitId { get; set; }
        public Jabatan Jabatan { get; set; }
        public int JabatanId { get; set; }
    }
}