using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCenterApp.Migrations
{
    /// <inheritdoc />
    public partial class AddBodyTransformChangeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MemberId",
                table: "BodyTransformRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "GoalType",
                table: "BodyTransformRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "GeneratedImagePath",
                table: "BodyTransformRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ExpectedChangePercent",
                table: "BodyTransformRequests",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "StartWeightKg",
                table: "BodyTransformRequests",
                type: "float",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BodyTransformRequests_MemberId",
                table: "BodyTransformRequests",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_BodyTransformRequests_AspNetUsers_MemberId",
                table: "BodyTransformRequests",
                column: "MemberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BodyTransformRequests_AspNetUsers_MemberId",
                table: "BodyTransformRequests");

            migrationBuilder.DropIndex(
                name: "IX_BodyTransformRequests_MemberId",
                table: "BodyTransformRequests");

            migrationBuilder.DropColumn(
                name: "ExpectedChangePercent",
                table: "BodyTransformRequests");

            migrationBuilder.DropColumn(
                name: "StartWeightKg",
                table: "BodyTransformRequests");

            migrationBuilder.AlterColumn<string>(
                name: "MemberId",
                table: "BodyTransformRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "GoalType",
                table: "BodyTransformRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "GeneratedImagePath",
                table: "BodyTransformRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
