using UnionAll.Api.ModelValidations;

namespace UnionAll.Api.Models
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
