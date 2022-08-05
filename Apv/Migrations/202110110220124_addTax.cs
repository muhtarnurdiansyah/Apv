namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTax : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Trans", "UploadTax", c => c.DateTime());
            AddColumn("dbo.Trans", "PathTax", c => c.String());
            AddColumn("dbo.Trans", "UploaderTaxId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Trans", "UploaderTaxId");
            AddForeignKey("dbo.Trans", "UploaderTaxId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Trans", "UploaderTaxId", "dbo.AspNetUsers");
            DropIndex("dbo.Trans", new[] { "UploaderTaxId" });
            DropColumn("dbo.Trans", "UploaderTaxId");
            DropColumn("dbo.Trans", "PathTax");
            DropColumn("dbo.Trans", "UploadTax");
        }
    }
}
