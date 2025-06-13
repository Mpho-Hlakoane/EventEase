using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEase.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Events_EventId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Venues_VenueId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_VenueId",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Venues",
                newName: "VenueId");

            migrationBuilder.RenameColumn(
                name: "BookingDate",
                table: "Bookings",
                newName: "StartDate");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Bookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_VenueId_StartDate_EndDate",
                table: "Bookings",
                columns: new[] { "VenueId", "StartDate", "EndDate" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Events_EventId",
                table: "Bookings",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Venues_VenueId",
                table: "Bookings",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "VenueId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Events_EventId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Venues_VenueId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_VenueId_StartDate_EndDate",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "VenueId",
                table: "Venues",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Bookings",
                newName: "BookingDate");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_VenueId",
                table: "Bookings",
                column: "VenueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Events_EventId",
                table: "Bookings",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Venues_VenueId",
                table: "Bookings",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
