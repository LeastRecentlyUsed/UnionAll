﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UnionAll.Store.Services;

namespace UnionAll.Store.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class DataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846");

            modelBuilder.Entity("UnionAll.Domain.Node", b =>
                {
                    b.Property<int>("NodeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("NODE_ID");

                    b.Property<DateTime>("LAST_MODIFIED")
                        .HasColumnType("DATETIME");

                    b.Property<string>("NodeMatchName")
                        .IsRequired()
                        .HasColumnName("NODE_MATCH_NAME")
                        .HasMaxLength(512);

                    b.Property<string>("NodeName")
                        .IsRequired()
                        .HasColumnName("NODE_NAME")
                        .HasMaxLength(512);

                    b.Property<int>("NodeStatus")
                        .HasColumnName("NODE_STATUS");

                    b.Property<int>("NodeTopic")
                        .HasColumnName("NODE_TOPIC");

                    b.Property<int>("NodeType")
                        .HasColumnName("NODE_TYPE");

                    b.HasKey("NodeId")
                        .HasName("PK_NODE_NODEID");

                    b.ToTable("NODE");
                });

            modelBuilder.Entity("UnionAll.Domain.Vector", b =>
                {
                    b.Property<int>("VectorId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("VECTOR_ID");

                    b.Property<DateTime>("LAST_MODIFIED")
                        .HasColumnType("DATETIME");

                    b.Property<int>("NodeObject")
                        .HasColumnName("NODE_OBJECT");

                    b.Property<int>("NodeParent")
                        .HasColumnName("NODE_PARENT");

                    b.Property<int>("NodeRoot")
                        .HasColumnName("NODE_ROOT");

                    b.Property<int>("NodeSubject")
                        .HasColumnName("NODE_SUBJECT");

                    b.Property<string>("VectorPhrase")
                        .IsRequired()
                        .HasColumnName("VECTOR_PHRASE");

                    b.Property<int>("VectorStatus")
                        .HasColumnName("VECTOR_STATUS");

                    b.HasKey("VectorId")
                        .HasName("PK_VECTOR_VECTORID");

                    b.HasIndex("NodeObject")
                        .HasName("IX_NODE_NODEOBJECT");

                    b.HasIndex("NodeParent")
                        .HasName("IX_NODE_NODEPARENT");

                    b.HasIndex("NodeRoot")
                        .HasName("IX_NODE_NODEROOT");

                    b.HasIndex("NodeSubject")
                        .HasName("IX_NODE_NODESUBJECT");

                    b.ToTable("VECTOR");
                });
#pragma warning restore 612, 618
        }
    }
}
