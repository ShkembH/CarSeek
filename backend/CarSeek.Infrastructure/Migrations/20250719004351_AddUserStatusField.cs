using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarSeek.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStatusField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "CarSeekUsers",
                type: "int",
                nullable: false,
                defaultValue: 1); // Default to Approved (1)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "CarSeekUsers");
        }
    }
} 