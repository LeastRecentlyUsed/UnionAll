using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using AutoMapper;
using UnionAll.Store.Services;
using UnionAll.Domain;
using UnionAll.Api.ModelValidations;
using UnionAll.Api.Logging;
using UnionAll.Api.Models;
using UnionAll.Api.ModelBinders;

namespace UnionAll.Api.Controllers
{
    public class NodeController : Controller
    {
        private INodeRepository _nodeRepo;
        private ILogger<NodeController> _logger;
        private IUrlHelper _urlHelper;

        /// <summary>
        /// Constructor to allow injection of datastore and logger functionality.
        /// </summary>
        /// <param name="repo">the injected Database Repository concrete class</param>
        /// <param name="logger">the injected Logging concrete class</param>
        /// <param name="urlHelper">the injected URL Helper concrete class</param>
        public NodeController(INodeRepository repo, ILogger<NodeController> logger, IUrlHelper urlHelper)
        {
            _nodeRepo = repo;
            _logger = logger;
            _urlHelper = urlHelper;
        }


        /// <summary>
        /// Query the DataStore for the list of all Node name & id pairs.
        /// </summary>
        /// <returns>a list of node name & identifier pairs</returns>
        [HttpGet("nodes/pairs", Name = "GetNodePairs")]
        public async Task<IActionResult> GetNodeList(DataRequestParams reqParams)
        {
            var keyValuePairs = await _nodeRepo.SelectNodeListAsync(reqParams);

            var providedNodes = Mapper.Map<IEnumerable<NodePairsDto>>(keyValuePairs);

            // add the pagination links
            string action = "GetNodePairs";
            Response.Headers.Add("LinkCurrent", CreateNavLink(action, reqParams, UrlNavigationType.None));
            Response.Headers.Add("LinkPrevious", CreateNavLink(action, reqParams, UrlNavigationType.Previous));
            Response.Headers.Add("LinkNext", CreateNavLink(action, reqParams, UrlNavigationType.Next));

            return Ok(providedNodes);
        }


        /// <summary>
        /// Query the DataStore for the list of all Nodes.
        /// </summary>
        /// <returns>a list of node identifiers</returns>
        [HttpGet("nodes", Name ="GetNodeList")]
        public async Task<IActionResult> GetNodeList(DataRequestParams reqParams, 
                         [FromHeader(Name="Accept")] string mediaType)
        {
            var domainNodes = await _nodeRepo.SelectAllNodesAsync(reqParams);

            if (mediaType == "application/unionall+json")
            {
                var providedNodes = Mapper.Map<IEnumerable<NodeWithLinksDto>>(domainNodes);

                // add the HATEOAS links for each node list DTO and a link for the entire wrapper.
                providedNodes = providedNodes.Select(node =>
                {
                    node = CreateLinksForNode(node);
                    return node;
                });

                var wrapper = new LinkedCollectionWrapperDto<NodeWithLinksDto>(providedNodes);
                return Ok(CreateLinksForNodeList(wrapper, reqParams));
            }
            else
            {
                // add the pagination links
                Response.Headers.Add("LinkCurrent", CreateNavLink("GetNodeList", reqParams, UrlNavigationType.None));
                Response.Headers.Add("LinkPrevious", CreateNavLink("GetNodeList", reqParams, UrlNavigationType.Previous));
                Response.Headers.Add("LinkNext", CreateNavLink("GetNodeList", reqParams, UrlNavigationType.Next));

                var providedNodes = Mapper.Map<IEnumerable<NodeDto>>(domainNodes);

                return Ok(providedNodes);
            }
        }


        /// <summary>
        /// Return a list of Node objects.
        /// </summary>
        /// <param name="ids">a string list of Node Ids such as '(1,2,3,4)' mapped to an int array</param>
        /// <returns>200 OK and the set of Nodes, if success</returns>
        [HttpGet("nodes/({ids})", Name = "GetNodeSet")]
        public async Task<IActionResult> GetNodeSet(DataRequestParams reqParams,
                        [ModelBinder(BinderType = typeof(StringListToIntArray))] IEnumerable<int> ids,
                        [FromHeader(Name="Accept")] string mediaType)
        {
            if (ids == null)
                return BadRequest();

            var domainNodeSet = await _nodeRepo.SelectNodeSetAsync(ids);

            // does the list of ids match the number of returned nodes?
            if (ids.Count() != domainNodeSet.Count())
                return NotFound();

            if (mediaType == "application/unionall+json")
            {
                var providedNodeSet = Mapper.Map<IEnumerable<NodeWithLinksDto>>(domainNodeSet);

                // add the HATEOAS links for each node list DTO and a link for the entire wrapper.
                providedNodeSet = providedNodeSet.Select(node =>
                {
                    node = CreateLinksForNode(node);
                    return node;
                });

                var wrapper = new LinkedCollectionWrapperDto<NodeWithLinksDto>(providedNodeSet);
                return Ok(CreateLinksForNodeList(wrapper, reqParams));
            }
            else
            {
                var providedNodeSet = Mapper.Map<IEnumerable<NodeDto>>(domainNodeSet);

                return Ok(providedNodeSet);
            }
        }


