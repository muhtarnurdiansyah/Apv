namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editTransPotongan : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransPotongans", "IsDone", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransPotongans", "IsDone");
        }
    }
}
