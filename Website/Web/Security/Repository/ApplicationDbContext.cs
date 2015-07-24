namespace Home.Security.Repository
{
    using System.Data.Entity;

    public class ApplicationDbContext : DbContext
    {
        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public ApplicationDbContext()
            : base("Name=DefaultConnection")
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            var entity = builder.Entity<ApplicationUser>();

            entity.ToTable("t_security_user");

            entity.HasKey(m => m.Id)
                  .Property(m => m.Id)
                  .HasColumnName("id")
                  .IsRequired();

            entity.Property(m => m.Key)
                  .HasColumnName("key")
                  .IsRequired()
                  .HasMaxLength(1024)
                  .IsUnicode(true)
                  .IsVariableLength();

            entity.Property(m => m.Name)
                  .HasColumnName("name")
                  .IsRequired()
                  .HasMaxLength(1024)
                  .IsUnicode(true)
                  .IsVariableLength();

            entity.Property(m => m.IdentityProvider)
                  .HasColumnName("ip")
                  .IsRequired();

            entity.Property(m => m.Role)
                  .HasColumnName("role")
                  .IsRequired();

            entity.Property(m => m.IsLocked)
                  .HasColumnName("is_locked")
                  .IsRequired();

            entity.Property(m => m.LockTimeUtc)
                  .HasColumnName("lock_time_utc")
                  .IsOptional();

            entity.Property(m => m.Created)
                  .HasColumnName("created")
                  .IsRequired();

            entity.Property(m => m.Modified)
                  .HasColumnName("modified")
                  .IsRequired();

            entity.Property(m => m.RowVersion)
                  .HasColumnName("row_version")
                  .IsRowVersion();
        }
    }
}