        /// <summary>
        /// Create a new set of nodes in the DataStore.
        /// </summary>
        /// <param name="requestNodeSet">the node set mapped from the request body to the dto class</param>
        /// <returns>201 created if success</returns>
        [HttpPost("nodes", Name = "CreateNodeSet")]
        public async Task<IActionResult> CreateNodeSet(DataRequestParams reqParams,
                        [FromBody] IEnumerable<NodeCreateDto> requestNodeSet,
                        [FromHeader(Name = "Accept")] string mediaType)
        {
            if (requestNodeSet == null)
                return BadRequest();

            var domainNodeSet = Mapper.Map<IEnumerable<Node>>(requestNodeSet);

            var savedNodeSet = await _nodeRepo.InsertNodeSetAsync(domainNodeSet);

            if (savedNodeSet == null)
                throw new Exception("Failed to save NodeSet to DataStore.");

            // create the list of seperated node IDs to construct the resource URI.
            var idList = string.Join(",", savedNodeSet.Select(a => a.NodeId));

            _logger.LogInformation(LogEvents.NewNodeSet, "CREATED Nodeset {a}.", idList);

            if (mediaType == "application/unionall+json")
            {
                var providedNodeSet = Mapper.Map<IEnumerable<NodeWithLinksDto>>(savedNodeSet);

                // add the HATEOAS links for each node list DTO and a link for the entire wrapper.
                providedNodeSet = providedNodeSet.Select(node =>
                {
                    node = CreateLinksForNode(node);
                    return node;
                });

                var wrapper = new LinkedCollectionWrapperDto<NodeWithLinksDto>(providedNodeSet);
                return Ok(CreateLinksForNodeList(wrapper, reqParams));
            }
            else
            {
                var providedNodeSet = Mapper.Map<IEnumerable<NodeDto>>(savedNodeSet);

                return CreatedAtRoute("GetNodeSet", new { ids = idList }, providedNodeSet);
            }
        }


        /// <summary>
        /// Query the DataStore for a given node ID and return the single item.
        /// </summary>
        /// <param name="id">identifier of the specific Node being requested</param>
        /// <returns>200 Ok and the node DTO if success</returns>
        [HttpGet("node/{id}", Name="GetNode")]
        public async Task<IActionResult> GetNode(int id,
                        [FromHeader(Name = "Accept")] string mediaType)
        {
            var domainNode = await _nodeRepo.SelectNodeAsync(id);

            if (domainNode == null)
                return NotFound();

            if (mediaType == "application/unionall+json")
            {
                //currenty AutoMapper converts enumerations to string if that is the target type in the DTO.
                var providedNode = Mapper.Map<NodeWithLinksDto>(domainNode);

                return Ok(CreateLinksForNode(providedNode));
            }
            else
            {
                var providedNode = Mapper.Map<NodeDto>(domainNode);

                return Ok(providedNode);
            }
        }


        /// <summary>
        /// Create a new node in the DataStore.
        /// </summary>
        /// <param name="requestNode">the node object mapped from the request body to the DTO</param>
        /// <returns>201 created response if success</returns>
        [HttpPost("node", Name="CreateNode")]
        public async Task<IActionResult> PostNode([FromBody] NodeCreateDto requestNode,
                        [FromHeader(Name = "Accept")] string mediaType)
        {
            // the incoming object could not be mapped to a Node DTO so 400 bad client request.
            if (requestNode == null)
                return BadRequest();

            // if the model state is invalid return a 422 unprocessable entity.
            if (!ModelState.IsValid)
                return new UnprocessableModel(ModelState);

            var domainNode = Mapper.Map<Node>(requestNode);
            Node savedNode = await _nodeRepo.InsertNodeAsync(domainNode);

            // if save fails (null return) we shoud return a 500 server error from Startup.cs
            if (savedNode == null)
                throw new Exception($"Failed to save new Node to DataStore.");

            _logger.LogInformation(LogEvents.NewNode, "CREATED node {a}.", savedNode.NodeId);

            if (mediaType == "application/unionall+json")
            {
                var providedNode = Mapper.Map<NodeWithLinksDto>(savedNode);

                // return a 201 created response (and the new resource location URI).
                return CreatedAtRoute("GetNode", new { id = providedNode.NodeId }, CreateLinksForNode(providedNode));
            }
            else
            {
                var providedNode = Mapper.Map<NodeDto>(savedNode);

                // return a 201 created response (and the new resource location URI).
                return CreatedAtRoute("GetNode", new { id = providedNode.NodeId }, providedNode);
            }
        }


