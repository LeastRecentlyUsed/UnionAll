using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataFork.API.Models
{
    public class NodeDto
    {
        public int NodeId { get; set; }

        public string NodeName { get; set; }

        public string NodeType { get; set; }

        public string NodeTopic { get; set; }
    }
}
