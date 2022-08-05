namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddKodeSuratOuputAttch : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.KodeSurats",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OutputAttches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Trans", "KodeSuratId", c => c.Int(nullable: false));
            AddColumn("dbo.TransAttachments", "OutputAttchId", c => c.Int(nullable: false));
            CreateIndex("dbo.Trans", "KodeSuratId");
            CreateIndex("dbo.TransAttachments", "OutputAttchId");
            AddForeignKey("dbo.Trans", "KodeSuratId", "dbo.KodeSurats", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TransAttachments", "OutputAttchId", "dbo.OutputAttches", "Id", cascadeDelete: true);
            DropColumn("dbo.TransAttachments", "Nama");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransAttachments", "Nama", c => c.String());
            DropForeignKey("dbo.TransAttachments", "OutputAttchId", "dbo.OutputAttches");
            DropForeignKey("dbo.Trans", "KodeSuratId", "dbo.KodeSurats");
            DropIndex("dbo.TransAttachments", new[] { "OutputAttchId" });
            DropIndex("dbo.Trans", new[] { "KodeSuratId" });
            DropColumn("dbo.TransAttachments", "OutputAttchId");
            DropColumn("dbo.Trans", "KodeSuratId");
            DropTable("dbo.OutputAttches");
            DropTable("dbo.KodeSurats");
        }
    }
}
