namespace Home.Services.SecurityService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class User : Entity
    {
        [Required]
        public string LoginName { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public Role Role { get; set; }

        public string _Password { get; set; }

        public bool _IsLocked { get; set; }

        public DateTimeOffset? _LockDateTimeUtc { get; set; }
    }
}
