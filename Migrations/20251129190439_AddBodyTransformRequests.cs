using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCenterApp.Migrations
{
    /// <inheritdoc />
    public partial class AddBodyTransformRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BodyTransformRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GoalType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DurationMonths = table.Column<int>(type: "int", nullable: true),
                    OriginalImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodyTransformRequests", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BodyTransformRequests");
        }
    }
}
