using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UnionAll.Domain;

namespace UnionAll.Store
{
    public static class DataTracking
    {
        /// For a non-tracked data entity within the context, determine the change status 
        /// and force the modified/unchanged flag depending on user activity. If the entry.state is not
        /// set for an entity then it will not be recognized by the dbcontext.
        /// <summary>
        /// <param name="entry">The context data object being applied to the database.</param>
        public static void ApplyEditState(EntityEntry entry)
        {
            if (entry.IsKeySet)
            {
                if (((ChangeStatus)entry.Entity).HasEdits)
                {
                    entry.State = EntityState.Modified;
                    entry.Property("LAST_MODIFIED").CurrentValue = DateTime.Now;
                }
                else
                    entry.State = EntityState.Unchanged;
            }
            else
            {
                entry.State = EntityState.Added;
                entry.Property("LAST_MODIFIED").CurrentValue = DateTime.Now;
            }
        }
    }
}
