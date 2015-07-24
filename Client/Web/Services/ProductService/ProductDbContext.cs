namespace Home.Services.ProductService
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration;
    using System.Linq;
    using System.Web;

    public class ProductDbContext : DbContext
    {
        static ProductDbContext()
        {
            Database.SetInitializer<ProductDbContext>(null);
        }

        public ProductDbContext()
            : base("Name=DefaultConnection")
        {
        }

        public DbSet<ProductDiamondImport> DiamondImports { get; set; }

        public DbSet<ProductDiamond> Diamonds { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            InitializeImport<ProductDiamondImport, ProductDiamond>(builder, "tc_product_diamond_import");

            {
                var entity = builder.CreateEntity<ProductDiamond>("tc_product_diamond");
                InitializeProduct<ProductDiamondImport, ProductDiamond>(entity);

                entity.HasRequired(m => m._Import)
                      .WithMany(m => m.Products)
                      .HasForeignKey(m => m._ImportId);

                entity.Property(m => m.ReportType)
                      .HasColumnName("report_type")
                      .IsOptional();

                entity.Property(m => m.ReportNumber)
                      .HasColumnName("report_number")
                      .IsOptional()
                      .HasMaxLength(128)
                      .IsUnicode(true)
                      .IsVariableLength();

                entity.Property(m => m.Caret)
                      .HasColumnName("caret")
                      .IsOptional();

                entity.Property(m => m.Clarity)
                      .HasColumnName("clarity")
                      .IsOptional();

                entity.Property(m => m.Color)
                      .HasColumnName("color")
                      .IsOptional();

                entity.Property(m => m.Cut)
                      .HasColumnName("cut")
                      .IsOptional();

                entity.Property(m => m.Polish)
                      .HasColumnName("polish")
                      .IsOptional();

                entity.Property(m => m.Symmetry)
                      .HasColumnName("symmetry")
                      .IsOptional();

                entity.Property(m => m.Fluorescence)
                      .HasColumnName("fluorescence")
                      .IsOptional()
                      .HasMaxLength(128)
                      .IsUnicode(true)
                      .IsVariableLength();

                entity.Property(m => m.Comment)
                      .HasColumnName("comment")
                      .IsOptional()
                      .HasMaxLength(512)
                      .IsUnicode(true)
                      .IsVariableLength();
            }
        }

        private static void InitializeImport<TImport, TProduct>(DbModelBuilder builder, string tableName)
            where TImport : ProductImport<TImport, TProduct>
            where TProduct : Product<TImport, TProduct>
        {
            var entity = builder.CreateEntity<ProductDiamondImport>(tableName);

            entity.Property(m => m.Count)
                  .HasColumnName("count")
                  .IsRequired();

            entity.HasMany(m => m.Products)
                  .WithRequired(m => m._Import)
                  .HasForeignKey(m => m._ImportId)
                  .WillCascadeOnDelete();
        }

        private static void InitializeProduct<TImport, TProduct>(EntityTypeConfiguration<TProduct> entity)
            where TImport : ProductImport<TImport, TProduct>
            where TProduct : Product<TImport, TProduct>
        {
            entity.Property(m => m.Index)
                  .HasColumnName("index")
                  .IsRequired();

            entity.Property(m => m.Status)
                  .HasColumnName("status")
                  .IsRequired();

            entity.Property(m => m.Cost)
                  .HasColumnName("cost")
                  .IsRequired();

            entity.Property(m => m.SalePrice)
                  .HasColumnName("sale_price")
                  .IsRequired();

            entity.Property(m => m.Vendor)
                  .HasColumnName("vendor")
                  .IsOptional()
                  .HasMaxLength(64)
                  .IsUnicode(true)
                  .IsVariableLength();

            entity.Property(m => m._ImportId)
                  .HasColumnName("import_id")
                  .IsRequired();
        }
    }
}