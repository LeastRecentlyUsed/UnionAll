using System.Collections.Generic;
using System.Threading.Tasks;
using UnionAll.Domain;

namespace UnionAll.Store.Services
{
    public interface INodeRepository
    {
        // select all Node ID and NAME values of active records from the database (with query)
        Task<IEnumerable<KeyValuePair<int, string>>> SelectNodeListAsync(DataRequestParams reqParams);

        // select all Nodes from the database
        Task<IEnumerable<Node>> SelectAllNodesAsync(DataRequestParams reqParams);

        // return a single active Node record given a Node ID.
        Task<Node> SelectNodeAsync(int id);

        // insert a new Node object into the database.
        Task<Node> InsertNodeAsync(Node node);

        // Mark a Node record in the database as deleted.
        Task<Node> DeleteNodeAsync(Node node);

        // Update a Node record in the database.
        Task<Node> UpdateNodeAsync(Node node);

        // select a requested collection of active Node records from the database.
        Task<IEnumerable<Node>> SelectNodeSetAsync(IEnumerable<int> idSet);

        // insert a collection of Node objects into the database.
        Task<IEnumerable<Node>> InsertNodeSetAsync(IEnumerable<Node> nodeSet);
    }
}
