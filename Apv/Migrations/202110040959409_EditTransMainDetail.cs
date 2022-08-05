namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EditTransMainDetail : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Trans", "MainDetailId", "dbo.MainDetails");
            DropIndex("dbo.Trans", new[] { "MainDetailId" });
            CreateTable(
                "dbo.TransMainDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TotalNominal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MainDetailId = c.Int(nullable: false),
                        TransId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MainDetails", t => t.MainDetailId, cascadeDelete: true)
                .ForeignKey("dbo.Trans", t => t.TransId, cascadeDelete: true)
                .Index(t => t.MainDetailId)
                .Index(t => t.TransId);
            
            DropColumn("dbo.Trans", "MainDetailId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Trans", "MainDetailId", c => c.Int(nullable: false));
            DropForeignKey("dbo.TransMainDetails", "TransId", "dbo.Trans");
            DropForeignKey("dbo.TransMainDetails", "MainDetailId", "dbo.MainDetails");
            DropIndex("dbo.TransMainDetails", new[] { "TransId" });
            DropIndex("dbo.TransMainDetails", new[] { "MainDetailId" });
            DropTable("dbo.TransMainDetails");
            CreateIndex("dbo.Trans", "MainDetailId");
            AddForeignKey("dbo.Trans", "MainDetailId", "dbo.MainDetails", "Id", cascadeDelete: true);
        }
    }
}
