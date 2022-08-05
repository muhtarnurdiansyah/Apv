namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editTermin : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.MainDetails", "TotalTermin", c => c.String());
            AlterColumn("dbo.Trans", "Termin", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Trans", "Termin", c => c.Int(nullable: false));
            AlterColumn("dbo.MainDetails", "TotalTermin", c => c.Int(nullable: false));
        }
    }
}
