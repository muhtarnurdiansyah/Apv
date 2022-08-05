namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editNilaidanNominalTrans : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubJenisPotongans", "Nilai", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Trans", "TotalNominal", c => c.Decimal(nullable: false, precision: 21, scale: 5));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Trans", "TotalNominal");
            DropColumn("dbo.SubJenisPotongans", "Nilai");
        }
    }
}
