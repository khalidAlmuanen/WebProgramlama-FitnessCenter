using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCenterApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Trainers",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "ExperienceYears",
                table: "Trainers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_FullName_GymId",
                table: "Trainers",
                columns: new[] { "FullName", "GymId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_Name_GymId",
                table: "Services",
                columns: new[] { "Name", "GymId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gyms_Name",
                table: "Gyms",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Trainers_FullName_GymId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Services_Name_GymId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Gyms_Name",
                table: "Gyms");

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Trainers",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ExperienceYears",
                table: "Trainers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
