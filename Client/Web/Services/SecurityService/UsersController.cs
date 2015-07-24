namespace Home.Services.SecurityService
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    [ServiceHandleError(ServiceName = ServiceHandleErrorHelper.SecurityServiceName)]
    public class UsersController : BaseODataController
    {
        private readonly SecurityDbContext database = new SecurityDbContext();

        // GET odata/Users
        [ServiceAuthorize]
        [HttpGet]
        [EnableQuery(PageSize = 50)]
        public IHttpActionResult Get()
        {
            var users = this.database.Users.Where(r => !r._IsLocked);
            return Ok(users);
        }

        // GET odata/Users('id')
        [ServiceAuthorize]
        [HttpGet]
        [EnableQuery]
        [ODataRoute("Users({id})")]
        public Task<IHttpActionResult> Get([FromODataUri] Guid id, ODataQueryOptions<User> options)
        {
            var users = this.database.Users.Where(r => r.Id == id && !r._IsLocked);
            return GetODataSingleAsync(users, options);
        }

        // GET odata/Users('id')/Property
        [ServiceAuthorize]
        [HttpGet]
        [ODataRoute("Users({id})/LoginName")]
        [ODataRoute("Users({id})/DisplayName")]
        [ODataRoute("Users({id})/Role")]
        [ODataRoute("Users({id})/Created")]
        [ODataRoute("Users({id})/CreatedBy")]
        [ODataRoute("Users({id})/Modified")]
        [ODataRoute("Users({id})/ModifiedBy")]
        public async Task<IHttpActionResult> GetProperty([FromODataUri] Guid id)
        {
            var user = await this.database.Users.FirstOrDefaultAsync(r => r.Id == id && !r._IsLocked);
            return GetODataProperty(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.database.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
