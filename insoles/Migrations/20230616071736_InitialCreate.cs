using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace insoles.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Apellidos = table.Column<string>(type: "TEXT", nullable: true),
                    FechaNacimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Lugar = table.Column<string>(type: "TEXT", nullable: true),
                    Peso = table.Column<float>(type: "REAL", nullable: true),
                    Altura = table.Column<float>(type: "REAL", nullable: true),
                    LongitudPie = table.Column<float>(type: "REAL", nullable: true),
                    NumeroPie = table.Column<int>(type: "INTEGER", nullable: true),
                    Profesion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PacienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    csv = table.Column<string>(type: "TEXT", nullable: false),
                    video1 = table.Column<string>(type: "TEXT", nullable: true),
                    video2 = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tests_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tests_PacienteId",
                table: "Tests",
                column: "PacienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "Pacientes");
        }
    }
}
