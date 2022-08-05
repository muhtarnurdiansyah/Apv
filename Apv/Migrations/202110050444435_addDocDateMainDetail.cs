namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDocDateMainDetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MainDetails", "DocDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MainDetails", "DocDate");
        }
    }
}
