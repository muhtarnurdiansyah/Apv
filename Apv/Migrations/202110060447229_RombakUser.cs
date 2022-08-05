namespace Apv.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RombakUser : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MappingDetailRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MappingRoleId = c.Int(nullable: false),
                        RoleId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MappingRoles", t => t.MappingRoleId, cascadeDelete: true)
                .Index(t => t.MappingRoleId);
            
            CreateTable(
                "dbo.MappingRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UnitId = c.Int(nullable: false),
                        JabatanId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Jabatans", t => t.JabatanId, cascadeDelete: true)
                .ForeignKey("dbo.Units", t => t.UnitId, cascadeDelete: true)
                .Index(t => t.UnitId)
                .Index(t => t.JabatanId);
            
            CreateTable(
                "dbo.SettingRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        IsDefault = c.Boolean(nullable: false),
                        MappingRoleId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        CreaterId = c.String(maxLength: 128),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(),
                        DeleteDate = c.DateTime(),
                        IsDelete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreaterId)
                .ForeignKey("dbo.MappingRoles", t => t.MappingRoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.MappingRoleId)
                .Index(t => t.UserId)
                .Index(t => t.CreaterId);
            
            AddColumn("dbo.AspNetUsers", "IsLocked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SettingRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.SettingRoles", "MappingRoleId", "dbo.MappingRoles");
            DropForeignKey("dbo.SettingRoles", "CreaterId", "dbo.AspNetUsers");
            DropForeignKey("dbo.MappingDetailRoles", "MappingRoleId", "dbo.MappingRoles");
            DropForeignKey("dbo.MappingRoles", "UnitId", "dbo.Units");
            DropForeignKey("dbo.MappingRoles", "JabatanId", "dbo.Jabatans");
            DropIndex("dbo.SettingRoles", new[] { "CreaterId" });
            DropIndex("dbo.SettingRoles", new[] { "UserId" });
            DropIndex("dbo.SettingRoles", new[] { "MappingRoleId" });
            DropIndex("dbo.MappingRoles", new[] { "JabatanId" });
            DropIndex("dbo.MappingRoles", new[] { "UnitId" });
            DropIndex("dbo.MappingDetailRoles", new[] { "MappingRoleId" });
            DropColumn("dbo.AspNetUsers", "IsLocked");
            DropTable("dbo.SettingRoles");
            DropTable("dbo.MappingRoles");
            DropTable("dbo.MappingDetailRoles");
        }
    }
}