        /// <summary>
        /// It is not allowable to request a node creation with a specific ID.  If it is attempted
        /// handle the request and return the correct HTTP response.
        /// </summary>
        /// <param name="id">the node id for attempted creation</param>
        /// <returns>a 400 response code</returns>
        [HttpPost("node/{id}")]
        public async Task<IActionResult> PostNode(int id)
        {
            var domainNode = await _nodeRepo.SelectNodeAsync(id);

            if (domainNode != null)
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }
            else
            {
                return NotFound();
            }
        }


        /// <summary>
        /// Delete a node from the data store.
        /// </summary>
        /// <param name="id">the node id for deletion</param>
        /// <returns>204 no content if success</returns>
        [HttpDelete("node/{id}", Name="DeleteNode")]
        public async Task<IActionResult> DeleteNode(int id)
        {
            var domainNode = await _nodeRepo.SelectNodeAsync(id);

            if (domainNode == null)
                return NotFound();

            domainNode.NodeStatus = NodesStatusValues.Deleted;
            domainNode.HasEdits = true;
            var deletedNode = await _nodeRepo.DeleteNodeAsync(domainNode);

            // if delete fails (null return) we shoud return a 500 server error from Startup.cs
            if (deletedNode == null)
                throw new Exception($"Failed to delete Node {id} from DataStore.");

            _logger.LogInformation(LogEvents.RemoveNode, "DELETED node {a}.", id);        
            // if deleted OK there was success but nothign to return (204 no content)
            return NoContent();
        }


        /// <summary>
        /// Allow a full update to a Node object on allowed fields.  Note, a PUT is a complete 
        /// replace of all allowed editable values (even to nulls).
        /// </summary>
        /// <param name="id">the node id for the full update</param>
        /// <returns>204 no content if success</returns>
        [HttpPut("node/{id}", Name="UpdateNode")]
        public async Task<IActionResult> PutNode(int id, 
                        [FromBody] Models.NodeUpdateDto requestNode)
        {
            if (requestNode == null)
                return BadRequest();

            // if the model state is invalid return a 422 unprocessable entity return status.
            if (!ModelState.IsValid)
                return new UnprocessableModel(ModelState);

            // get the node from the data store.
            var domainNode = await _nodeRepo.SelectNodeAsync(id);

            if (domainNode == null)
                return NotFound();

            // map the NodeUpdateDto from request into the retrieved Node from the data store.
            Mapper.Map(requestNode, domainNode);
            domainNode.HasEdits = true;

            // perform the data store update operation.
            var updatedNode = await _nodeRepo.UpdateNodeAsync(domainNode);

            if (updatedNode == null)
                throw new Exception($"Failed to update (put) Node {id} in DataStore.");

            _logger.LogInformation(LogEvents.ModifiedNode, "UPDATED full Node {a}.", id);

            // if success return no content as the consumer has not requested a node, just an updated.
            // if you prefer it is fine to map the updated node to a DTO and return Ok(NodeDTO).
            return NoContent();
        }


        /// <summary>
        /// a partial update is a patch request, this follows the json patch standard RFC 6902
        /// at https://tools.ietf.org/html/rfc6902. request media type should be application/json-patch+json
        /// </summary>
        /// <param name="id">the node id for the partial update</param>
        /// <returns>204 no content if success</returns>
        [HttpPatch("node/{id}", Name="PartiallyUpdateNode")]
        public async Task<IActionResult> PatchNode(int id, 
                        [FromBody] JsonPatchDocument<NodeUpdateDto> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            // get the node from the data store.
            var domainNode = await _nodeRepo.SelectNodeAsync(id);

            if (domainNode == null)
                return NotFound();

            // map the existing domain node to a node update DTO 
            var patchNode = Mapper.Map<NodeUpdateDto>(domainNode);

            // now apply the patch document to that DTO.  By passing the model state, any errors
            // in the patch document will invalidate the model state.
            patchDoc.ApplyTo(patchNode, ModelState);

            // validate the update DTO after applying the patch document to see if the DTO is valid.
            TryValidateModel(patchNode);

            if (!ModelState.IsValid)
                return new UnprocessableModel(ModelState);

            // map the patched DTO back to the previously retrieved data store node.
            Mapper.Map(patchNode, domainNode);

            // set update flag and apply to the data store.
            domainNode.HasEdits = true;
            var updatedNode = await _nodeRepo.UpdateNodeAsync(domainNode);

            if (updatedNode == null)
                throw new Exception($"Failed to update (patch) Node {id} in DataStore.");

            _logger.LogInformation(LogEvents.ModifiedNode, "UPDATED partial Node {a}.", id);

            return NoContent();
        }


