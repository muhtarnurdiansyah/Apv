namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSlipBatch : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TransBatches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransId = c.Int(nullable: false),
                        ÍsUrgent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Trans", t => t.TransId, cascadeDelete: true)
                .Index(t => t.TransId);
            
            CreateTable(
                "dbo.TransSlips",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Tanggal = c.DateTime(nullable: false),
                        NoReferensi = c.String(),
                        NamaRekDebit = c.String(),
                        NamaRekDebit2 = c.String(),
                        NoRekDebit = c.String(),
                        NoRekDebit2 = c.String(),
                        IsNoRekDebitVA = c.Boolean(nullable: false),
                        NamaCabangDebit = c.String(),
                        JenisRekeningDebitId = c.Int(),
                        PesanDebit = c.String(),
                        PesanDebit2 = c.String(),
                        CurrencyDebitId = c.Int(),
                        NominalDebit = c.Decimal(nullable: false, precision: 21, scale: 5),
                        NamaRekKredit = c.String(),
                        NoRekKredit = c.String(),
                        NoRekKredit2 = c.String(),
                        IsNoRekKreditVA = c.Boolean(nullable: false),
                        NamaCabangKredit = c.String(),
                        JenisRekeningKreditId = c.Int(),
                        BankKreditId = c.Int(),
                        CurrencyKreditId = c.Int(),
                        NominalKredit = c.Decimal(nullable: false, precision: 21, scale: 5),
                        AddKredit = c.String(),
                        AddKredit2 = c.String(),
                        PhoneKredit = c.String(),
                        CityCodeKredit = c.String(),
                        IdKredit = c.String(),
                        IdTypeKredit = c.String(),
                        SandiTXN = c.String(),
                        NoJurnal = c.String(),
                        Keterangan1 = c.String(),
                        Keterangan2 = c.String(),
                        Keterangan3 = c.String(),
                        Biaya = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Kurs = c.Decimal(nullable: false, precision: 18, scale: 2),
                        JenisSlipId = c.Int(),
                        OutputSlipId = c.Int(),
                        TransBatchId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Banks", t => t.BankKreditId)
                .ForeignKey("dbo.Currencies", t => t.CurrencyDebitId)
                .ForeignKey("dbo.Currencies", t => t.CurrencyKreditId)
                .ForeignKey("dbo.JenisRekenings", t => t.JenisRekeningDebitId)
                .ForeignKey("dbo.JenisRekenings", t => t.JenisRekeningKreditId)
                .ForeignKey("dbo.JenisSlips", t => t.JenisSlipId)
                .ForeignKey("dbo.OutputSlips", t => t.OutputSlipId)
                .ForeignKey("dbo.TransBatches", t => t.TransBatchId, cascadeDelete: true)
                .Index(t => t.JenisRekeningDebitId)
                .Index(t => t.CurrencyDebitId)
                .Index(t => t.JenisRekeningKreditId)
                .Index(t => t.BankKreditId)
                .Index(t => t.CurrencyKreditId)
                .Index(t => t.JenisSlipId)
                .Index(t => t.OutputSlipId)
                .Index(t => t.TransBatchId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TransSlips", "TransBatchId", "dbo.TransBatches");
            DropForeignKey("dbo.TransSlips", "OutputSlipId", "dbo.OutputSlips");
            DropForeignKey("dbo.TransSlips", "JenisSlipId", "dbo.JenisSlips");
            DropForeignKey("dbo.TransSlips", "JenisRekeningKreditId", "dbo.JenisRekenings");
            DropForeignKey("dbo.TransSlips", "JenisRekeningDebitId", "dbo.JenisRekenings");
            DropForeignKey("dbo.TransSlips", "CurrencyKreditId", "dbo.Currencies");
            DropForeignKey("dbo.TransSlips", "CurrencyDebitId", "dbo.Currencies");
            DropForeignKey("dbo.TransSlips", "BankKreditId", "dbo.Banks");
            DropForeignKey("dbo.TransBatches", "TransId", "dbo.Trans");
            DropIndex("dbo.TransSlips", new[] { "TransBatchId" });
            DropIndex("dbo.TransSlips", new[] { "OutputSlipId" });
            DropIndex("dbo.TransSlips", new[] { "JenisSlipId" });
            DropIndex("dbo.TransSlips", new[] { "CurrencyKreditId" });
            DropIndex("dbo.TransSlips", new[] { "BankKreditId" });
            DropIndex("dbo.TransSlips", new[] { "JenisRekeningKreditId" });
            DropIndex("dbo.TransSlips", new[] { "CurrencyDebitId" });
            DropIndex("dbo.TransSlips", new[] { "JenisRekeningDebitId" });
            DropIndex("dbo.TransBatches", new[] { "TransId" });
            DropTable("dbo.TransSlips");
            DropTable("dbo.TransBatches");
        }
    }
}
