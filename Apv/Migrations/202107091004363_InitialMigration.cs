namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Banks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                        KodeBIC = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(),
                        DeleteDate = c.DateTime(),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Currencies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(),
                        DeleteDate = c.DateTime(),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Jabatans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Kelompoks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Singkatan = c.String(),
                        Nama = c.String(),
                        WilayahId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Wilayahs", t => t.WilayahId, cascadeDelete: true)
                .Index(t => t.WilayahId);
            
            CreateTable(
                "dbo.Wilayahs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                        Singkatan = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LogUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        SessionID = c.String(),
                        IPAddress = c.String(),
                        IsLogin = c.Boolean(nullable: false),
                        LastLogin = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Nama = c.String(nullable: false),
                        NPP = c.String(nullable: false),
                        UnitId = c.Int(nullable: false),
                        JabatanId = c.Int(nullable: false),
                        StatusPegawaiId = c.Int(nullable: false),
                        IsMiniSidebar = c.Boolean(nullable: false),
                        ColorNavbar = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Jabatans", t => t.JabatanId, cascadeDelete: true)
                .ForeignKey("dbo.StatusPegawais", t => t.StatusPegawaiId, cascadeDelete: true)
                .ForeignKey("dbo.Units", t => t.UnitId, cascadeDelete: true)
                .Index(t => t.UnitId)
                .Index(t => t.JabatanId)
                .Index(t => t.StatusPegawaiId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.StatusPegawais",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Units",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                        KelompokId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Kelompoks", t => t.KelompokId, cascadeDelete: true)
                .Index(t => t.KelompokId);
            
            CreateTable(
                "dbo.Nominals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        KelompokId = c.Int(nullable: false),
                        JabatanId = c.Int(nullable: false),
                        Min = c.Long(nullable: false),
                        Max = c.Long(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(),
                        DeleteDate = c.DateTime(),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Jabatans", t => t.JabatanId, cascadeDelete: true)
                .ForeignKey("dbo.Kelompoks", t => t.KelompokId, cascadeDelete: true)
                .Index(t => t.KelompokId)
                .Index(t => t.JabatanId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.Status",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                        Warna = c.String(),
                        Warna2 = c.String(),
                        Fill = c.String(),
                        Border = c.String(),
                        HighLight = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Nominals", "KelompokId", "dbo.Kelompoks");
            DropForeignKey("dbo.Nominals", "JabatanId", "dbo.Jabatans");
            DropForeignKey("dbo.LogUsers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "UnitId", "dbo.Units");
            DropForeignKey("dbo.Units", "KelompokId", "dbo.Kelompoks");
            DropForeignKey("dbo.AspNetUsers", "StatusPegawaiId", "dbo.StatusPegawais");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "JabatanId", "dbo.Jabatans");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Kelompoks", "WilayahId", "dbo.Wilayahs");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Nominals", new[] { "JabatanId" });
            DropIndex("dbo.Nominals", new[] { "KelompokId" });
            DropIndex("dbo.Units", new[] { "KelompokId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "StatusPegawaiId" });
            DropIndex("dbo.AspNetUsers", new[] { "JabatanId" });
            DropIndex("dbo.AspNetUsers", new[] { "UnitId" });
            DropIndex("dbo.LogUsers", new[] { "UserId" });
            DropIndex("dbo.Kelompoks", new[] { "WilayahId" });
            DropTable("dbo.Status");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Nominals");
            DropTable("dbo.Units");
            DropTable("dbo.StatusPegawais");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.LogUsers");
            DropTable("dbo.Wilayahs");
            DropTable("dbo.Kelompoks");
            DropTable("dbo.Jabatans");
            DropTable("dbo.Currencies");
            DropTable("dbo.Banks");
        }
    }
}
