namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addRekMCOAccyCab : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.NoRekenings", newName: "NoRekCabangs");
            DropForeignKey("dbo.TransAttachments", "JenisAttchId", "dbo.JenisAttches");
            DropIndex("dbo.TransAttachments", new[] { "JenisAttchId" });
            CreateTable(
                "dbo.NoRekCurrencies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        No = c.String(),
                        Nama = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NoRekMCOAs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        No = c.String(),
                        Nama = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SubJenisAttches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                        JenisAttchId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.JenisAttches", t => t.JenisAttchId, cascadeDelete: true)
                .Index(t => t.JenisAttchId);
            
            AddColumn("dbo.TransAttachments", "SubJenisAttchId", c => c.Int(nullable: false));
            CreateIndex("dbo.TransAttachments", "SubJenisAttchId");
            AddForeignKey("dbo.TransAttachments", "SubJenisAttchId", "dbo.SubJenisAttches", "Id", cascadeDelete: true);
            DropColumn("dbo.TransAttachments", "JenisAttchId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransAttachments", "JenisAttchId", c => c.Int(nullable: false));
            DropForeignKey("dbo.TransAttachments", "SubJenisAttchId", "dbo.SubJenisAttches");
            DropForeignKey("dbo.SubJenisAttches", "JenisAttchId", "dbo.JenisAttches");
            DropIndex("dbo.TransAttachments", new[] { "SubJenisAttchId" });
            DropIndex("dbo.SubJenisAttches", new[] { "JenisAttchId" });
            DropColumn("dbo.TransAttachments", "SubJenisAttchId");
            DropTable("dbo.SubJenisAttches");
            DropTable("dbo.NoRekMCOAs");
            DropTable("dbo.NoRekCurrencies");
            CreateIndex("dbo.TransAttachments", "JenisAttchId");
            AddForeignKey("dbo.TransAttachments", "JenisAttchId", "dbo.JenisAttches", "Id", cascadeDelete: true);
            RenameTable(name: "dbo.NoRekCabangs", newName: "NoRekenings");
        }
    }
}
