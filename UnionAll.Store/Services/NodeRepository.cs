using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataFork.Domain;

namespace DataFork.DataStore.Services
{
    public class NodeRepository: INodeRepository
    {
        private DataContext _ctx;

        /// <summary>
        /// Accept the database context from the calling application and set the change tracking to function in a
        /// disconnected mode.  As this will be used as an API endpoint, REST will be used in place of change tracking.
        /// </summary>
        /// <param name="context">the EF Core DbContext that is generated within the calling application</param>
        public NodeRepository(DataContext context)
        {
            _ctx = context;
            _ctx.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        /// <summary>
        /// Return the id and name of the set of all Nodes in the database. 
        /// <summary>
        public async Task<IEnumerable<KeyValuePair<int, string>>> SelectNodeListAsync(DataRequestParams reqParams)
        {
            // create an IQueryable collection but defer execution.
            var nodeList = (
                from nl in _ctx.Nodes
                where nl.NodeStatus == NodesStatusValues.Active
                orderby nl.NodeName
                select new { nl.NodeId, nl.NodeName }
            );

            // apply any search query (note: ToLowerInvariant() will not execute in Sqlite).
            if (!string.IsNullOrEmpty(reqParams.SearchQuery))
            {
                // trim and ignore case
                var searchQueryForSelect = reqParams.SearchQuery.Trim().ToLower();
                // apply the search term
                nodeList = nodeList.Where(n => n.NodeName.ToLower().Contains(searchQueryForSelect));
            }

            // apply the deferred execution to the IQueryable collection and return a paged dictionary.
            return await nodeList
                .Skip(reqParams.PageSize * (reqParams.PageNumber - 1))
                .Take(reqParams.PageSize)
                .ToDictionaryAsync(k => k.NodeId, v => v.NodeName);
        }

        /// <summary>
        /// Return the collection of all Nodes in the database. 
        /// <summary>
        public async Task<IEnumerable<Node>> SelectAllNodesAsync(DataRequestParams reqParams)
        {
            // create an IQueryable collection but defer execution.
            var nodeList = (
                from nl in _ctx.Nodes
                where nl.NodeStatus == NodesStatusValues.Active
                orderby nl.NodeName
                select nl
            );

            // apply any search query (note: ToLowerInvariant() will not execute in Sqlite).
            if (!string.IsNullOrEmpty(reqParams.SearchQuery))
            {
                // trim and ignore case
                var searchQueryForSelect = reqParams.SearchQuery.Trim().ToLower();
                // apply the search term
                nodeList = (IOrderedQueryable<Node>)nodeList.Where(n => n.NodeName.ToLower().Contains(searchQueryForSelect));
            }

            // apply the deferred execution to the IQueryable collection and return a paged dictionary.
            return await nodeList
                .Skip(reqParams.PageSize * (reqParams.PageNumber - 1))
                .Take(reqParams.PageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieve a single node from the database.
        /// <summary>
        /// <param name="nodeId">The ID of the requested Node.</param>
        public async Task<Node> SelectNodeAsync(int nodeId)
        {
            return await (
                from n in _ctx.Nodes
                where n.NodeStatus == NodesStatusValues.Active
                where n.NodeId == nodeId
                select n
            )
            .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Store a single Node into the database.
        /// <summary>
        /// <param name="node">The Node Object being stored.</param>
        /// <returns>the saved node record</returns>
        public async Task<Node> InsertNodeAsync(Node node)
        {
            _ctx.ChangeTracker.TrackGraph(node, e => DataTracking.ApplyEditState(e.Entry));
            if (await _ctx.SaveChangesAsync() > 0)
                return node;
            else
                return null;
        }

        /// <summary>
        /// Retrieve a collection of nodes from the database
        /// </summary>
        /// <param name="idSet">a key list of the requested node rows</param>
        /// <returns>the requested set of node records</returns>
        public async Task<IEnumerable<Node>> SelectNodeSetAsync(IEnumerable<int> idSet)
        {
            var nodeSet = (
                from nodes in _ctx.Nodes
                where nodes.NodeStatus == NodesStatusValues.Active
                where idSet.Contains(nodes.NodeId)
                orderby (nodes.NodeId)
                select nodes
            );

            if (nodeSet.Count() == idSet.Count())
                return await nodeSet.ToListAsync();
            else
                return null;
        }

        /// <summary>
        /// Store a collection of Nodes into the database.
        /// </summary>
        /// <param name="nodeSet">The IEnumerable Node collection to be stored.</param>
        /// <returns>the saved set of node records</returns>
        public async Task<IEnumerable<Node>> InsertNodeSetAsync(IEnumerable<Node> nodeSet)
        {
            foreach (Node n in nodeSet)
            {
                _ctx.ChangeTracker.TrackGraph(n, e => DataTracking.ApplyEditState(e.Entry));
            }

            if (await _ctx.SaveChangesAsync() == nodeSet.Count())
                return nodeSet;
            else
                return null;
        }

        /// <summary>
        /// Delete a node from the DataStore by setting the status to deleted.  Do not actually
        /// physically delete the database record. 
        /// </summary>
        /// <param name="node">The Node record to be deleted</param>
        /// <returns>The deleted Node object</returns>
        public async Task<Node> DeleteNodeAsync(Node node)
        {
            _ctx.ChangeTracker.TrackGraph(node, e => DataTracking.ApplyEditState(e.Entry));
            if (await _ctx.SaveChangesAsync() > 0)
                return node;
            else
                return null;
        }

        /// <summary>
        /// Update the Node in the DataStore to reflect the edits that were made.
        /// </summary>
        /// <param name="node">The edited Node record</param>
        /// <returns>The updated Node object</returns>
        public async Task<Node> UpdateNodeAsync(Node node)
        {
            _ctx.ChangeTracker.TrackGraph(node, e => DataTracking.ApplyEditState(e.Entry));
            if (await _ctx.SaveChangesAsync() > 0)
                return node;
            else
                return null;
        }
    }
}
