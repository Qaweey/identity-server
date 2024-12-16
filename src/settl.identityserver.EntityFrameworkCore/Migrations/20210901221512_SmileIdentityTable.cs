using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace settl.identityserver.EntityFrameworkCore.Migrations
{
    public partial class SmileIdentityTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_IdentityServer_IdVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JSONVersion = table.Column<string>(type: "varchar(5)", nullable: false),
                    SmileJobId = table.Column<string>(type: "varchar(20)", nullable: false),
                    Job_Id = table.Column<string>(type: "varchar(30)", nullable: false),
                    User_Id = table.Column<string>(type: "varchar(30)", nullable: false),
                    Job_Type = table.Column<int>(type: "int", nullable: false),
                    ResultType = table.Column<string>(type: "varchar(20)", nullable: false),
                    ResultText = table.Column<string>(type: "varchar(20)", nullable: false),
                    ResultCode = table.Column<string>(type: "varchar(5)", nullable: false),
                    IsFinalResult = table.Column<string>(type: "varchar(5)", nullable: false),
                    Return_Personal_Info = table.Column<string>(type: "varchar(50)", nullable: false),
                    Verify_ID_Number = table.Column<string>(type: "varchar(50)", nullable: false),
                    Names = table.Column<string>(type: "varchar(50)", nullable: false),
                    DOB = table.Column<string>(type: "varchar(50)", nullable: false),
                    Gender = table.Column<string>(type: "varchar(50)", nullable: false),
                    Phone_Number = table.Column<string>(type: "varchar(50)", nullable: false),
                    ID_Verification = table.Column<string>(type: "varchar(50)", nullable: false),
                    Source = table.Column<string>(type: "varchar(50)", nullable: false),
                    SecKey = table.Column<string>(type: "varchar(50)", nullable: false),
                    TimeStamp = table.Column<string>(type: "varchar(50)", nullable: false),
                    ReferenceCode = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_IdentityServer_IdVerifications", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_IdentityServer_IdVerifications");
        }
    }
}
