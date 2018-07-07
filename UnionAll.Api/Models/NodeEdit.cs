using System.ComponentModel.DataAnnotations;
using UnionAll.Domain;

namespace UnionAll.Api.Models
{
    // abstract base class cannot be used on its own.
    // virtual properties can be overriden in inheriting class.
    public abstract class NodeEdit
    {
        [Required(ErrorMessage = "Name is mandatory")]
        [MaxLength(256, ErrorMessage = "Name must be 256 characters or less")]
        [MinLength(3, ErrorMessage = "Name must contain at least 3 characters")]
        public string NodeName { get; set; }

        [Required(ErrorMessage = "NodeType is mandatory")]
        [EnumDataType(typeof(NodeValueTypes))]
        public virtual string NodeType { get; set; }

        [Required(ErrorMessage = "NodeTopic is mandatory")]
        [EnumDataType(typeof(NodeTopics))]
        public virtual string NodeTopic { get; set; }
    }
}
