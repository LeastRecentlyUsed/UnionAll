namespace UnionAll.Api.Models
{
    public class NodeWithLinksDto: LinkResouceBaseDto
    {
        public int NodeId { get; set; }

        public string NodeName { get; set; }

        public string NodeType { get; set; }

        public string NodeTopic { get; set; }
    }
}
