namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeTransBatch : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TransBatches", "TransId", "dbo.Trans");
            DropForeignKey("dbo.TransSlips", "TransBatchId", "dbo.TransBatches");
            DropIndex("dbo.TransBatches", new[] { "TransId" });
            DropIndex("dbo.TransSlips", new[] { "TransBatchId" });
            AddColumn("dbo.TransSlips", "TransId", c => c.Int(nullable: false));
            CreateIndex("dbo.TransSlips", "TransId");
            AddForeignKey("dbo.TransSlips", "TransId", "dbo.Trans", "Id", cascadeDelete: true);
            DropColumn("dbo.TransSlips", "TransBatchId");
            DropTable("dbo.TransBatches");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TransBatches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransId = c.Int(nullable: false),
                        ÍsUrgent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.TransSlips", "TransBatchId", c => c.Int(nullable: false));
            DropForeignKey("dbo.TransSlips", "TransId", "dbo.Trans");
            DropIndex("dbo.TransSlips", new[] { "TransId" });
            DropColumn("dbo.TransSlips", "TransId");
            CreateIndex("dbo.TransSlips", "TransBatchId");
            CreateIndex("dbo.TransBatches", "TransId");
            AddForeignKey("dbo.TransSlips", "TransBatchId", "dbo.TransBatches", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TransBatches", "TransId", "dbo.Trans", "Id", cascadeDelete: true);
        }
    }
}
