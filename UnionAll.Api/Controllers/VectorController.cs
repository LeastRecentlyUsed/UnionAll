using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using AutoMapper;
using DataFork.DataStore.Services;
using DataFork.Domain;
using DataFork.API.Logging;
using DataFork.API.Models;
using DataFork.API.ModelValidations;
using DataFork.API.ModelBinders;

namespace DataFork.API.Controllers
{
    public class VectorController : Controller
    {
        private IVectorRepository _vectorRepo;
        private ILogger<VectorController> _logger;
        private IUrlHelper _urlHelper;

        public VectorController(IVectorRepository repo, ILogger<VectorController> logger, IUrlHelper urlHelper)
        {
            _vectorRepo = repo;
            _logger = logger;
            _urlHelper = urlHelper;
        }


        /// <summary>
        /// Query the DataStore for a given vector ID of a node subject and return the single item.
        /// </summary>
        /// <param name="nodeId">identifier of the node for the singel vector being requested</param>
        /// <param name="vectorId">identifier of the specific vector being requested</param>
        /// <returns>200 ok and the vector DTO if success</returns>
        [HttpGet("fork/node/{nodeId}/vector/{vectorId}", Name="GetVector")]
        public async Task<IActionResult> GetVector(int nodeId, int vectorId,
                        [FromHeader(Name = "Accept")] string mediaType)
        {
            var domainVector = await _vectorRepo.SelectVectorAsync(nodeId, vectorId);

            if (domainVector == null)
                return NotFound();

            if (mediaType == "application/vnd.fork.v1+json")
            {
                var providedVector = Mapper.Map<VectorWithLinksDto>(domainVector);
                return Ok(CreateLinksForVector(providedVector));
            }
            else
            {
                var providedVector = Mapper.Map<VectorDto>(domainVector);
                return Ok(providedVector);
            }
        }


        /// <summary>
        /// Query the DataStore for all vectors of a given node subject and return the collection
        /// </summary>
        /// <param name="id">identifier of the specific node of the required vectors</param>
        /// <returns>200 ok and the collection of vector DTOs if success</returns>
        [HttpGet("fork/node/{nodeId}/vectors", Name="GetVectorsForNode")]
        public async Task<IActionResult> GetVectorsForNode(int nodeId, 
                        DataRequestParams reqParams,
                        [FromHeader(Name = "Accept")] string mediaType)
        {
            var nodeVectors = await _vectorRepo.SelectVectorsByNodeAsync(nodeId, reqParams);

            if (nodeVectors == null)
                return NotFound();

            if (mediaType == "application/vnd.fork.v1+json")
            {
                var providedVectors = Mapper.Map<IEnumerable<VectorWithLinksDto>>(nodeVectors);

                // add the HATEOAS links for each vectorDTO
                providedVectors = providedVectors.Select(vector =>
                {
                    vector = CreateLinksForVector(vector);
                    return vector;
                });

                var wrapper = new Models.LinkedCollectionWrapperDto<VectorWithLinksDto>(providedVectors);
                return Ok(CreateLinksForNodeVectorList(wrapper, reqParams));
            }
            else
            {
                // add the pagination links
                Response.Headers.Add("LinkCurrent", CreateNavLink("GetVectorsForNode", reqParams, UrlNavigationType.None));
                Response.Headers.Add("LinkPrevious", CreateNavLink("GetVectorsForNode", reqParams, UrlNavigationType.Previous));
                Response.Headers.Add("LinkNext", CreateNavLink("GetVectorsForNode", reqParams, UrlNavigationType.Next));

                var providedVectors = Mapper.Map<IEnumerable<VectorDto>>(nodeVectors);

                return Ok(providedVectors);
            }
        }


