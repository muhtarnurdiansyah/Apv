namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editMainDetailTanggal : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.MainDetails", "StartDate", c => c.DateTime());
            AlterColumn("dbo.MainDetails", "EndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.MainDetails", "EndDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.MainDetails", "StartDate", c => c.DateTime(nullable: false));
        }
    }
}
