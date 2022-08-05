namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addNoinTrans : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Trans", "NomorReg", c => c.String());
            AddColumn("dbo.Trans", "NomorCN", c => c.String());
            AddColumn("dbo.Trans", "NomorCNPPN", c => c.String());
            AddColumn("dbo.Trans", "NomorPP", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Trans", "NomorPP");
            DropColumn("dbo.Trans", "NomorCNPPN");
            DropColumn("dbo.Trans", "NomorCN");
            DropColumn("dbo.Trans", "NomorReg");
        }
    }
}