        /// <summary>
        /// Query the DataStore for a collection of comma seperated vector IDs (for a node)
        /// </summary>
        /// <param name="id">a string list of Node Ids such as '(1,2,3,4)' mapped to an int array</param>
        /// <returns>200 ok and the set of vectors if success</returns>
        [HttpGet("fork/node/{nodeId}/vectors/({ids})", Name = "GetVectorSetForNode")]
        public async Task<IActionResult> GetVectorSetForNode(int nodeId,
                        DataRequestParams reqParams,
                        [ModelBinder(BinderType = typeof(StringListToIntArray))] IEnumerable<int> ids,
                        [FromHeader(Name = "Accept")] string mediaType)
        {
            if (ids == null)
                return BadRequest();

            var vectorSet = await _vectorRepo.SelectVectorSetAsync(ids);

            if (vectorSet == null)
                return NotFound();

            if (mediaType == "application/vnd.fork.v1+json")
            {
                var providedVectors = Mapper.Map<IEnumerable<VectorWithLinksDto>>(vectorSet);

                // add the HATEOAS links for each vectorDTO
                providedVectors = providedVectors.Select(vector =>
                {
                    vector = CreateLinksForVector(vector);
                    return vector;
                });

                var wrapper = new LinkedCollectionWrapperDto<VectorWithLinksDto>(providedVectors);
                return Ok(CreateLinksForNodeVectorList(wrapper, reqParams));
            }
            else
            {
                var providedVectors = Mapper.Map<IEnumerable<VectorDto>>(vectorSet);

                return Ok(providedVectors);
            }
        }


        /// <summary>
        /// Create a new vector in the DataStore.
        /// </summary>
        /// <param name="requestVector">the vector mapped from the request body to the DTO</param>
        /// <returns>201 created response if success</returns>
        [HttpPost("fork/node/{nodeId}/vector", Name="CreateVector")]
        public async Task<IActionResult> PostVector(int nodeId,
                        [FromBody] Models.VectorCreateDto requestVector,
                        [FromHeader(Name = "Accept")] string mediaType)
        {
            if (requestVector == null)
                return BadRequest();

            // if the model state is invalid return a 422 unprocessable entity return status.
            if (!ModelState.IsValid)
                return new UnprocessableModel(ModelState);

            var domainVector = Mapper.Map<Vector>(requestVector);

            domainVector.NodeSubject = nodeId;
            var savedVector = await _vectorRepo.InsertVectorAsync(domainVector);

            if (savedVector == null)
                throw new Exception($"Failed to save new Vector to DataStore.");

            _logger.LogInformation(LogEvents.NewVector, "CREATED vector {a}.", savedVector.VectorId);

            if (mediaType == "application/vnd.fork.v1+json")
            {
                var providedVector = Mapper.Map<VectorWithLinksDto>(savedVector);
                // return a 201 created response (and the new resource location URI).
                return CreatedAtRoute("GetVector", 
                    new { nodeId, vectorId = providedVector.VectorId }, 
                    CreateLinksForVector(providedVector)
                );
            }
            else
            {
                var providedVector = Mapper.Map<VectorDto>(savedVector);
                // return a 201 created response (and the new resource location URI).
                return CreatedAtRoute("GetVector", 
                    new { nodeId, vectorId = providedVector.VectorId }, providedVector);
            }
        }


        /// <summary>
        /// Create a new set of vectors in the DataStore.
        /// </summary>
        /// <param name="requestVectorSet">the vector set mapped from the request body to the dto class</param>
        /// <returns>201 created if success</returns>
        [HttpPost("fork/node/{nodeId}/vectors", Name = "CreateVectorSet")]
        public async Task<IActionResult> PostVectorSet(int nodeId,
                        DataRequestParams reqParams,
                        [FromBody] IEnumerable<Models.VectorCreateDto> requestVectorSet,
                        [FromHeader(Name = "Accept")] string mediaType)
        {
            if (requestVectorSet == null)
                return BadRequest();

            var domainVectorSet = Mapper.Map<IEnumerable<Vector>>(requestVectorSet);
            domainVectorSet.Select(v => { v.NodeSubject = nodeId; return v; }).ToList();

            var savedVectorSet = await _vectorRepo.InsertVectorSetAsync(domainVectorSet);

            if (savedVectorSet == null)
                throw new Exception("Failed to save VectorSet to DataStore.");

            // create the list of seperated node IDs to construct the resource URI.
            var idList = string.Join(",", savedVectorSet.Select(a => a.VectorId));

            _logger.LogInformation(LogEvents.ModifiedVector, "CREATED vector set {a}.", idList);

            if (mediaType == "application/vnd.fork.v1+json")
            {
                var providedVectoreSet = Mapper.Map<IEnumerable<VectorWithLinksDto>>(savedVectorSet);

                // add the HATEOAS links for each node list DTO and a link for the entire wrapper.
                providedVectoreSet = providedVectoreSet.Select(vector =>
                {
                    vector = CreateLinksForVector(vector);
                    return vector;
                });

                var wrapper = new LinkedCollectionWrapperDto<VectorWithLinksDto>(providedVectoreSet);
                return Ok(CreateLinksForNodeVectorList(wrapper, reqParams));
            }
            else
            {
                var providedNodeSet = Mapper.Map<IEnumerable<VectorDto>>(savedVectorSet);

                return CreatedAtRoute("GetVectorSetForNode", 
                    new { nodeId, ids = idList }, providedNodeSet);
            }
        }


