using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArtistsCatalog",
                columns: table => new
                {
                    ArtistId = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    FetchedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistsCatalog", x => x.ArtistId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Pid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Pname = table.Column<string>(type: "longtext", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Pid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

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
                    shuffle = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    skipped = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    offline = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    offline_timestamp = table.Column<long>(type: "bigint", nullable: true),
                    incognito_mode = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    TrackId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spotify", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TrackGenreWeights",
                columns: table => new
                {
                    TrackId = table.Column<string>(type: "varchar(255)", nullable: false),
                    Genre = table.Column<string>(type: "varchar(255)", nullable: false),
                    Weight = table.Column<double>(type: "double", nullable: false),
                    BuiltAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackGenreWeights", x => new { x.TrackId, x.Genre });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TracksCatalog",
                columns: table => new
                {
                    TrackId = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    AlbumId = table.Column<string>(type: "longtext", nullable: true),
                    AlbumName = table.Column<string>(type: "longtext", nullable: true),
                    FetchedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TracksCatalog", x => x.TrackId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Uid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Uname = table.Column<string>(type: "varchar(255)", nullable: false),
                    Upass = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Uid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArtistGenres",
                columns: table => new
                {
                    ArtistId = table.Column<string>(type: "varchar(255)", nullable: false),
                    Genre = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistGenres", x => new { x.ArtistId, x.Genre });
                    table.ForeignKey(
                        name: "FK_ArtistGenres_ArtistsCatalog_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "ArtistsCatalog",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TrackArtists",
                columns: table => new
                {
                    TrackId = table.Column<string>(type: "varchar(255)", nullable: false),
                    ArtistId = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackArtists", x => new { x.TrackId, x.ArtistId });
                    table.ForeignKey(
                        name: "FK_TrackArtists_ArtistsCatalog_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "ArtistsCatalog",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackArtists_TracksCatalog_TrackId",
                        column: x => x.TrackId,
                        principalTable: "TracksCatalog",
                        principalColumn: "TrackId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Fid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Uid = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "longtext", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Address = table.Column<string>(type: "longtext", nullable: true),
                    Phone = table.Column<string>(type: "longtext", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Fid);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Users_Uid",
                        column: x => x.Uid,
                        principalTable: "Users",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Oid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Uid = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Oid);
                    table.ForeignKey(
                        name: "FK_Orders_Users_Uid",
                        column: x => x.Uid,
                        principalTable: "Users",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    Oid = table.Column<int>(type: "int", nullable: false),
                    Pid = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => new { x.Oid, x.Pid });
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_Oid",
                        column: x => x.Oid,
                        principalTable: "Orders",
                        principalColumn: "Oid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Products_Pid",
                        column: x => x.Pid,
                        principalTable: "Products",
                        principalColumn: "Pid",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistGenres_Genre",
                table: "ArtistGenres",
                column: "Genre");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_Uid",
                table: "Feedbacks",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_Pid",
                table: "OrderDetails",
                column: "Pid");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Uid",
                table: "Orders",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "IX_Spotify_TrackId",
                table: "Spotify",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackArtists_ArtistId",
                table: "TrackArtists",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Uname",
                table: "Users",
                column: "Uname",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistGenres");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Spotify");

            migrationBuilder.DropTable(
                name: "TrackArtists");

            migrationBuilder.DropTable(
                name: "TrackGenreWeights");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ArtistsCatalog");

            migrationBuilder.DropTable(
                name: "TracksCatalog");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
