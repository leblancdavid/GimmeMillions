﻿// <auto-generated />
using System;
using GimmeMillions.Database;
using GimmeMillions.SQLDataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GimmeMillions.SQLDataAccess.Migrations
{
    [DbContext(typeof(GimmeMillionsContext))]
    [Migration("20200823194854_LastUpdatedDateTimeStockHistory")]
    partial class LastUpdatedDateTimeStockHistory
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.7");

            modelBuilder.Entity("GimmeMillions.Domain.Features.FeatureVector", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DataStr")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Encoding")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("FeatureVectors");
                });

            modelBuilder.Entity("GimmeMillions.Domain.Stocks.StockHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("HistoricalDataStr")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("TEXT");

                    b.Property<string>("Symbol")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("StockHistories");
                });
#pragma warning restore 612, 618
        }
    }
}
