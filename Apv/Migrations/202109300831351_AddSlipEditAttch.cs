namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSlipEditAttch : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JenisRekenings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JenisSlips",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Singkatan = c.String(),
                        Nama = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OutputSlips",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nama = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.TransAttachments", "IsInclude");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TransAttachments", "IsInclude", c => c.Boolean(nullable: false));
            DropTable("dbo.OutputSlips");
            DropTable("dbo.JenisSlips");
            DropTable("dbo.JenisRekenings");
        }
    }
}
