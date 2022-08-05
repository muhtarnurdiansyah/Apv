using Apv.Models;
using Apv.Models.Setting;
using System.Collections.Generic;

namespace Apv.ViewModels
{
    public class UserRoleVM
    {
        public ApplicationUser User { get; set; }
        public IEnumerable<SettingRole> SettingRole { get; set; }
    }
    public class MappingRoleVM
    {
        public int Id { get; set; }
        public int JabatanId { get; set; }
        public int UnitId { get; set; }
        public IEnumerable<string> RoleId { get; set; }
    }
}