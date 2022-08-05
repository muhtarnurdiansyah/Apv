namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editNamaMain : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Mains", "Uraian", c => c.String());
            DropColumn("dbo.Mains", "Nama");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Mains", "Nama", c => c.String());
            DropColumn("dbo.Mains", "Uraian");
        }
    }
}
