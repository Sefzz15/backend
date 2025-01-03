﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using backend.Data;

#nullable disable

namespace backend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("backend.Models.Customer", b =>
                {
                    b.Property<int>("c_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("address")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("city")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("first_name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("last_name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("phone")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.HasKey("c_id");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("backend.Models.Order", b =>
                {
                    b.Property<int>("o_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("c_id")
                        .HasColumnType("int");

                    b.Property<DateTime>("o_date")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)")
                        .HasDefaultValue("Pending");

                    b.Property<decimal>("total_amount")
                        .HasColumnType("DECIMAL(10,2)");

                    b.HasKey("o_id");

                    b.HasIndex("c_id");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("backend.Models.OrderDetail", b =>
                {
                    b.Property<int>("o_details_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("o_id")
                        .HasColumnType("int");

                    b.Property<int>("p_id")
                        .HasColumnType("int");

                    b.Property<decimal>("price")
                        .HasColumnType("DECIMAL(10,2)");

                    b.Property<int>("quantity")
                        .HasColumnType("int");

                    b.HasKey("o_details_id");

                    b.HasIndex("p_id");

                    b.HasIndex("o_id", "p_id")
                        .IsUnique();

                    b.ToTable("OrderDetails");
                });

            modelBuilder.Entity("backend.Models.Product", b =>
                {
                    b.Property<int>("p_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("description")
                        .HasColumnType("TEXT");

                    b.Property<string>("p_name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<decimal>("price")
                        .HasColumnType("DECIMAL(10,2)");

                    b.Property<int>("stock_quantity")
                        .HasColumnType("int");

                    b.HasKey("p_id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("backend.Models.User", b =>
                {
                    b.Property<int>("uid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("uname")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("upass")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("uid");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("backend.Models.Order", b =>
                {
                    b.HasOne("backend.Models.Customer", "Customer")
                        .WithMany("Orders")
                        .HasForeignKey("c_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("backend.Models.OrderDetail", b =>
                {
                    b.HasOne("backend.Models.Order", "Order")
                        .WithMany("OrderDetails")
                        .HasForeignKey("o_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("backend.Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("p_id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("backend.Models.Customer", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("backend.Models.Order", b =>
                {
                    b.Navigation("OrderDetails");
                });
#pragma warning restore 612, 618
        }
    }
}
