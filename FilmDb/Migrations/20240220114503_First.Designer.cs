﻿// <auto-generated />
using System;
using FilmDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FilmDb.Migrations
{
    [DbContext(typeof(FilmDbContext))]
    [Migration("20240220114503_First")]
    partial class First
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.16")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("FilmDb.Model.Actor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Actors");
                });

            modelBuilder.Entity("FilmDb.Model.ActorFilm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ActorId")
                        .HasColumnType("int");

                    b.Property<int>("FilmId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ActorId");

                    b.HasIndex("FilmId");

                    b.ToTable("ActorFilms");
                });

            modelBuilder.Entity("FilmDb.Model.Advice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<Guid>("Key")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("Advices");
                });

            modelBuilder.Entity("FilmDb.Model.AdviceCustomProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AdviceId")
                        .HasColumnType("int");

                    b.Property<int>("CustomPropertyId")
                        .HasColumnType("int");

                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AdviceId");

                    b.HasIndex("CustomPropertyId");

                    b.ToTable("AdviceCustomPropertys");
                });

            modelBuilder.Entity("FilmDb.Model.AdviceFilm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AdviceId")
                        .HasColumnType("int");

                    b.Property<int>("FilmId")
                        .HasColumnType("int");

                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AdviceId");

                    b.HasIndex("FilmId");

                    b.ToTable("AdviceFilms");
                });

            modelBuilder.Entity("FilmDb.Model.Country", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Countrys");
                });

            modelBuilder.Entity("FilmDb.Model.CountryFilm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CountryId")
                        .HasColumnType("int");

                    b.Property<int>("FilmId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CountryId");

                    b.HasIndex("FilmId");

                    b.ToTable("CountryFilms");
                });

            modelBuilder.Entity("FilmDb.Model.CustomProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("CustomPropertys");
                });

            modelBuilder.Entity("FilmDb.Model.Film", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KinopoiskId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Poster")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("Rating")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Slogan")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Year")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Films");
                });

            modelBuilder.Entity("FilmDb.Model.FilmCustomProperty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CustomPropertyId")
                        .HasColumnType("int");

                    b.Property<int>("FilmId")
                        .HasColumnType("int");

                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CustomPropertyId");

                    b.HasIndex("FilmId");

                    b.ToTable("FilmCustomPropertys");
                });

            modelBuilder.Entity("FilmDb.Model.Genre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("FilmDb.Model.GenreFilm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("FilmId")
                        .HasColumnType("int");

                    b.Property<int>("GenreId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FilmId");

                    b.HasIndex("GenreId");

                    b.ToTable("GenreFilms");
                });

            modelBuilder.Entity("FilmDb.Model.IMDbLoading", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DataSetID")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EnglishActors")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EnglishCountries")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EnglishDescription")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EnglishGenries")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EnglishProducers")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EnglishTags")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EnglishTitle")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IMDbId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Poster")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Rating")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RussianActors")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RussianCountries")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RussianDescription")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RussianGenries")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RussianProducers")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RussianTags")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RussianTitle")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Year")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("IMDbLoadings");
                });

            modelBuilder.Entity("FilmDb.Model.Producer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Producers");
                });

            modelBuilder.Entity("FilmDb.Model.ProducerFilm", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("FilmId")
                        .HasColumnType("int");

                    b.Property<int>("ProducerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FilmId");

                    b.HasIndex("ProducerId");

                    b.ToTable("ProducerFilms");
                });

            modelBuilder.Entity("FilmDb.Model.ActorFilm", b =>
                {
                    b.HasOne("FilmDb.Model.Actor", "Actor")
                        .WithMany()
                        .HasForeignKey("ActorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FilmDb.Model.Film", "Film")
                        .WithMany("ActorFilm")
                        .HasForeignKey("FilmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Actor");

                    b.Navigation("Film");
                });

            modelBuilder.Entity("FilmDb.Model.AdviceCustomProperty", b =>
                {
                    b.HasOne("FilmDb.Model.Advice", "Advice")
                        .WithMany("AdviceCustomProperty")
                        .HasForeignKey("AdviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FilmDb.Model.CustomProperty", "CustomProperty")
                        .WithMany()
                        .HasForeignKey("CustomPropertyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Advice");

                    b.Navigation("CustomProperty");
                });

            modelBuilder.Entity("FilmDb.Model.AdviceFilm", b =>
                {
                    b.HasOne("FilmDb.Model.Advice", "Advice")
                        .WithMany("AdviceFilm")
                        .HasForeignKey("AdviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FilmDb.Model.Film", "Film")
                        .WithMany()
                        .HasForeignKey("FilmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Advice");

                    b.Navigation("Film");
                });

            modelBuilder.Entity("FilmDb.Model.CountryFilm", b =>
                {
                    b.HasOne("FilmDb.Model.Country", "Country")
                        .WithMany()
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FilmDb.Model.Film", "Film")
                        .WithMany("CountryFilm")
                        .HasForeignKey("FilmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Country");

                    b.Navigation("Film");
                });

            modelBuilder.Entity("FilmDb.Model.FilmCustomProperty", b =>
                {
                    b.HasOne("FilmDb.Model.CustomProperty", "CustomProperty")
                        .WithMany()
                        .HasForeignKey("CustomPropertyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FilmDb.Model.Film", "Film")
                        .WithMany("FilmCustomProperty")
                        .HasForeignKey("FilmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CustomProperty");

                    b.Navigation("Film");
                });

            modelBuilder.Entity("FilmDb.Model.GenreFilm", b =>
                {
                    b.HasOne("FilmDb.Model.Film", "Film")
                        .WithMany("GenreFilm")
                        .HasForeignKey("FilmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FilmDb.Model.Genre", "Genre")
                        .WithMany()
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Film");

                    b.Navigation("Genre");
                });

            modelBuilder.Entity("FilmDb.Model.ProducerFilm", b =>
                {
                    b.HasOne("FilmDb.Model.Film", "Film")
                        .WithMany("ProducerFilm")
                        .HasForeignKey("FilmId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FilmDb.Model.Producer", "Producer")
                        .WithMany()
                        .HasForeignKey("ProducerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Film");

                    b.Navigation("Producer");
                });

            modelBuilder.Entity("FilmDb.Model.Advice", b =>
                {
                    b.Navigation("AdviceCustomProperty");

                    b.Navigation("AdviceFilm");
                });

            modelBuilder.Entity("FilmDb.Model.Film", b =>
                {
                    b.Navigation("ActorFilm");

                    b.Navigation("CountryFilm");

                    b.Navigation("FilmCustomProperty");

                    b.Navigation("GenreFilm");

                    b.Navigation("ProducerFilm");
                });
#pragma warning restore 612, 618
        }
    }
}
