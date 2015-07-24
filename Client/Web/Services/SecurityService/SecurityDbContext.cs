namespace Home.Services.SecurityService
{
    using System.Data.Entity;

    public class SecurityDbContext : DbContext
    {
        static SecurityDbContext()
        {
            Database.SetInitializer<SecurityDbContext>(null);
        }

        public SecurityDbContext()
            : base("Name=DefaultConnection")
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            var entity = builder.CreateEntity<User>("tc_employee");

            entity.Property(m => m.LoginName)
                  .HasColumnName("login_name")
                  .IsRequired()
                  .HasMaxLength(64)
                  .IsUnicode(false)
                  .IsVariableLength();

            entity.Property(m => m.DisplayName)
                  .HasColumnName("full_name")
                  .IsRequired()
                  .HasMaxLength(64)
                  .IsUnicode(true)
                  .IsVariableLength();

            entity.Property(m => m._Password)
                  .HasColumnName("password")
                  .IsRequired()
                  .HasMaxLength(64)
                  .IsUnicode(false)
                  .IsVariableLength();

            entity.Property(m => m.Role)
                  .HasColumnName("role")
                  .IsRequired();

            entity.Property(m => m._IsLocked)
                  .HasColumnName("is_locked")
                  .IsRequired();

            entity.Property(m => m._LockDateTimeUtc)
                  .HasColumnName("lock_time")
                  .IsOptional();
        }
    }
}
