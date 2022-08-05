using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using Apv.Models.Master;
using Apv.Models.Transaksi;
using Apv.Models.Setting;

namespace Apv.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Nama")]
        public string Nama { get; set; }
        [Required]
        [Display(Name = "NPP")]
        public string NPP { get; set; }
        public Unit Unit { get; set; }
        public int UnitId { get; set; }
        public Jabatan Jabatan { get; set; }
        public int JabatanId { get; set; }
        public StatusPegawai StatusPegawai { get; set; }
        public int StatusPegawaiId { get; set; }
        public bool IsMiniSidebar { get; set; }
        public string ColorNavbar { get; set; }
        public bool IsLocked { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        #region Master
        public DbSet<Bank> Bank { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<Divisi> Divisi { get; set; }
        public DbSet<Jabatan> Jabatan { get; set; }
        public DbSet<JenisAttch> JenisAttch { get; set; }
        public DbSet<JenisDokumen> JenisDokumen { get; set; }
        public DbSet<JenisPotongan> JenisPotongan { get; set; }
        public DbSet<JenisRekening> JenisRekening { get; set; }
        public DbSet<JenisSlip> JenisSlip { get; set; }
        public DbSet<Kelompok> Kelompok { get; set; }
        public DbSet<KodeSurat> KodeSurat { get; set; }
        public DbSet<Nominal> Nominal { get; set; }
        public DbSet<NoRekCabang> NoRekCabang { get; set; }
        public DbSet<NoRekCurrency> NoRekCurrency { get; set; }
        public DbSet<NoRekMCOA> NoRekMCOA { get; set; }
        public DbSet<OutputAttch> OutputAttch { get; set; }
        public DbSet<OutputSlip> OutputSlip { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<StatusPegawai> StatusPegawai { get; set; }
        public DbSet<SubJenisAttch> SubJenisAttch { get; set; }
        public DbSet<SubJenisPotongan> SubJenisPotongan { get; set; }
        public DbSet<Unit> Unit { get; set; }
        public DbSet<Vendor> Vendor { get; set; }
        public DbSet<Wilayah> Wilayah { get; set; }
        #endregion

        #region Setting
        public DbSet<MappingDetailRole> MappingDetailRole { get; set; }
        public DbSet<MappingRole> MappingRole { get; set; }
        public DbSet<SettingRole> SettingRole { get; set; }
        public DbSet<SettingSlip> SettingSlip { get; set; }
        #endregion

        #region Transaksi
        public DbSet<Main> Main { get; set; }
        public DbSet<MainDetail> MainDetail { get; set; }
        public DbSet<Trans> Trans { get; set; }
        public DbSet<TransAttachment> TransAttachment { get; set; }
        public DbSet<TransMainDetail> TransMainDetail { get; set; }
        public DbSet<TransPengadaan> TransPengadaan { get; set; }
        public DbSet<TransPotongan> TransPotongan { get; set; }
        public DbSet<TransRekening> TransRekening { get; set; }
        public DbSet<TransSlip> TransSlip { get; set; }
        public DbSet<TransTracking> TransTracking { get; set; }
        #endregion
        public DbSet<LogUser> LogUser { get; set; }
        public ApplicationDbContext()
            : base("Apv", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MainDetail>().Property(x => x.TotalNominal).HasPrecision(21, 5);
            modelBuilder.Entity<Trans>().Property(x => x.TotalNominal).HasPrecision(21, 5);
            modelBuilder.Entity<TransRekening>().Property(x => x.Nominal).HasPrecision(21, 5);
            modelBuilder.Entity<TransPengadaan>().Property(x => x.Nominal).HasPrecision(21, 5);
            modelBuilder.Entity<TransPotongan>().Property(x => x.Nominal).HasPrecision(21, 5);
            modelBuilder.Entity<TransPotongan>().Property(x => x.Total).HasPrecision(21, 5);
            modelBuilder.Entity<TransSlip>().Property(x => x.NominalDebit).HasPrecision(21, 5);
            modelBuilder.Entity<TransSlip>().Property(x => x.NominalKredit).HasPrecision(21, 5);

            base.OnModelCreating(modelBuilder);
        }
    }
}