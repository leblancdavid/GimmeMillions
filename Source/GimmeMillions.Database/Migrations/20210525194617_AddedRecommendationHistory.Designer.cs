﻿// <auto-generated />
using System;
using GimmeMillions.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GimmeMillions.Database.Migrations
{
    [DbContext(typeof(GimmeMillionsContext))]
    [Migration("20210525194617_AddedRecommendationHistory")]
    partial class AddedRecommendationHistory
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("GimmeMillions.Domain.Authentication.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FirstName")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<int>("Role")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StocksWatchlistString")
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("GimmeMillions.Domain.Stocks.Recommendations.StockRecommendationHistory", b =>
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

                    b.Property<string>("SystemId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("RecommendationHistories");
                });

            modelBuilder.Entity("GimmeMillions.Domain.Stocks.StockData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("AdjustedClose")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("AveragePercentPeriodRange")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Close")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("High")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Low")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Open")
                        .HasColumnType("TEXT");

                    b.Property<int>("Period")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("PreviousClose")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Signal")
                        .HasColumnType("TEXT");

                    b.Property<string>("Symbol")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Volume")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("StockData");
                });

            modelBuilder.Entity("GimmeMillions.Domain.Stocks.StockRecommendation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateUpdated")
                        .HasColumnType("TEXT");

                    b.Property<int?>("LastDataId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("LowPrediction")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Prediction")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Sentiment")
                        .HasColumnType("TEXT");

                    b.Property<string>("SystemId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("LastDataId");

                    b.ToTable("LastRecommendations");
                });

            modelBuilder.Entity("GimmeMillions.Domain.Stocks.StockRecommendation", b =>
                {
                    b.HasOne("GimmeMillions.Domain.Stocks.StockData", "LastData")
                        .WithMany()
                        .HasForeignKey("LastDataId");

                    b.Navigation("LastData");
                });
#pragma warning restore 612, 618
        }
    }
}
