using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faluf.BannerGen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BannerRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AIModel = table.Column<int>(type: "int", nullable: false),
                    BannerFormats = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptimizedInstructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CTAButtonText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BannerLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HexColors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BannerFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediaType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BannerRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BannerFiles_BannerRequests_BannerRequestId",
                        column: x => x.BannerRequestId,
                        principalTable: "BannerRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AIModel = table.Column<int>(type: "int", nullable: false),
                    BannerFormat = table.Column<int>(type: "int", nullable: false),
                    TotalCostUSD = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    GeneratedHtmlUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedCSSUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedJSUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BannerRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Banners_BannerRequests_BannerRequestId",
                        column: x => x.BannerRequestId,
                        principalTable: "BannerRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OverlayTexts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontFamilyUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FontWeight = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BannerRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverlayTexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OverlayTexts_BannerRequests_BannerRequestId",
                        column: x => x.BannerRequestId,
                        principalTable: "BannerRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BannerFiles_BannerRequestId",
                table: "BannerFiles",
                column: "BannerRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_BannerRequestId",
                table: "Banners",
                column: "BannerRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_OverlayTexts_BannerRequestId",
                table: "OverlayTexts",
                column: "BannerRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannerFiles");

            migrationBuilder.DropTable(
                name: "Banners");

            migrationBuilder.DropTable(
                name: "OverlayTexts");

            migrationBuilder.DropTable(
                name: "BannerRequests");
        }
    }
}
