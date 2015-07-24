namespace Home.Services.SaleService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SaleLineItem : OpenEntity
    {
        [Required]
        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public SaleStatus Status { get; set; }

        public Guid _SaleHeaderId { get; set; }

        public virtual SaleHeader _SaleHeader { get; set; }
    }
}
