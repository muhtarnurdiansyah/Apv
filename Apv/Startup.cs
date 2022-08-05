using Apv.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Apv.Startup))]
namespace Apv
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateRoles();
            var idProvider = new CustomUserIdProvider();
            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => idProvider);
            app.MapSignalR();
        }

        private void CreateRoles()
        {
            ApplicationDbContext _context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));

            // Creating role
            #region Modul
            if (!roleManager.RoleExists("Modul Admin"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Modul Admin";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Modul Inputer"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Modul Inputer";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Modul Approver"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Modul Approver";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Modul Verificator"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Modul Verificator";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Modul Verificator Supervisor"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Modul Verificator Supervisor";
                roleManager.Create(role);
            }            

            if (!roleManager.RoleExists("Modul Riwayat"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Modul Riwayat";
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("Modul SLA"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Modul SLA";
                roleManager.Create(role);
            }
            #endregion            
        }
    }
}
