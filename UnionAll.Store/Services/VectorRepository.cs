using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataFork.Domain;

namespace DataFork.DataStore.Services
{
    public class VectorRepository: IVectorRepository
    {
        private DataContext _ctx;

        /// <summary>
        /// Accept the database context from the calling application and set the change tracking to function in a
        /// disconnected mode.  As this will be used as an API endpoint, REST will be used in place of change tracking.
        /// </summary>
        /// <param name="context">the EF Core DbContext that is generated within the calling application</param>
        public VectorRepository(DataContext context)
        {
            _ctx = context;
            _ctx.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        /// <summary>
        /// Select a single vector from the Database.
        /// </summary>
        /// <param name="vectorId">the required ID of the vector</param>
        /// <returns>the requested vector record or null</returns>
        public async Task<Vector> SelectVectorAsync(int nodeId, int vectorId)
        {
            return await (
                from v in _ctx.Vectors
                where v.VectorStatus == VectorStatusValues.Active
                where v.NodeSubject == nodeId
                where v.VectorId == vectorId
                select v
            )
            .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Insert a single Vector into the database.
        /// <summary>
        /// <param name="vector">The Vector Object being stored.</param>
        /// <returns>the inserted vector record if success</returns>
        public async Task<Vector> InsertVectorAsync(Vector vector)
        {
            _ctx.ChangeTracker.TrackGraph(vector, v => DataTracking.ApplyEditState(v.Entry));
            if (await _ctx.SaveChangesAsync() > 0)
                return vector;
            else
                return null;
        }

        /// <summary>
        /// Retrieve a collection of vectors from the database
        /// </summary>
        /// <param name="idSet">a key list of the requested vector rows</param>
        /// <returns>the requested set of vector objects</returns>
        public async Task<IEnumerable<Vector>> SelectVectorSetAsync(IEnumerable<int> idSet)
        {
            return await (
                from v in _ctx.Vectors
                where v.VectorStatus == VectorStatusValues.Active
                where idSet.Contains(v.VectorId)
                orderby (v.VectorId)
                select v
            )
            .ToListAsync();
        }

        /// <summary>
        /// Store a collection of vectors into the database.
        /// </summary>
        /// <param name="vectorSet">The IEnumerable vector collection to be stored.</param>
        /// <returns>the saved set of vector objects</returns>
        public async Task<IEnumerable<Vector>> InsertVectorSetAsync(IEnumerable<Vector> vectorSet)
        {
            foreach (Vector v in vectorSet)
            {
                _ctx.ChangeTracker.TrackGraph(v, e => DataTracking.ApplyEditState(e.Entry));
            }

            if (await _ctx.SaveChangesAsync() == vectorSet.Count())
                return vectorSet;
            else
                return null;
        }

        /// <summary>
        /// Delete a vector from the DataStore by setting the status to deleted.  Do not actually
        /// physically delete the database record. 
        /// </summary>
        /// <param name="vector">The Vector record to be deleted</param>
        /// <returns>The deleted Vector object</returns>
        public async Task<Vector> DeleteVectorAsync(Vector vector)
        {
            _ctx.ChangeTracker.TrackGraph(vector, e => DataTracking.ApplyEditState(e.Entry));
            if (await _ctx.SaveChangesAsync() > 0)
                return vector;
            else
                return null;
        }

        /// <summary>
        /// Update the Vector in the database to reflect the edits that were made.
        /// </summary>
        /// <param name="vector">The edited vector record</param>
        /// <returns>The updated vector object</returns>
        public async Task<Vector> UpdateVectorAsync(Vector vector)
        {
            _ctx.ChangeTracker.TrackGraph(vector, e => DataTracking.ApplyEditState(e.Entry));
            if (await _ctx.SaveChangesAsync() > 0)
                return vector;
            else
                return null;
        }

        /// <summary>
        /// Retrieve all vectors from the database for a given node.
        /// <summary>
        /// <param name="nodeId">The ID of the embedded Subject Node.</param>
        /// <returns>The found vector object collection</returns>
        public async Task<IEnumerable<Vector>> SelectVectorsByNodeAsync(
                        int nodeId, DataRequestParams reqParams)
        {
            var vectorSet = await (
                from vc in _ctx.Vectors
                where vc.VectorStatus == VectorStatusValues.Active
                where vc.NodeSubject == nodeId
                orderby vc.VectorId
                select vc
            )
            .Skip(reqParams.PageSize * (reqParams.PageNumber - 1))
            .Take(reqParams.PageSize)
            .ToListAsync();

            if (vectorSet.Count() > 0)
                return vectorSet;
            else
                return null;
        }
        //
    }
}
