using System;
using Microsoft.EntityFrameworkCore;
using UnionAll.Domain;

namespace UnionAll.Store
{
    public class DefineDbSchema
    {
        /// <summary>
        /// Specify the database creation DML options for the Node DB table explicitly.  Column order must still be 
        /// defined by manually editing the EF core migration output file.
        /// </summary>
        /// <param name="mod">the model builder object being used to define the database entities in EF Core</param>
        public void DefineNode(ModelBuilder mod)
        {
            mod.Entity<Node>().ToTable("NODE");

            mod.Entity<Node>()
                .HasKey(ns => ns.NodeId)
                .HasName("PK_NODE_NODEID");

            mod.Entity<Node>()
                .Property(p => p.NodeId)
                .HasColumnName("NODE_ID")
                .HasColumnType("INTEGER");

            mod.Entity<Node>()
                .Property(p => p.NodeName)
                .HasColumnName("NODE_NAME")
                .HasColumnType("TEXT")
                .IsRequired()
                .HasMaxLength(512);

            mod.Entity<Node>()
                .Property(p => p.NodeType)
                .HasColumnName("NODE_TYPE")
                .HasColumnType("INTEGER");

            mod.Entity<Node>()
                .Property(p => p.NodeTopic)
                .HasColumnName("NODE_TOPIC")
                .HasColumnType("INTEGER");

            mod.Entity<Node>()
                .Property(p => p.NodeMatchName)
                .HasColumnName("NODE_MATCH_NAME")
                .HasColumnType("TEXT")
                .IsRequired()
                .HasMaxLength(512);

            mod.Entity<Node>()
                .Property(p => p.NodeStatus)
                .HasColumnName("NODE_STATUS")
                .HasColumnType("INTEGER");

            //mod.Entity<Node>()
            //    .Property<DateTime>("LAST_MODIFIED")
            //    .HasColumnType("DATETIME");

            mod.Entity<Node>().Ignore("HasEdits");
        }

        /// <summary>
        /// Specify the database creation DML options for the Vector DB table explicitly.  Column order must still be 
        /// defined by manually editing the EF core migration output file.
        /// </summary>
        /// <param name="mod">the model builder object being used to define the database in EF Core</param>
        public void DefineVector(ModelBuilder mod)
        {
            mod.Entity<Vector>().ToTable("VECTOR");

            mod.Entity<Vector>()
                .HasKey(v => v.VectorId)
                .HasName("PK_VECTOR_VECTORID");

            mod.Entity<Vector>()
                .Property(v => v.VectorId)
                .HasColumnName("VECTOR_ID")
                .HasColumnType("INTEGER");

            mod.Entity<Vector>()
                .Property(v => v.VectorPhrase)
                .HasColumnName("VECTOR_PHRASE")
                .HasColumnType("TEXT");

            mod.Entity<Vector>()
                .Property(v => v.NodeSubject)
                .HasColumnName("NODE_SUBJECT")
                .HasColumnType("INTEGER");

            mod.Entity<Vector>()
                .Property(v => v.NodeObject)
                .HasColumnName("NODE_OBJECT")
                .HasColumnType("INTEGER");

            mod.Entity<Vector>()
                .Property(v => v.NodeParent)
                .HasColumnName("NODE_PARENT")
                .HasColumnType("INTEGER");

            mod.Entity<Vector>()
                .Property(v => v.NodeRoot)
                .HasColumnName("NODE_ROOT")
                .HasColumnType("INTEGER");

            mod.Entity<Vector>()
               .Property(v => v.VectorStatus)
               .HasColumnName("VECTOR_STATUS")
               .HasColumnType("INTEGER");

            //mod.Entity<Vector>()
            //    .Property<DateTime>("LAST_MODIFIED")
            //    .HasColumnType("DATETIME");

            mod.Entity<Vector>().Ignore("HasEdits");

            mod.Entity<Vector>()
                .HasIndex(i => i.NodeObject)
                .HasName("IX_NODE_NODEOBJECT");
            mod.Entity<Vector>()
                .HasIndex(i => i.NodeSubject)
                .HasName("IX_NODE_NODESUBJECT");
            mod.Entity<Vector>()
                .HasIndex(i => i.NodeParent)
                .HasName("IX_NODE_NODEPARENT");
            mod.Entity<Vector>()
                .HasIndex(i => i.NodeRoot)
                .HasName("IX_NODE_NODEROOT");
        }

        /// <summary>
        /// Define the Sqlite specific Data Definition Language statements.
        /// </summary>
        /// <param name="mod">the model builder object being used to define the database in EF Core</param>
        public void DefineSqliteDDL(ModelBuilder mod)
        {
            mod.Entity<Node>()
                .Property<DateTime>("LAST_MODIFIED")
                .HasColumnType("DATETIME");

            mod.Entity<Vector>()
                .Property<DateTime>("LAST_MODIFIED")
                .HasColumnType("DATETIME");

            mod.Entity<Node>()
                .Property(ns => ns.NodeId)
                .ValueGeneratedOnAdd();

            mod.Entity<Vector>()
                .Property(v => v.VectorId)
                .ValueGeneratedOnAdd();
        }

        /// <summary>
        /// Define the NpgSql PostgreSQL specific Data Definition Language statements
        /// </summary>
        /// <param name="mod">the model builder object being used to define the database in EF Core</param>
        public void DefineNpgSqlDDL(ModelBuilder mod)
        {
            mod.Entity<Node>()
                .Property<DateTime>("LAST_MODIFIED")
                .HasColumnType("TIMESTAMP");

            mod.Entity<Vector>()
                .Property<DateTime>("LAST_MODIFIED")
                .HasColumnType("TIMESTAMP");

            mod.HasSequence<long>("NODEID_SEQ")
                .StartsAt(1)
                .IncrementsBy(1);

            mod.Entity<Node>()
                .Property(ns => ns.NodeId)
                .HasDefaultValueSql("nextval('\"NODEID_SEQ\"')");

            mod.HasSequence<long>("VECTORID_SEQ")
                .StartsAt(1)
                .IncrementsBy(1);

            mod.Entity<Vector>()
                .Property(v => v.VectorId)
                .HasDefaultValueSql("nextval('\"VECTORID_SEQ\"')");
        }

        /// <summary>
        /// Define the default (in-memory tests, etc) specific Data Definition Language statements
        /// </summary>
        /// <param name="mod">the model builder object being used to define the database in EF Core</param>
        public void DefineDefaultDDL(ModelBuilder mod)
        {
            mod.Entity<Node>()
                .Property<DateTime>("LAST_MODIFIED")
                .HasColumnType("DATETIME");

            mod.Entity<Vector>()
                .Property<DateTime>("LAST_MODIFIED")
                .HasColumnType("DATETIME");
        }
    }
}
