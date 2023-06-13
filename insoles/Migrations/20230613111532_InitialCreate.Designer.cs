﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using insoles.Database;

#nullable disable

namespace insoles.Migrations
{
    [DbContext(typeof(DBContextSqlLite))]
    [Migration("20230613111532_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("insoles.Model.Paciente", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<float?>("Altura")
                        .HasColumnType("REAL");

                    b.Property<string>("Apellidos")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("FechaNacimiento")
                        .HasColumnType("TEXT");

                    b.Property<float?>("LongitudPie")
                        .HasColumnType("REAL");

                    b.Property<string>("Lugar")
                        .HasColumnType("TEXT");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("NumeroPie")
                        .HasColumnType("INTEGER");

                    b.Property<float?>("Peso")
                        .HasColumnType("REAL");

                    b.Property<string>("Profesion")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Pacientes");
                });

            modelBuilder.Entity("insoles.Model.Test", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<int?>("PacienteId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("csv")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("video1")
                        .HasColumnType("TEXT");

                    b.Property<string>("video2")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PacienteId");

                    b.ToTable("Test");
                });

            modelBuilder.Entity("insoles.Model.Test", b =>
                {
                    b.HasOne("insoles.Model.Paciente", null)
                        .WithMany("Tests")
                        .HasForeignKey("PacienteId");
                });

            modelBuilder.Entity("insoles.Model.Paciente", b =>
                {
                    b.Navigation("Tests");
                });
#pragma warning restore 612, 618
        }
    }
}
