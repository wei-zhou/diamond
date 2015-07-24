namespace Home.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class Entity
    {
        [Key]
        public Guid Id { get; set; }

        public DateTimeOffset Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTimeOffset Modified { get; set; }

        public string ModifiedBy { get; set; }

        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }
    }
}
