namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNoRekening : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NoRekenings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        No = c.String(),
                        Nama = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.NoRekenings");
        }
    }
}
