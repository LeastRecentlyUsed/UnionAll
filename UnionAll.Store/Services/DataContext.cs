using Microsoft.EntityFrameworkCore;
using UnionAll.Domain;

namespace UnionAll.Store.Services
{
    public class DataContext: DbContext
    {
        public DbSet<Node> Nodes { get; set; }
        public DbSet<Vector> Vectors { get; set; }

        /// <summary>
        /// A constructor that allows context options to be passed in means that any DB type (vendor) can be configured
        /// and used to create this data store DbContext.  As long as the DB type is supported by EF Core.
        /// <summary>
        public DataContext(DbContextOptions options) : base(options)
        { }

        /// <summary>
        /// Fluent API commands define the database table structures and relationships accurately.
        /// </summary>
        /// <param name="mod">EF core ModelBuilder object.<param>
        protected override void OnModelCreating(ModelBuilder schemaBuilder)
        {
            DefineDbSchema schema = new DefineDbSchema();
            schema.DefineNode(schemaBuilder);
            schema.DefineVector(schemaBuilder);

            if (this.Database.IsSqlite())
            {
                schema.DefineSqliteDDL(schemaBuilder);
            }
            else if (this.Database.IsNpgsql())
            {
                schema.DefineNpgSqlDDL(schemaBuilder);
            }
            else
            {
                schema.DefineDefaultDDL(schemaBuilder);
            }
        }

        /// <summary>
        /// A custom implementation to update the last modified database table column whenever an insert or update has  
        /// been made to a row.  The last modified property does not exist in the application, only in the DB.
        /// </summary>
        /// <returns></returns>
        //public override int SaveChanges()
        //{
        //    foreach (var entry in ChangeTracker.Entries()
        //             .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        //    {
        //        entry.Property("LAST_MODIFIED").CurrentValue = DateTime.Now;
        //    }
        //    return base.SaveChanges();
        //}

    }
}
