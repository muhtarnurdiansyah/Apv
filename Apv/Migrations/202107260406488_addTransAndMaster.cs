namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTransAndMaster : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Divisis",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                        Singkatan = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JenisAttches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JenisPotongans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Mains",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        VendorId = c.Int(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(),
                        DeleteDate = c.DateTime(),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Vendors", t => t.VendorId, cascadeDelete: true)
                .Index(t => t.VendorId);
            
            CreateTable(
                "dbo.Vendors",
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
                "dbo.MainDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nomor = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        TotalNominal = c.Decimal(nullable: false, precision: 21, scale: 5),
                        TotalTermin = c.Int(nullable: false),
                        IsKontrak = c.Boolean(nullable: false),
                        Index = c.Int(nullable: false),
                        Path = c.String(),
                        MainId = c.Int(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(),
                        DeleteDate = c.DateTime(),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Mains", t => t.MainId, cascadeDelete: true)
                .Index(t => t.MainId);
            
            CreateTable(
                "dbo.SubJenisPotongans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                        JenisPotonganId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.JenisPotongans", t => t.JenisPotonganId, cascadeDelete: true)
                .Index(t => t.JenisPotonganId);
            
            CreateTable(
                "dbo.Trans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nomor = c.String(),
                        Uraian = c.String(),
                        DocDate = c.DateTime(nullable: false),
                        Termin = c.Int(nullable: false),
                        MainDetailId = c.Int(nullable: false),
                        StatusId = c.Int(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(),
                        DeleteDate = c.DateTime(),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MainDetails", t => t.MainDetailId, cascadeDelete: true)
                .ForeignKey("dbo.Status", t => t.StatusId, cascadeDelete: true)
                .Index(t => t.MainDetailId)
                .Index(t => t.StatusId);
            
            CreateTable(
                "dbo.TransAttachments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nomor = c.String(),
                        Nama = c.String(),
                        Jumlah = c.Int(nullable: false),
                        Path = c.String(),
                        DocDate = c.DateTime(nullable: false),
                        IsInclude = c.Boolean(nullable: false),
                        TransId = c.Int(nullable: false),
                        JenisAttchId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.JenisAttches", t => t.JenisAttchId, cascadeDelete: true)
                .ForeignKey("dbo.Trans", t => t.TransId, cascadeDelete: true)
                .Index(t => t.TransId)
                .Index(t => t.JenisAttchId);
            
            CreateTable(
                "dbo.TransPengadaans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                        Nominal = c.Decimal(nullable: false, precision: 21, scale: 5),
                        TransId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Trans", t => t.TransId, cascadeDelete: true)
                .Index(t => t.TransId);
            
            CreateTable(
                "dbo.TransPotongans",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nominal = c.Decimal(nullable: false, precision: 21, scale: 5),
                        Total = c.Decimal(nullable: false, precision: 21, scale: 5),
                        TransId = c.Int(nullable: false),
                        SubJenisPotonganId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SubJenisPotongans", t => t.SubJenisPotonganId, cascadeDelete: true)
                .ForeignKey("dbo.Trans", t => t.TransId, cascadeDelete: true)
                .Index(t => t.TransId)
                .Index(t => t.SubJenisPotonganId);
            
            CreateTable(
                "dbo.TransRekenings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NoRek = c.String(),
                        NoRek2 = c.String(),
                        Nama = c.String(),
                        BankId = c.Int(),
                        CurrencyId = c.Int(nullable: false),
                        Nominal = c.Decimal(nullable: false, precision: 21, scale: 5),
                        Lokasi = c.String(),
                        Keterangan1 = c.String(),
                        Keterangan2 = c.String(),
                        Keterangan3 = c.String(),
                        IsMain = c.Boolean(nullable: false),
                        IsDebit = c.Boolean(nullable: false),
                        TransId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Banks", t => t.BankId)
                .ForeignKey("dbo.Currencies", t => t.CurrencyId, cascadeDelete: true)
                .ForeignKey("dbo.Trans", t => t.TransId, cascadeDelete: true)
                .Index(t => t.BankId)
                .Index(t => t.CurrencyId)
                .Index(t => t.TransId);
            
            CreateTable(
                "dbo.TransTrackings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransId = c.Int(nullable: false),
                        ReceiveDate = c.DateTime(nullable: false),
                        ReceiverId = c.String(maxLength: 128),
                        ReceiverActivity = c.String(),
                        ReceiverIcon = c.String(),
                        ReceiverColorIcon = c.String(),
                        SendDate = c.DateTime(),
                        SenderId = c.String(maxLength: 128),
                        SenderKeterangan = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ReceiverId)
                .ForeignKey("dbo.AspNetUsers", t => t.SenderId)
                .ForeignKey("dbo.Trans", t => t.TransId, cascadeDelete: true)
                .Index(t => t.TransId)
                .Index(t => t.ReceiverId)
                .Index(t => t.SenderId);
            
            AddColumn("dbo.Wilayahs", "DivisiId", c => c.Int(nullable: false));
            CreateIndex("dbo.Wilayahs", "DivisiId");
            AddForeignKey("dbo.Wilayahs", "DivisiId", "dbo.Divisis", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TransTrackings", "TransId", "dbo.Trans");
            DropForeignKey("dbo.TransTrackings", "SenderId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TransTrackings", "ReceiverId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TransRekenings", "TransId", "dbo.Trans");
            DropForeignKey("dbo.TransRekenings", "CurrencyId", "dbo.Currencies");
            DropForeignKey("dbo.TransRekenings", "BankId", "dbo.Banks");
            DropForeignKey("dbo.TransPotongans", "TransId", "dbo.Trans");
            DropForeignKey("dbo.TransPotongans", "SubJenisPotonganId", "dbo.SubJenisPotongans");
            DropForeignKey("dbo.TransPengadaans", "TransId", "dbo.Trans");
            DropForeignKey("dbo.TransAttachments", "TransId", "dbo.Trans");
            DropForeignKey("dbo.TransAttachments", "JenisAttchId", "dbo.JenisAttches");
            DropForeignKey("dbo.Trans", "StatusId", "dbo.Status");
            DropForeignKey("dbo.Trans", "MainDetailId", "dbo.MainDetails");
            DropForeignKey("dbo.SubJenisPotongans", "JenisPotonganId", "dbo.JenisPotongans");
            DropForeignKey("dbo.MainDetails", "MainId", "dbo.Mains");
            DropForeignKey("dbo.Mains", "VendorId", "dbo.Vendors");
            DropForeignKey("dbo.Wilayahs", "DivisiId", "dbo.Divisis");
            DropIndex("dbo.TransTrackings", new[] { "SenderId" });
            DropIndex("dbo.TransTrackings", new[] { "ReceiverId" });
            DropIndex("dbo.TransTrackings", new[] { "TransId" });
            DropIndex("dbo.TransRekenings", new[] { "TransId" });
            DropIndex("dbo.TransRekenings", new[] { "CurrencyId" });
            DropIndex("dbo.TransRekenings", new[] { "BankId" });
            DropIndex("dbo.TransPotongans", new[] { "SubJenisPotonganId" });
            DropIndex("dbo.TransPotongans", new[] { "TransId" });
            DropIndex("dbo.TransPengadaans", new[] { "TransId" });
            DropIndex("dbo.TransAttachments", new[] { "JenisAttchId" });
            DropIndex("dbo.TransAttachments", new[] { "TransId" });
            DropIndex("dbo.Trans", new[] { "StatusId" });
            DropIndex("dbo.Trans", new[] { "MainDetailId" });
            DropIndex("dbo.SubJenisPotongans", new[] { "JenisPotonganId" });
            DropIndex("dbo.MainDetails", new[] { "MainId" });
            DropIndex("dbo.Mains", new[] { "VendorId" });
            DropIndex("dbo.Wilayahs", new[] { "DivisiId" });
            DropColumn("dbo.Wilayahs", "DivisiId");
            DropTable("dbo.TransTrackings");
            DropTable("dbo.TransRekenings");
            DropTable("dbo.TransPotongans");
            DropTable("dbo.TransPengadaans");
            DropTable("dbo.TransAttachments");
            DropTable("dbo.Trans");
            DropTable("dbo.SubJenisPotongans");
            DropTable("dbo.MainDetails");
            DropTable("dbo.Vendors");
            DropTable("dbo.Mains");
            DropTable("dbo.JenisPotongans");
            DropTable("dbo.JenisAttches");
            DropTable("dbo.Divisis");
        }
    }
}
