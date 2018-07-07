using System.Collections.Generic;

namespace UnionAll.Api.Models
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
