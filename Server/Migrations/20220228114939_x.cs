using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Server.Migrations
{
    public partial class x : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ST__AUDIT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "serial", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UTC_DT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HTTP_STATUS_CODE = table.Column<int>(type: "integer", nullable: false, defaultValue: 200),
                    DESCRIPTION = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OBJECT = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SOURCE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SOURCE_IP = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SOURCE_DEVICE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ST__AUDIT", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ST__AUDIT");
        }
    }
}
