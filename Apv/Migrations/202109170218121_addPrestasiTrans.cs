namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPrestasiTrans : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Trans", "Prestasi", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Trans", "Prestasi");
        }
    }
}
