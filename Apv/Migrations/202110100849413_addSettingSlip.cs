namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addSettingSlip : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SettingSlips",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Tanggal = c.Boolean(nullable: false),
                        NoReferensi = c.Boolean(nullable: false),
                        NamaRekDebit = c.Boolean(nullable: false),
                        NamaRekDebit2 = c.Boolean(nullable: false),
                        NoRekDebit = c.Boolean(nullable: false),
                        NoRekDebit2 = c.Boolean(nullable: false),
                        NoRekDebitText = c.Boolean(nullable: false),
                        NamaCabangDebit = c.Boolean(nullable: false),
                        JenisRekeningDebit = c.Boolean(nullable: false),
                        PesanDebit = c.Boolean(nullable: false),
                        PesanDebit2 = c.Boolean(nullable: false),
                        CurrencyDebit = c.Boolean(nullable: false),
                        NominalDebit = c.Boolean(nullable: false),
                        NamaRekKredit = c.Boolean(nullable: false),
                        NoRekKredit = c.Boolean(nullable: false),
                        NoRekKredit2 = c.Boolean(nullable: false),
                        NoRekKreditText = c.Boolean(nullable: false),
                        NamaCabangKredit = c.Boolean(nullable: false),
                        JenisRekeningKredit = c.Boolean(nullable: false),
                        BankKredit = c.Boolean(nullable: false),
                        CurrencyKredit = c.Boolean(nullable: false),
                        NominalKredit = c.Boolean(nullable: false),
                        AddKredit = c.Boolean(nullable: false),
                        AddKredit2 = c.Boolean(nullable: false),
                        PhoneKredit = c.Boolean(nullable: false),
                        CityCodeKredit = c.Boolean(nullable: false),
                        IdKredit = c.Boolean(nullable: false),
                        IdTypeKredit = c.Boolean(nullable: false),
                        SandiTXN = c.Boolean(nullable: false),
                        Keterangan1 = c.Boolean(nullable: false),
                        Keterangan2 = c.Boolean(nullable: false),
                        Keterangan3 = c.Boolean(nullable: false),
                        Biaya = c.Boolean(nullable: false),
                        Kurs = c.Boolean(nullable: false),
                        JenisSlipId = c.Int(nullable: false),
                        OutputSlipId = c.Int(nullable: false),
                        KelompokId = c.Int(nullable: false),
                        CreaterId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreaterId)
                .ForeignKey("dbo.JenisSlips", t => t.JenisSlipId, cascadeDelete: true)
                .ForeignKey("dbo.Kelompoks", t => t.KelompokId, cascadeDelete: true)
                .ForeignKey("dbo.OutputSlips", t => t.OutputSlipId, cascadeDelete: true)
                .Index(t => t.JenisSlipId)
                .Index(t => t.OutputSlipId)
                .Index(t => t.KelompokId)
                .Index(t => t.CreaterId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SettingSlips", "OutputSlipId", "dbo.OutputSlips");
            DropForeignKey("dbo.SettingSlips", "KelompokId", "dbo.Kelompoks");
            DropForeignKey("dbo.SettingSlips", "JenisSlipId", "dbo.JenisSlips");
            DropForeignKey("dbo.SettingSlips", "CreaterId", "dbo.AspNetUsers");
            DropIndex("dbo.SettingSlips", new[] { "CreaterId" });
            DropIndex("dbo.SettingSlips", new[] { "KelompokId" });
            DropIndex("dbo.SettingSlips", new[] { "OutputSlipId" });
            DropIndex("dbo.SettingSlips", new[] { "JenisSlipId" });
            DropTable("dbo.SettingSlips");
        }
    }
}
