namespace Home.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.OData;

    public static class RepositoryHelper
    {
        public static bool IsTransient(this Entity entity)
        {
            Debug.Assert(entity != null);

            return entity.Id == Guid.Empty;
        }

        public static EntityTypeConfiguration<TEntityType> CreateEntity<TEntityType>(this DbModelBuilder builder, string tableName)
            where TEntityType : Entity
        {
            Debug.Assert(builder != null);
            Debug.Assert(!string.IsNullOrEmpty(tableName));

            var entity = builder.Entity<TEntityType>();

            entity.ToTable(tableName);

            entity.HasKey(m => m.Id)
                  .Property(m => m.Id)
                  .HasColumnName("id")
                  .IsRequired();

            entity.Property(m => m.Created)
                  .HasColumnName("created")
                  .IsRequired();

            entity.Property(m => m.CreatedBy)
                  .HasColumnName("created_by")
                  .IsRequired()
                  .HasMaxLength(64)
                  .IsUnicode(true)
                  .IsVariableLength();

            entity.Property(m => m.Modified)
                  .HasColumnName("modified")
                  .IsRequired();

            entity.Property(m => m.ModifiedBy)
                  .HasColumnName("modified_by")
                  .IsRequired()
                  .HasMaxLength(64)
                  .IsUnicode(true)
                  .IsVariableLength();

            entity.Property(m => m.RowVersion)
                  .HasColumnName("row_version")
                  .IsRowVersion();

            return entity;
        }

        public static T Create<T>(this T entity, DbSet<T> set, DateTimeOffset now, string user)
            where T : Entity, new()
        {
            Debug.Assert(entity != null);
            Debug.Assert(set != null);

            entity.InitializeCreate(now, user);
            set.Add(entity);
            return entity;
        }

        public static async Task<T> CreateAsync<T>(this T entity, DbContext database, DbSet<T> set)
            where T : Entity, new()
        {
            Debug.Assert(entity != null);
            Debug.Assert(database != null);
            Debug.Assert(set != null);

            var now = DateTimeOffset.UtcNow;
            var user = Thread.CurrentPrincipal.Identity.Name;
            entity.InitializeCreate(now, user);
            set.Add(entity);
            await database.SaveChangesAsync();
            return entity;
        }

        public static T CreatePatch<T>(this Delta<T> patch, DbSet<T> set, DateTimeOffset now, string user, Action<T> initialize)
            where T : Entity, new()
        {
            Debug.Assert(patch != null);
            Debug.Assert(set != null);
            Debug.Assert(initialize != null);

            var entity = new T();
            patch.Patch(entity);
            initialize(entity);
            return entity.Create(set, now, user);
        }

        public static Task<T> CreatePatchAsync<T>(this Delta<T> patch, DbContext database, DbSet<T> set, Action<T> initialize)
            where T : Entity, new()
        {
            Debug.Assert(patch != null);
            Debug.Assert(database != null);
            Debug.Assert(set != null);
            Debug.Assert(initialize != null);

            var entity = new T();
            patch.Patch(entity);
            initialize(entity);
            return entity.CreateAsync(database, set);
        }

        public static T Update<T>(this T entity, DbContext database, DbSet<T> set, DateTimeOffset now, string user)
            where T : Entity, new()
        {
            Debug.Assert(entity != null);
            Debug.Assert(database != null);
            Debug.Assert(set != null);

            entity.InitializeUpdate(now, user);
            set.Attach(entity);
            database.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        public static async Task<T> UpdateAsync<T>(this T entity, DbContext database, DbSet<T> set)
            where T : Entity, new()
        {
            Debug.Assert(entity != null);
            Debug.Assert(database != null);
            Debug.Assert(set != null);

            var now = DateTimeOffset.UtcNow;
            var user = Thread.CurrentPrincipal.Identity.Name;
            entity.InitializeUpdate(now, user);
            set.Attach(entity);
            database.Entry(entity).State = EntityState.Modified;
            await database.SaveChangesAsync();
            return entity;
        }

        public static T UpdatePatch<T>(this Delta<T> patch, T entity, DbContext database, DbSet<T> set, DateTimeOffset now, string user)
            where T : Entity, new()
        {
            Debug.Assert(patch != null);
            Debug.Assert(entity != null);
            Debug.Assert(set != null);

            patch.Patch(entity);
            return entity.Update(database, set, now, user);
        }

        public static Task<T> UpdatePatchAsync<T>(this Delta<T> patch, T entity, DbContext database, DbSet<T> set)
            where T : Entity, new()
        {
            Debug.Assert(patch != null);
            Debug.Assert(entity != null);
            Debug.Assert(database != null);
            Debug.Assert(set != null);

            patch.Patch(entity);
            return entity.UpdateAsync(database, set);
        }

        public static void Delete<T>(this T entity, DbSet<T> set)
            where T : Entity, new()
        {
            Debug.Assert(entity != null);
            Debug.Assert(set != null);

            set.Remove(entity);
        }

        public static Task DeleteAsync<T>(this T entity, DbContext database, DbSet<T> set)
            where T : Entity, new()
        {
            Debug.Assert(entity != null);
            Debug.Assert(database != null);
            Debug.Assert(set != null);

            set.Remove(entity);
            return database.SaveChangesAsync();
        }

        public static void InitializeCreate(this Entity entity, DateTimeOffset now, string user)
        {
            Debug.Assert(entity != null);

            entity.Id = Guid.NewGuid();
            entity.Created = now;
            entity.CreatedBy = user;
            entity.Modified = now;
            entity.ModifiedBy = user;
        }

        public static void InitializeUpdate(this Entity entity, DateTimeOffset now, string user)
        {
            Debug.Assert(entity != null);

            entity.Modified = now;
            entity.ModifiedBy = user;
        }
    }
}
