using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace UnionAll.Api.Controllers
{
    public class RootController : Controller
    {
        private IUrlHelper _urlHelper;

        public RootController(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        [HttpGet("/", Name = "GetRoot")]
        public IActionResult GetRoot([FromHeader(Name = "Accept")] string mediaType)
        {
            if (mediaType == "application/vnd.fork.v1+json")
            {
                var links = new List<Models.LinkDto>();

                links.Add(new Models.LinkDto(
                    _urlHelper.Link("GetRoot", new { }),
                    "self",
                    "GET"
                    ));
                links.Add(new Models.LinkDto(
                    _urlHelper.Link("GetNodePairs", new { }),
                    "node_pairs",
                    "GET"
                    ));
                links.Add(new Models.LinkDto(
                    _urlHelper.Link("GetNodeList", new { }),
                    "node_list",
                    "GET"
                    ));
                links.Add(new Models.LinkDto(
                    _urlHelper.Link("CreateNode", new { }),
                    "create_node",
                    "GET"
                    ));

                return Ok(links);
            }

            return NoContent();
        }
    }
}