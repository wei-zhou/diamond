namespace Home.Services.SaleService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.OData.Builder;

    public class SaleHeader : Entity
    {
        public string NumberText { get; set; }

        public int DayNumber { get; set; }

        public int TotalNumber { get; set; }

        [Required]
        public string SalesPersonName { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Contained]
        public virtual IList<Contact> CustomerContacts { get; set; }

        [Contained]
        public virtual IList<SaleLineItem> Items { get; set; }

        public SaleStatus Status { get; set; }
    }
}
