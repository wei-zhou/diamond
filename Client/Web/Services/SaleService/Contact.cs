namespace Home.Services.SaleService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Contact : Entity
    {
        [Required]
        public ContactMethod Method { get; set; }

        [Required]
        public string Value { get; set; }

        public Guid _SaleHeaderId { get; set; }

        public virtual SaleHeader _SaleHeader { get; set; }
    }
}
