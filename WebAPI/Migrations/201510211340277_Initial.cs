namespace WebAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Devices",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.Long(nullable: false),
                        DeviceName = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Login = c.String(),
                        Email = c.String(),
                        Password = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Indiagrams",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.Long(nullable: false),
                        LastIndiagramInfoId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.IndiagramInfos", t => t.LastIndiagramInfoId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.LastIndiagramInfoId);
            
            CreateTable(
                "dbo.IndiagramInfos",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        IndiagramId = c.Long(nullable: false),
                        Version = c.Long(nullable: false),
                        ParentId = c.Long(nullable: false),
                        Position = c.Int(nullable: false),
                        Text = c.String(),
                        SoundPath = c.String(),
                        ImagePath = c.String(),
                        SoundHash = c.String(),
                        ImageHash = c.String(),
                        IsCategory = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Indiagrams", t => t.IndiagramId, cascadeDelete: true)
                .Index(t => t.IndiagramId);
            
            CreateTable(
                "dbo.IndiagramStates",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        IndiagramInfoId = c.Long(),
                        DeviceId = c.Long(nullable: false),
                        IsEnabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Devices", t => t.DeviceId, cascadeDelete: true)
                .ForeignKey("dbo.IndiagramInfos", t => t.IndiagramInfoId)
                .Index(t => t.IndiagramInfoId)
                .Index(t => t.DeviceId);
            
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        DeviceId = c.Long(nullable: false),
                        VersionNumber = c.Long(nullable: false),
                        Date = c.DateTime(nullable: false),
                        SerializedSettings = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Devices", t => t.DeviceId, cascadeDelete: true)
                .Index(t => t.DeviceId);
            
            CreateTable(
                "dbo.Versions",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.Long(nullable: false),
                        DeviceId = c.Long(nullable: false),
                        IsOpen = c.Boolean(nullable: false),
                        Number = c.Long(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Versions", "UserId", "dbo.Users");
            DropForeignKey("dbo.Settings", "DeviceId", "dbo.Devices");
            DropForeignKey("dbo.Indiagrams", "UserId", "dbo.Users");
            DropForeignKey("dbo.Indiagrams", "LastIndiagramInfoId", "dbo.IndiagramInfos");
            DropForeignKey("dbo.IndiagramInfos", "IndiagramId", "dbo.Indiagrams");
            DropForeignKey("dbo.IndiagramStates", "IndiagramInfoId", "dbo.IndiagramInfos");
            DropForeignKey("dbo.IndiagramStates", "DeviceId", "dbo.Devices");
            DropForeignKey("dbo.Devices", "UserId", "dbo.Users");
            DropIndex("dbo.Versions", new[] { "UserId" });
            DropIndex("dbo.Settings", new[] { "DeviceId" });
            DropIndex("dbo.IndiagramStates", new[] { "DeviceId" });
            DropIndex("dbo.IndiagramStates", new[] { "IndiagramInfoId" });
            DropIndex("dbo.IndiagramInfos", new[] { "IndiagramId" });
            DropIndex("dbo.Indiagrams", new[] { "LastIndiagramInfoId" });
            DropIndex("dbo.Indiagrams", new[] { "UserId" });
            DropIndex("dbo.Devices", new[] { "UserId" });
            DropTable("dbo.Versions");
            DropTable("dbo.Settings");
            DropTable("dbo.IndiagramStates");
            DropTable("dbo.IndiagramInfos");
            DropTable("dbo.Indiagrams");
            DropTable("dbo.Users");
            DropTable("dbo.Devices");
        }
    }
}
