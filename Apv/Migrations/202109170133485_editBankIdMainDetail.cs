namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editBankIdMainDetail : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MainDetails", "BankId", "dbo.Banks");
            DropIndex("dbo.MainDetails", new[] { "BankId" });
            AlterColumn("dbo.MainDetails", "BankId", c => c.Int(nullable: false));
            CreateIndex("dbo.MainDetails", "BankId");
            AddForeignKey("dbo.MainDetails", "BankId", "dbo.Banks", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MainDetails", "BankId", "dbo.Banks");
            DropIndex("dbo.MainDetails", new[] { "BankId" });
            AlterColumn("dbo.MainDetails", "BankId", c => c.Int());
            CreateIndex("dbo.MainDetails", "BankId");
            AddForeignKey("dbo.MainDetails", "BankId", "dbo.Banks", "Id");
        }
    }
}
