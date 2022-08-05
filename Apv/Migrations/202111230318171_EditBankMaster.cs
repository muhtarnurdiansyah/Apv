namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EditBankMaster : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Banks", "Singkatan", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Banks", "Singkatan");
        }
    }
}
