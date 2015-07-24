namespace Home.Services.SaleService
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SaleDbContext : DbContext
    {
        static SaleDbContext()
        {
            Database.SetInitializer<SaleDbContext>(null);
        }

        public SaleDbContext()
            : base("Name=DefaultConnection")
        {
        }

        public DbSet<SaleHeader> SaleHeaders { get; set; }

        public DbSet<SaleLineItem> SaleLineItems { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            {
                var entity = builder.CreateEntity<SaleHeader>("tc_sale_header");

                entity.Property(m => m.NumberText)
                      .HasColumnName("sale_number")
                      .IsRequired()
                      .HasMaxLength(64)
                      .IsUnicode(false)
                      .IsVariableLength();

                entity.Property(m => m.DayNumber)
                      .HasColumnName("sale_day_number")
                      .IsRequired();

                entity.Property(m => m.TotalNumber)
                      .HasColumnName("sale_total_number")
                      .IsRequired();

                entity.Property(m => m.SalesPersonName)
                      .HasColumnName("sales_person_name")
                      .IsRequired()
                      .HasMaxLength(64)
                      .IsUnicode(true)
                      .IsVariableLength();

                entity.Property(m => m.CustomerName)
                      .HasColumnName("customer_name")
                      .IsRequired()
                      .HasMaxLength(64)
                      .IsUnicode(true)
                      .IsVariableLength();

                entity.Property(m => m.Status)
                      .HasColumnName("status")
                      .IsRequired();

                entity.HasMany(m => m.Items)
                      .WithRequired(m => m._SaleHeader)
                      .HasForeignKey(m => m._SaleHeaderId)
                      .WillCascadeOnDelete();

                entity.HasMany(m => m.CustomerContacts)
                      .WithRequired(m => m._SaleHeader)
                      .HasForeignKey(m => m._SaleHeaderId)
                      .WillCascadeOnDelete();
            }

            {
                var entity = builder.CreateEntity<SaleLineItem>("tc_sale_line");

                entity.Property(m => m.ProductName)
                      .HasColumnName("product_name")
                      .IsRequired()
                      .HasMaxLength(64)
                      .IsUnicode(true)
                      .IsVariableLength();

                entity.Property(m => m.ProductDescription)
                      .HasColumnName("product_description")
                      .IsOptional()
                      .HasMaxLength(1024)
                      .IsUnicode(true)
                      .IsVariableLength();

                entity.Property(m => m.Quantity)
                      .HasColumnName("quantity")
                      .IsRequired();

                entity.Property(m => m.UnitPrice)
                      .HasColumnName("unit_price")
                      .IsRequired();

                entity.Property(m => m.Status)
                      .HasColumnName("status")
                      .IsRequired();

                entity.Property(m => m._DynamicProperties)
                      .HasColumnName("product_detail")
                      .IsOptional()
                      .HasColumnType("XML");

                entity.Property(m => m._SaleHeaderId)
                      .HasColumnName("sale_id")
                      .IsRequired();

                entity.HasRequired(m => m._SaleHeader)
                      .WithMany(m => m.Items)
                      .HasForeignKey(m => m._SaleHeaderId);
            }

            {
                var entity = builder.CreateEntity<Contact>("tc_customer_contact");

                entity.Property(m => m.Method)
                      .HasColumnName("method")
                      .IsRequired();

                entity.Property(m => m.Value)
                      .HasColumnName("value")
                      .IsRequired()
                      .HasMaxLength(1024)
                      .IsUnicode(true)
                      .IsVariableLength();

                entity.Property(m => m._SaleHeaderId)
                      .HasColumnName("sale_id")
                      .IsRequired();

                entity.HasRequired(m => m._SaleHeader)
                      .WithMany(m => m.CustomerContacts)
                      .HasForeignKey(m => m._SaleHeaderId);
            }
        }
    }
}
