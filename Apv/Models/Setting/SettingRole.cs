using Apv.Models.Core;
using Apv.Models.Master;
using System;

namespace Apv.Models.Setting
{
    public class SettingRole : BaseModel
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsDefault { get; set; }
        public MappingRole MappingRole { get; set; }
        public int MappingRoleId { get; set; }        
        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser Creater { get; set; }
        public string CreaterId { get; set; }
    }
}