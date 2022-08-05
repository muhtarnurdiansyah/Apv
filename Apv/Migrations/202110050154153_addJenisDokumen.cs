namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addJenisDokumen : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JenisDokumen",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Singkatan = c.String(),
                        Nama = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.MainDetails", "JenisDokumenId", c => c.Int(nullable: false));
            CreateIndex("dbo.MainDetails", "JenisDokumenId");
            AddForeignKey("dbo.MainDetails", "JenisDokumenId", "dbo.JenisDokumen", "Id", cascadeDelete: true);
            DropColumn("dbo.MainDetails", "IsKontrak");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MainDetails", "IsKontrak", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.MainDetails", "JenisDokumenId", "dbo.JenisDokumen");
            DropIndex("dbo.MainDetails", new[] { "JenisDokumenId" });
            DropColumn("dbo.MainDetails", "JenisDokumenId");
            DropTable("dbo.JenisDokumen");
        }
    }
}
