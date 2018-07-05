using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataFork.API.Models
{
    public class LinkedCollectionWrapperDto<T>: LinkResouceBaseDto
        where T : LinkResouceBaseDto
    {
        public IEnumerable<T> Value { get; set; }

        public LinkedCollectionWrapperDto(IEnumerable<T> value)
        {
            Value = value;
        }
    }
}
