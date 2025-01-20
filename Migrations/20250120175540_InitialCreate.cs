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
                name: "Products",
                columns: table => new
                {
                    pid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    p_name = table.Column<string>(type: "varchar(255)", nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.pid);
                    table.CheckConstraint("CHK_Product_Quantity_Price", "stock >= 0 AND price > 0");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    uid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    uname = table.Column<string>(type: "varchar(255)", nullable: false),
                    upass = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.uid);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    cid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    uid = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    phone = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.cid);
                    table.ForeignKey(
                        name: "FK_Customers_Users_uid",
                        column: x => x.uid,
                        principalTable: "Users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    oid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    cid = table.Column<int>(type: "int", nullable: false),
                    o_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.oid);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_cid",
                        column: x => x.cid,
                        principalTable: "Customers",
                        principalColumn: "cid",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    o_details_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    oid = table.Column<int>(type: "int", nullable: false),
                    pid = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.o_details_id);
                    table.CheckConstraint("CHK_OrderDetail_Quantity_Price", "quantity > 0 AND price > 0");
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_oid",
                        column: x => x.oid,
                        principalTable: "Orders",
                        principalColumn: "oid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Products_pid",
                        column: x => x.pid,
                        principalTable: "Products",
                        principalColumn: "pid",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_email",
                table: "Customers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_uid",
                table: "Customers",
                column: "uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_oid_pid",
                table: "OrderDetails",
                columns: new[] { "oid", "pid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_pid",
                table: "OrderDetails",
                column: "pid");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_cid",
                table: "Orders",
                column: "cid");

            migrationBuilder.CreateIndex(
                name: "IX_Products_p_name",
                table: "Products",
                column: "p_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_uname",
                table: "Users",
                column: "uname",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
