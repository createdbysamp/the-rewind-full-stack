using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheRewind.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingValueToTableAGAIN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RatingValue",
                table: "Ratings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingValue",
                table: "Ratings");
        }
    }
}
