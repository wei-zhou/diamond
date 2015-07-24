namespace Home.Security.Repository
{
    using System;

    public class ApplicationUser
    {
        public virtual Guid Id { get; set; }
        public virtual string Key { get; set; }
        public virtual string Name { get; set; }
        public virtual byte IdentityProvider { get; set; }
        public virtual byte Role { get; set; }
        public virtual bool IsLocked { get; set; }
        public virtual DateTime? LockTimeUtc { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime Modified { get; set; }
        public virtual byte[] RowVersion { get; set; }
    }
}
