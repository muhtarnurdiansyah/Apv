namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addIsActiveMaindetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MainDetails", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MainDetails", "IsActive");
        }
    }
}
