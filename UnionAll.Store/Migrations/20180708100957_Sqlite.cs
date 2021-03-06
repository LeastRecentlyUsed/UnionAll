﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UnionAll.Store.Migrations
{
    public partial class Sqlite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NODE",
                columns: table => new
                {
                    NODE_ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NODE_NAME = table.Column<string>(maxLength: 512, nullable: false),
                    NODE_TYPE = table.Column<int>(nullable: false),
                    NODE_TOPIC = table.Column<int>(nullable: false),
                    NODE_STATUS = table.Column<int>(nullable: false),
                    NODE_MATCH_NAME = table.Column<string>(maxLength: 512, nullable: false),
                    LAST_MODIFIED = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NODE_NODEID", x => x.NODE_ID);
                });

            migrationBuilder.CreateTable(
                name: "VECTOR",
                columns: table => new
                {
                    VECTOR_ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VECTOR_PHRASE = table.Column<string>(nullable: false),
                    NODE_SUBJECT = table.Column<int>(nullable: false),
                    NODE_OBJECT = table.Column<int>(nullable: false),
                    NODE_PARENT = table.Column<int>(nullable: false),
                    NODE_ROOT = table.Column<int>(nullable: false),
                    VECTOR_STATUS = table.Column<int>(nullable: false),
                    LAST_MODIFIED = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VECTOR_VECTORID", x => x.VECTOR_ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NODE_NODEOBJECT",
                table: "VECTOR",
                column: "NODE_OBJECT");

            migrationBuilder.CreateIndex(
                name: "IX_NODE_NODEPARENT",
                table: "VECTOR",
                column: "NODE_PARENT");

            migrationBuilder.CreateIndex(
                name: "IX_NODE_NODEROOT",
                table: "VECTOR",
                column: "NODE_ROOT");

            migrationBuilder.CreateIndex(
                name: "IX_NODE_NODESUBJECT",
                table: "VECTOR",
                column: "NODE_SUBJECT");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NODE");

            migrationBuilder.DropTable(
                name: "VECTOR");
        }
    }
}