        /// <summary>
        /// It is not allowable for a client to request a vector creation with a specific ID.
        /// </summary>
        /// <param name="id">the vector id for attempted creation</param>
        /// <returns>a 400 response code</returns>
        [HttpPost("fork/node/{nodeId}/vector/{vectorId}")]
        public async Task<IActionResult> PostVector(int nodeId, int vectorId)
        {
            var domainVector = await _vectorRepo.SelectVectorAsync(nodeId, vectorId);

            if (domainVector != null)
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            else
                return NotFound();
        }


        /// <summary>
        /// Delete a vector from the data store.
        /// </summary>
        /// <param name="id">the vector id for deletion</param>
        /// <returns>204 no content if success</returns>
        [HttpDelete("fork/node/{nodeId}/vector/{vectorId}", Name="DeleteVector")]
        public async Task<IActionResult> DeleteVector(int nodeId, int vectorId)
        {
            var domainVector = await _vectorRepo.SelectVectorAsync(nodeId, vectorId);

            if (domainVector == null)
                return BadRequest();

            domainVector.VectorStatus = VectorStatusValues.Deleted;
            domainVector.HasEdits = true;
            var deletedVector = await _vectorRepo.DeleteVectorAsync(domainVector);

            if (deletedVector == null)
                throw new Exception($"Failed to delete Vector {vectorId} from DataStore.");

            _logger.LogInformation(LogEvents.RemoveVector, "DELETED vector {a} for node {b}", vectorId, nodeId);

            return NoContent();
        }


        /// <summary>
        /// Allow a full update to a vector object on allowed fields.  Note, a PUT is a complete 
        /// replace of all allowed editable values (even to nulls).
        /// </summary>
        /// <param name="id">the vector id for the full update</param>
        /// <returns>204 no content if success</returns>
        [HttpPut("fork/node/{nodeId}/vector/{vectorId}", Name="UpdateVector")]
        public async Task<IActionResult> PutVector(int nodeId, int vectorId, 
                        [FromBody] VectorUpdateDto requestVector)
        {
            if (requestVector == null)
                return BadRequest();

            // if the model state is invalid return a 422 unprocessable entity return status.
            if (!ModelState.IsValid)
                return new UnprocessableModel(ModelState);

            var domainVector = await _vectorRepo.SelectVectorAsync(nodeId, vectorId);

            if (domainVector == null)
                return NotFound();

            Mapper.Map(requestVector, domainVector);
            domainVector.NodeSubject = nodeId;
            domainVector.HasEdits = true;

            var updatedVector = await _vectorRepo.UpdateVectorAsync(domainVector);

            if (updatedVector == null)
                throw new Exception($"Failed to update (put) Vector {vectorId} for Node {nodeId} in DataStore.");

            _logger.LogInformation(LogEvents.ModifiedVector, "UPDATED full vector {a} for node {b}.", vectorId, nodeId);

            return NoContent();
        }


        /// <summary>
        /// a partial update is a patch request, this follows the json patch standard RFC 6902
        /// at https://tools.ietf.org/html/rfc6902. request media type should be application/json-patch+json
        /// </summary>
        /// <param name="id">the vector id for the partial update</param>
        /// <returns>204 no content if success</returns>
        [HttpPatch("fork/node/{nodeId}/vector/{vectorId}", Name="PartiallyUpdateVector")]
        public async Task<IActionResult> PatchVector(int nodeId, int vectorId, 
                        [FromBody] JsonPatchDocument<VectorUpdateDto> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var domainVector = await _vectorRepo.SelectVectorAsync(nodeId, vectorId);

            if (domainVector == null)
                return NotFound();

            var patchVector = Mapper.Map<VectorUpdateDto>(domainVector);
            patchDoc.ApplyTo(patchVector);

            // validate the update DTO after applying the patch document to see if the DTO is valid.
            TryValidateModel(patchVector);

            if (!ModelState.IsValid)
                return new UnprocessableModel(ModelState);

            Mapper.Map(patchVector, domainVector);

            domainVector.NodeSubject = nodeId;
            domainVector.HasEdits = true;
            var updatedVector = await _vectorRepo.UpdateVectorAsync(domainVector);

            if (updatedVector == null)
                throw new Exception($"Failed to update (patch) Vector {vectorId} for Node {nodeId} in DataStore.");

            _logger.LogInformation(LogEvents.ModifiedVector, "UPDATED partial vector {a} for node {b}.", vectorId, nodeId);

            return NoContent();
        }


