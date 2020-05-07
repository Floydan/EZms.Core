using Microsoft.EntityFrameworkCore.Migrations;

namespace EZms.Core.Migrations
{
    public partial class allowedgroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowedGroupsAsJson",
                schema: "EZms",
                table: "ContentVersions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AllowedGroupsAsJson",
                schema: "EZms",
                table: "Content",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedGroupsAsJson",
                schema: "EZms",
                table: "ContentVersions");

            migrationBuilder.DropColumn(
                name: "AllowedGroupsAsJson",
                schema: "EZms",
                table: "Content");
        }
    }
}
