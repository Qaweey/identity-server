using Microsoft.EntityFrameworkCore.Migrations;

namespace settl.identityserver.EntityFrameworkCore.Migrations
{
    public partial class addedipaddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "tbl_IdentityServer_RegisterNewDevice",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "tbl_IdentityServer_RegisterNewDevice");
        }
    }
}