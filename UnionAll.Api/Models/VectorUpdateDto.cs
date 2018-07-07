namespace UnionAll.Api.Models
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
