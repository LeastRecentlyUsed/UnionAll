using System;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace UnionAll.Domain
{
    public class Node: ChangeStatus
    {
        public int NodeId { get; private set; }

        private string _nodeName;

        [Required]
        [MaxLength(512)]
        public string NodeName
        {
            get { return _nodeName; }
            set
            {
                _nodeName = value;
                NodeMatchName = CleanName(value).ToLower();
            }
        }

        [Required]
        public NodeValueTypes NodeType { get; set; }

        [Required]
        public NodeTopics NodeTopic { get; set; }

        [Required]
        public NodesStatusValues NodeStatus { get; set; } = NodesStatusValues.Active;

        public string NodeMatchName { get; private set; }

        /// <summary>
        /// Used for Node Name comparisons within the data store.
        /// </summary>
        /// <param name="name">user generated node name</param>
        /// <returns>clean node name</returns>
        private static string CleanName(string name)
        {
            try
            {
                return Regex.Replace(name, @"[^\w\.@-]", "", RegexOptions.None, TimeSpan.FromSeconds(2));
            }
            catch (RegexMatchTimeoutException)
            {
                return name;
            }
        }
    }
}
