using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace B1_TestTask_1.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalculationResults",
                columns: table => new
                {
                    SumOfIntegers = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    MedianOfDoubles = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "StringsData",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateColumn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LatinCharsColumn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RussianCharsColumn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntegerColumn = table.Column<int>(type: "int", nullable: false),
                    DoubleColumn = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StringsData", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalculationResults");

            migrationBuilder.DropTable(
                name: "StringsData");
        }
    }
}