        /// <summary>
        /// Generate previous and next URI strings for HTTP request navigation within an action method
        /// </summary>
        /// <param name="action">the controller action method of the URL being created</param>
        /// <param name="reqParams">the page size and page number values</param>
        /// <param name="uriType">specifies if the required URI to be generated is previous or next</param>
        /// <returns></returns>
        private string CreateNavLink(string action, DataRequestParams reqParams, UrlNavigationType uriType)
        {
            string _url = string.Empty;

            switch (uriType)
            {
                case UrlNavigationType.Previous:
                    if (reqParams.PageNumber - 1 > 0)
                    {
                        _url = _urlHelper.Link(action,
                            new
                            {
                                pageNumber = reqParams.PageNumber - 1,
                                pageSize = reqParams.PageSize
                            });
                    }
                    break;
                case UrlNavigationType.Next:
                    _url = _urlHelper.Link(action,
                        new
                        {
                            pageNumber = reqParams.PageNumber + 1,
                            pageSize = reqParams.PageSize
                        });
                    break;
                default:
                    _url = _urlHelper.Link(action,
                        new
                        {
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
        /// <param name="vector">the existing vector for which links are being constructed</param>
        /// <returns>the existing node plus the allowable links</returns>
        private Models.VectorWithLinksDto CreateLinksForVector(Models.VectorWithLinksDto vector)
        {
            // create a link to the node itself
            vector.Links.Add(new Models.LinkDto(
                _urlHelper.Link("GetVector", new { nodeId = vector.NodeSubject, vectorId = vector.VectorId }),
                "self",
                "GET"
                ));
            // create a link to delete the node
            vector.Links.Add(new Models.LinkDto(
                _urlHelper.Link("DeleteVector", new { nodeId = vector.NodeSubject, vectorId = vector.VectorId }),
                "delete_vector",
                "DELETE"
                ));
            // create a link to update the entire node
            vector.Links.Add(new Models.LinkDto(
                _urlHelper.Link("UpdateVector", new { nodeId = vector.NodeSubject, vectorId = vector.VectorId }),
                "update_vector",
                "PUT"
                ));
            // create a link to update selective components of the node
            vector.Links.Add(new Models.LinkDto(
                _urlHelper.Link("PartiallyUpdateVector", new { nodeId = vector.NodeSubject, vectorId = vector.VectorId }),
                "partially_update_vector",
                "PATCH"
                ));

            return vector;
        }


        /// <summary>
        /// Create the HATEOAS links for allowable hyperlink actions via HTTP navigation
        /// </summary>
        /// <param name="vectorWrapper">the containing wrapper class that has a value and links section</param>
        /// <param name="reqParams">the pagination request parameters from the http header</param>
        /// <returns>the existing nodeList entry plus the allowable links</returns>
        private Models.LinkedCollectionWrapperDto<Models.VectorWithLinksDto> CreateLinksForNodeVectorList(
            Models.LinkedCollectionWrapperDto<Models.VectorWithLinksDto> vectorWrapper,
            DataRequestParams reqParams)
        {
            vectorWrapper.Links.Add(new Models.LinkDto(
                _urlHelper.Link("GetVectorsForNode", new {}),
                "self",
                "GET"
                ));
            // next page
            vectorWrapper.Links.Add(new Models.LinkDto(
                CreateNavLink("GetVectorsForNode", reqParams, UrlNavigationType.Next),
                "next_page",
                "GET"
                ));
            // previous page
            vectorWrapper.Links.Add(new Models.LinkDto(
                CreateNavLink("GetVectorsForNode", reqParams, UrlNavigationType.Previous),
                "previous_page",
                "GET"
                ));
            // current page
            vectorWrapper.Links.Add(new Models.LinkDto(
                CreateNavLink("GetVectorsForNode", reqParams, UrlNavigationType.None),
                "current_page",
                "GET"
                ));
            return vectorWrapper;
        }
        //
    }
}