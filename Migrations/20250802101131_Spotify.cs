using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Spotify : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Spotify",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Ts = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Platform = table.Column<string>(type: "longtext", nullable: false),
                    MsPlayed = table.Column<int>(type: "int", nullable: false),
                    ConnCountry = table.Column<string>(type: "longtext", nullable: false),
                    IpAddr = table.Column<string>(type: "longtext", nullable: false),
                    TrackName = table.Column<string>(type: "longtext", nullable: false),
                    AlbumArtistName = table.Column<string>(type: "longtext", nullable: false),
                    AlbumName = table.Column<string>(type: "longtext", nullable: false),
                    SpotifyTrackUri = table.Column<string>(type: "longtext", nullable: false),
                    EpisodeName = table.Column<string>(type: "longtext", nullable: false),
                    EpisodeShowName = table.Column<string>(type: "longtext", nullable: false),
                    SpotifyEpisodeUri = table.Column<string>(type: "longtext", nullable: false),
                    AudiobookTitle = table.Column<string>(type: "longtext", nullable: false),
                    AudiobookUri = table.Column<string>(type: "longtext", nullable: false),
                    AudiobookChapterUri = table.Column<string>(type: "longtext", nullable: false),
                    AudiobookChapterTitle = table.Column<string>(type: "longtext", nullable: false),
                    ReasonStart = table.Column<string>(type: "longtext", nullable: false),
                    ReasonEnd = table.Column<string>(type: "longtext", nullable: false),
                    Shuffle = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Skipped = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Offline = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OfflineTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    IncognitoMode = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spotify", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Spotify");
        }
    }
}
