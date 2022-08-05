namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addNPWPAlamatMaindetail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MainDetails", "NPWP", c => c.String());
            AddColumn("dbo.MainDetails", "Alamat", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MainDetails", "Alamat");
            DropColumn("dbo.MainDetails", "NPWP");
        }
    }
}
