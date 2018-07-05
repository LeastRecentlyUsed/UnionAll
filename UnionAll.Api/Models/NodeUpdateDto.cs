using System.ComponentModel.DataAnnotations;
using DataFork.Domain;
using DataFork.API.ModelValidations;

namespace DataFork.API.Models
{
    public class NodeUpdateDto: NodeEdit
    {
        [NodeEnumNotDefault]
        public override string NodeType
        {
            get => base.NodeType;
            set => base.NodeType = value;
        }

        [NodeEnumNotDefault]
        public override string NodeTopic
        {
            get => base.NodeTopic;
            set => base.NodeTopic = value;
        }
    }
}
