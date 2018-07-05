﻿// <auto-generated />
using System;
using DataFork.DataStore.Services;
using DataFork.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DataFork.DataStore.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20180630193655_Sqlite")]
    partial class Sqlite
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846");

            modelBuilder.Entity("DataFork.Domain.Node", b =>
                {
                    b.Property<int>("NodeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("NODE_ID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LAST_MODIFIED")
                        .HasColumnType("DATETIME");

                    b.Property<string>("NodeMatchName")
                        .IsRequired()
                        .HasColumnName("NODE_MATCH_NAME")
                        .HasColumnType("TEXT")
                        .HasMaxLength(512);

                    b.Property<string>("NodeName")
                        .IsRequired()
                        .HasColumnName("NODE_NAME")
                        .HasColumnType("TEXT")
                        .HasMaxLength(512);

                    b.Property<NodesStatusValues>("NodeStatus")
                        .HasColumnName("NODE_STATUS")
                        .HasColumnType("INTEGER");

                    b.Property<NodeTopics>("NodeTopic")
                        .HasColumnName("NODE_TOPIC")
                        .HasColumnType("INTEGER");

                    b.Property<NodeValueTypes>("NodeType")
                        .HasColumnName("NODE_TYPE")
                        .HasColumnType("INTEGER");

                    b.HasKey("NodeId")
                        .HasName("PK_NODE_NODEID");

                    b.ToTable("NODE");
                });

            modelBuilder.Entity("DataFork.Domain.Vector", b =>
                {
                    b.Property<int>("VectorId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("VECTOR_ID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LAST_MODIFIED")
                        .HasColumnType("DATETIME");

                    b.Property<int>("NodeObject")
                        .HasColumnName("NODE_OBJECT")
                        .HasColumnType("INTEGER");

                    b.Property<int>("NodeParent")
                        .HasColumnName("NODE_PARENT")
                        .HasColumnType("INTEGER");

                    b.Property<int>("NodeRoot")
                        .HasColumnName("NODE_ROOT")
                        .HasColumnType("INTEGER");

                    b.Property<int>("NodeSubject")
                        .HasColumnName("NODE_SUBJECT")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VectorPhrase")
                        .IsRequired()
                        .HasColumnName("VECTOR_PHRASE")
                        .HasColumnType("TEXT");

                    b.Property<VectorStatusValues>("VectorStatus")
                        .HasColumnName("VECTOR_STATUS")
                        .HasColumnType("INTEGER");

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