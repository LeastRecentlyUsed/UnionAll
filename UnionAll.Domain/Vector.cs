using System.ComponentModel.DataAnnotations;

namespace UnionAll.Domain
{
    public class Vector: ChangeStatus
    {
        public int VectorId { get; private set; }

        [Required]
        public string VectorPhrase { get; set; }

        [Required]
        public int NodeSubject { get; set; }

        [Required]
        public int NodeObject { get; set; }

        [Required]
        public int NodeParent { get; set; }

        [Required]
        public int NodeRoot { get; set; }

        public VectorStatusValues VectorStatus { get; set; } = VectorStatusValues.Active;
    }
}
