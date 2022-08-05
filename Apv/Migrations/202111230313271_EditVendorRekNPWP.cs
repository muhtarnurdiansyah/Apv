namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EditVendorRekNPWP : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Vendors", "NoRek", c => c.String());
            AddColumn("dbo.Vendors", "NamaRek", c => c.String());
            AddColumn("dbo.Vendors", "BankId", c => c.Int());
            AddColumn("dbo.Vendors", "Cabang", c => c.String());
            AddColumn("dbo.Vendors", "NPWP", c => c.String());
            AddColumn("dbo.Vendors", "Alamat", c => c.String());
            CreateIndex("dbo.Vendors", "BankId");
            AddForeignKey("dbo.Vendors", "BankId", "dbo.Banks", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Vendors", "BankId", "dbo.Banks");
            DropIndex("dbo.Vendors", new[] { "BankId" });
            DropColumn("dbo.Vendors", "Alamat");
            DropColumn("dbo.Vendors", "NPWP");
            DropColumn("dbo.Vendors", "Cabang");
            DropColumn("dbo.Vendors", "BankId");
            DropColumn("dbo.Vendors", "NamaRek");
            DropColumn("dbo.Vendors", "NoRek");
        }
    }
}
