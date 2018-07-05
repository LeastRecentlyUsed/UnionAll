using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataFork.API.Models
{
    public abstract class LinkResouceBaseDto
    {
        public List<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
}