        /// <summary>
        /// Generate previous and next URI strings for HTTP request navigation within an action method
        /// </summary>
        /// <param name="reqParams">the page size and page number values</param>
        /// <param name="urlType">specifies if the required URL to be generated is previous or next</param>
        /// <returns></returns>
        private string CreateNavLink(string action, DataRequestParams reqParams, UrlNavigationType urlType)
        {
            string _url = string.Empty;

            switch (urlType)
            {
                case UrlNavigationType.Previous:
                    if (reqParams.PageNumber - 1 > 0)
                    {
                        _url = _urlHelper.Link(action,
                            new
                            {
                                searchQuery = reqParams.SearchQuery,
                                pageNumber = reqParams.PageNumber - 1,
                                pageSize = reqParams.PageSize
                            });
                    }
                    break;
                case UrlNavigationType.Next:
                    _url = _urlHelper.Link(action,
                        new
                        {
                            searchQuery = reqParams.SearchQuery,
                            pageNumber = reqParams.PageNumber + 1,
                            pageSize = reqParams.PageSize
                        });
                    break;
                default:
                    _url = _urlHelper.Link(action,
                        new
                        {
                            searchQuery = reqParams.SearchQuery,
                            pageNumber = reqParams.PageNumber,
                            pageSize = reqParams.PageSize
                        });
                    break;
            }
            //return Newtonsoft.Json.JsonConvert.SerializeObject(_url);
            return _url;
        }


        /// <summary>
        /// Create the HATEOAS links for allowable hyperlink actions via HTTP navigation
        /// </summary>
        /// <param name="node">the existing node for which links are being constructed</param>
        /// <returns>the existing node plus the allowable links</returns>
        private NodeWithLinksDto CreateLinksForNode(NodeWithLinksDto node)
        {
            // create a link to the node itself
            node.Links.Add(new LinkDto(
                _urlHelper.Link("GetNode", new { id = node.NodeId }), 
                "self",
                "GET"
                ));
            // create a link to delete the node
            node.Links.Add(new LinkDto(
                _urlHelper.Link("DeleteNode", new { id = node.NodeId }),
                "delete_node",
                "DELETE"
                ));
            // create a link to update the entire node
            node.Links.Add(new LinkDto(
                _urlHelper.Link("UpdateNode", new { id = node.NodeId }),
                "update_node",
                "PUT"
                ));
            // create a link to update selective components of the node
            node.Links.Add(new LinkDto(
                _urlHelper.Link("PartiallyUpdateNode", new { id = node.NodeId }),
                "partially_update_node",
                "PATCH"
                ));
            // create a link to the add a vector
            node.Links.Add(new LinkDto(
                _urlHelper.Link("CreateVector", new { nodeId = node.NodeId }),
                "create_vector",
                "POST"
                ));

            return node;
        }

        /// <summary>
        /// Create the HATEOAS links for the node collection being returned
        /// </summary>
        /// <param name="nodeList">the existing nodeList entry for which links are being constructed</param>
        /// <returns>the existing nodeList entry plus the allowable links</returns>
        private LinkedCollectionWrapperDto<NodeWithLinksDto> CreateLinksForNodeList(
            LinkedCollectionWrapperDto<NodeWithLinksDto> nodeWrapper,
            DataRequestParams reqParams)
        {
            nodeWrapper.Links.Add(new LinkDto(
                _urlHelper.Link("GetNodeList", new {}),
                "self",
                "GET"
                ));
            // next page
            nodeWrapper.Links.Add(new LinkDto(
                CreateNavLink("GetNodeList", reqParams, UrlNavigationType.Next),
                "next_page",
                "GET"
                ));
            // previous page
            nodeWrapper.Links.Add(new LinkDto(
                CreateNavLink("GetNodeList", reqParams, UrlNavigationType.Previous),
                "previous_page",
                "GET"
                ));
            // current page
            nodeWrapper.Links.Add(new LinkDto(
                CreateNavLink("GetNodeList", reqParams, UrlNavigationType.None),
                "current_page",
                "GET"
                ));
            return nodeWrapper;
        }
        //
     }
}