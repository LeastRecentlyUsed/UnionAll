using System.Collections.Generic;
using System.Threading.Tasks;
using UnionAll.Domain;

namespace UnionAll.Store.Services
{
    public interface IVectorRepository
    {
        // return a single active Vector record given a Vector ID.
        Task<Vector> SelectVectorAsync(int nodeId, int vectorId);

        // insert a new Vector object into the database.
        Task<Vector> InsertVectorAsync(Vector vector);

        // Mark a Vector record in the database as deleted.
        Task<Vector> DeleteVectorAsync(Vector vector);

        // Update a Vector record in the database.
        Task<Vector> UpdateVectorAsync(Vector vector);

        // select a requested collection of active Vector records from the database.
        Task<IEnumerable<Vector>> SelectVectorSetAsync(IEnumerable<int> idSet);

        // insert a collection of Vector objects into the database.
        Task<IEnumerable<Vector>> InsertVectorSetAsync(IEnumerable<Vector> vectorSet);

        // select a requested collection of active Vector records belonging to a specific Node.
        Task<IEnumerable<Vector>> SelectVectorsByNodeAsync(int id, DataRequestParams reqParams);
    }
}
