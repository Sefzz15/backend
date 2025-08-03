using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixedMigration : Migration
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
                    ts = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    platform = table.Column<string>(type: "longtext", nullable: true),
                    ms_played = table.Column<int>(type: "int", nullable: false),
                    conn_country = table.Column<string>(type: "longtext", nullable: true),
                    ip_addr = table.Column<string>(type: "longtext", nullable: true),
                    master_metadata_track_name = table.Column<string>(type: "longtext", nullable: true),
                    master_metadata_album_artist_name = table.Column<string>(type: "longtext", nullable: true),
                    master_metadata_album_album_name = table.Column<string>(type: "longtext", nullable: true),
                    spotify_track_uri = table.Column<string>(type: "longtext", nullable: true),
                    episode_name = table.Column<string>(type: "longtext", nullable: true),
                    episode_show_name = table.Column<string>(type: "longtext", nullable: true),
                    spotify_episode_uri = table.Column<string>(type: "longtext", nullable: true),
                    audiobook_title = table.Column<string>(type: "longtext", nullable: true),
                    audiobook_uri = table.Column<string>(type: "longtext", nullable: true),
                    audiobook_chapter_uri = table.Column<string>(type: "longtext", nullable: true),
                    audiobook_chapter_title = table.Column<string>(type: "longtext", nullable: true),
                    reason_start = table.Column<string>(type: "longtext", nullable: true),
                    reason_end = table.Column<string>(type: "longtext", nullable: true),
                    shuffle = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    skipped = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    offline = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    offline_timestamp = table.Column<long>(type: "bigint", nullable: false),
                    incognito_mode = table.Column<bool>(type: "tinyint(1)", nullable: false)
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
