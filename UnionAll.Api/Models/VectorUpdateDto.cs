using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataFork.API.Models
{
    public class VectorUpdateDto
    {
        public string VectorPhrase { get; set; }

        //public int NodeSubject { get; set; }

        public int NodeObject { get; set; }

        public int NodeParent { get; set; }

        public int NodeRoot { get; set; }
    }
}
