namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editSubJenisPotongan : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubJenisPotongans", "Nama2", c => c.String());
            AddColumn("dbo.SubJenisPotongans", "Norek", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SubJenisPotongans", "Norek");
            DropColumn("dbo.SubJenisPotongans", "Nama2");
        }
    }
}
