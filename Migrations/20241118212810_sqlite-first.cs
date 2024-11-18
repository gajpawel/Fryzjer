using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fryzjer.Migrations
{
    /// <inheritdoc />
    public partial class sqlitefirst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Surname = table.Column<string>(type: "TEXT", nullable: false),
                    Gender = table.Column<char>(type: "TEXT", nullable: false),
                    Login = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Place",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    address = table.Column<string>(type: "TEXT", nullable: true),
                    logoPath = table.Column<string>(type: "TEXT", nullable: true),
                    telephoneNumber = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Place", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    price = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StationType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    descripion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hairdresser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    surname = table.Column<string>(type: "TEXT", nullable: true),
                    login = table.Column<string>(type: "TEXT", nullable: true),
                    password = table.Column<string>(type: "TEXT", nullable: true),
                    photoPath = table.Column<string>(type: "TEXT", nullable: true),
                    status = table.Column<char>(type: "TEXT", nullable: false),
                    PlaceId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hairdresser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hairdresser_Place_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "Place",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServiceStation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    StationTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceStation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceStation_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceStation_StationType_StationTypeId",
                        column: x => x.StationTypeId,
                        principalTable: "StationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Station",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StationTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlaceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Station", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Station_Place_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "Place",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Station_StationType_StationTypeId",
                        column: x => x.StationTypeId,
                        principalTable: "StationType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    time = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    status = table.Column<char>(type: "TEXT", nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    HairdresserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservation_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservation_Hairdresser_HairdresserId",
                        column: x => x.HairdresserId,
                        principalTable: "Hairdresser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservation_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Specialization",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HairdresserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Specialization_Hairdresser_HairdresserId",
                        column: x => x.HairdresserId,
                        principalTable: "Hairdresser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Specialization_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReservedStation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReservationId = table.Column<int>(type: "INTEGER", nullable: false),
                    StationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservedStation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservedStation_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservedStation_Station_StationId",
                        column: x => x.StationId,
                        principalTable: "Station",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hairdresser_PlaceId",
                table: "Hairdresser",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_ClientId",
                table: "Reservation",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_HairdresserId",
                table: "Reservation",
                column: "HairdresserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_ServiceId",
                table: "Reservation",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservedStation_ReservationId",
                table: "ReservedStation",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservedStation_StationId",
                table: "ReservedStation",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceStation_ServiceId",
                table: "ServiceStation",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceStation_StationTypeId",
                table: "ServiceStation",
                column: "StationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Specialization_HairdresserId",
                table: "Specialization",
                column: "HairdresserId");

            migrationBuilder.CreateIndex(
                name: "IX_Specialization_ServiceId",
                table: "Specialization",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Station_PlaceId",
                table: "Station",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Station_StationTypeId",
                table: "Station",
                column: "StationTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservedStation");

            migrationBuilder.DropTable(
                name: "ServiceStation");

            migrationBuilder.DropTable(
                name: "Specialization");

            migrationBuilder.DropTable(
                name: "Reservation");

            migrationBuilder.DropTable(
                name: "Station");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "Hairdresser");

            migrationBuilder.DropTable(
                name: "Service");

            migrationBuilder.DropTable(
                name: "StationType");

            migrationBuilder.DropTable(
                name: "Place");
        }
    }
}
