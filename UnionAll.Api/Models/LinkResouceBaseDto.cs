using System.Collections.Generic;

namespace UnionAll.Api.Models
{
    public abstract class LinkResouceBaseDto
    {
        public List<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
}
