namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PerubahanNoRekAll : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MainDetails", "NoRek", c => c.String());
            AddColumn("dbo.MainDetails", "NamaRek", c => c.String());
            AddColumn("dbo.MainDetails", "BankId", c => c.Int());
            AddColumn("dbo.MainDetails", "Cabang", c => c.String());
            AddColumn("dbo.SubJenisPotongans", "NoRek2", c => c.String());
            AddColumn("dbo.TransRekenings", "Cabang", c => c.String());
            CreateIndex("dbo.MainDetails", "BankId");
            AddForeignKey("dbo.MainDetails", "BankId", "dbo.Banks", "Id");
            DropColumn("dbo.TransRekenings", "Lokasi");
            DropColumn("dbo.TransRekenings", "Keterangan1");
            DropColumn("dbo.TransRekenings", "Keterangan2");
            DropColumn("dbo.TransRekenings", "Keterangan3");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransRekenings", "Keterangan3", c => c.String());
            AddColumn("dbo.TransRekenings", "Keterangan2", c => c.String());
            AddColumn("dbo.TransRekenings", "Keterangan1", c => c.String());
            AddColumn("dbo.TransRekenings", "Lokasi", c => c.String());
            DropForeignKey("dbo.MainDetails", "BankId", "dbo.Banks");
            DropIndex("dbo.MainDetails", new[] { "BankId" });
            DropColumn("dbo.TransRekenings", "Cabang");
            DropColumn("dbo.SubJenisPotongans", "NoRek2");
            DropColumn("dbo.MainDetails", "Cabang");
            DropColumn("dbo.MainDetails", "BankId");
            DropColumn("dbo.MainDetails", "NamaRek");
            DropColumn("dbo.MainDetails", "NoRek");
        }
    }
}
