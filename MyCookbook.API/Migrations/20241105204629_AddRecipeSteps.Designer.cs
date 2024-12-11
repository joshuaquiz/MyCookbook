﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyCookbook.API;

#nullable disable

namespace MyCookbook.API.Migrations
{
    [DbContext(typeof(MyCookbookContext))]
    [Migration("20241105204629_AddRecipeSteps")]
    partial class AddRecipeSteps
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("MyCookbook.API.Author", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("BackgroundImage")
                        .HasColumnType("TEXT");

                    b.Property<string>("Image")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("UserGuid")
                        .HasColumnType("TEXT");

                    b.HasKey("Guid");

                    b.HasIndex("UserGuid");

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("MyCookbook.API.Ingredient", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Image")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Guid");

                    b.ToTable("Ingredients");
                });

            modelBuilder.Entity("MyCookbook.API.Recipe", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("AuthorGuid")
                        .HasColumnType("TEXT");

                    b.Property<string>("Image")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("RecipeUrlGuid")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("TotalTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Guid");

                    b.HasIndex("AuthorGuid");

                    b.HasIndex("RecipeUrlGuid");

                    b.ToTable("Recipes");
                });

            modelBuilder.Entity("MyCookbook.API.RecipeStep", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Instructions")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("RecipeGuid")
                        .HasColumnType("TEXT");

                    b.Property<int>("RecipeStepType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StepNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("Guid");

                    b.HasIndex("RecipeGuid");

                    b.ToTable("RecipeSteps");
                });

            modelBuilder.Entity("MyCookbook.API.RecipeStepIngredient", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("IngredientGuid")
                        .HasColumnType("TEXT");

                    b.Property<int>("Measurement")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.Property<string>("Quantity")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("RecipeStepGuid")
                        .HasColumnType("TEXT");

                    b.HasKey("Guid");

                    b.HasIndex("IngredientGuid");

                    b.HasIndex("RecipeStepGuid");

                    b.ToTable("RecipeStepIngredients");
                });

            modelBuilder.Entity("MyCookbook.API.RecipeUrl", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("CompletedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Exception")
                        .HasColumnType("TEXT");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LdJson")
                        .HasColumnType("TEXT");

                    b.Property<int>("ParserVersion")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProcessingStatus")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("StatusCode")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Uri")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Guid");

                    b.ToTable("RecipeUrls");
                });

            modelBuilder.Entity("MyCookbook.API.User", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Image")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Guid");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("MyCookbook.API.Author", b =>
                {
                    b.HasOne("MyCookbook.API.User", "User")
                        .WithMany()
                        .HasForeignKey("UserGuid");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MyCookbook.API.Recipe", b =>
                {
                    b.HasOne("MyCookbook.API.Author", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MyCookbook.API.RecipeUrl", "RecipeUrl")
                        .WithMany()
                        .HasForeignKey("RecipeUrlGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("RecipeUrl");
                });

            modelBuilder.Entity("MyCookbook.API.RecipeStep", b =>
                {
                    b.HasOne("MyCookbook.API.Recipe", "Recipe")
                        .WithMany("RecipeSteps")
                        .HasForeignKey("RecipeGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("MyCookbook.API.RecipeStepIngredient", b =>
                {
                    b.HasOne("MyCookbook.API.Ingredient", "Ingredient")
                        .WithMany()
                        .HasForeignKey("IngredientGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MyCookbook.API.RecipeStep", "RecipeStep")
                        .WithMany("RecipeIngredients")
                        .HasForeignKey("RecipeStepGuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Ingredient");

                    b.Navigation("RecipeStep");
                });

            modelBuilder.Entity("MyCookbook.API.Recipe", b =>
                {
                    b.Navigation("RecipeSteps");
                });

            modelBuilder.Entity("MyCookbook.API.RecipeStep", b =>
                {
                    b.Navigation("RecipeIngredients");
                });
#pragma warning restore 612, 618
        }
    }
}
