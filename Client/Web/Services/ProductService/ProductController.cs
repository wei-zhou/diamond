namespace Home.Services.ProductService
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;
    using Home.Services.SecurityService;

    [ServiceHandleError(ServiceName = ServiceHandleErrorHelper.ProductServiceName)]
    public class ProductController : BaseODataController
    {
        private readonly ProductDbContext database = new ProductDbContext();

        #region Diamond import operations

        // GET odata/DiamondImports
        [ServiceAuthorize]
        [HttpGet]
        [EnableQuery(PageSize = 50)]
        [ODataRoute("DiamondImports")]
        public IHttpActionResult GetDiamondImport(ODataQueryOptions<ProductDiamondImport> options)
        {
            var imports = this.database.DiamondImports.AsQueryable();
            var expands = options.GetExpandPropertyNames();
            if (expands.Contains("Products")) imports = imports.Include(s => s.Products);

            return Ok(imports);
        }

        // GET odata/DiamondImports('id')
        [ServiceAuthorize]
        [HttpGet]
        [EnableQuery]
        [ODataRoute("DiamondImports({id})")]
        public Task<IHttpActionResult> GetDiamondImport([FromODataUri] Guid id, ODataQueryOptions<ProductDiamondImport> options)
        {
            var imports = this.database.DiamondImports.Where(s => s.Id == id);
            var expands = options.GetExpandPropertyNames();
            if (expands.Contains("Products")) imports = imports.Include(s => s.Products);

            return GetODataSingleAsync(imports, options);
        }

        // GET odata/DiamondImports('id')/Property
        [ServiceAuthorize]
        [HttpGet]
        [ODataRoute("DiamondImports({id})/Count")]
        [ODataRoute("DiamondImports({id})/Created")]
        [ODataRoute("DiamondImports({id})/CreatedBy")]
        [ODataRoute("DiamondImports({id})/Modified")]
        [ODataRoute("DiamondImports({id})/ModifiedBy")]
        public async Task<IHttpActionResult> GetDiamondImportProperty([FromODataUri] Guid id)
        {
            var import = await this.database.DiamondImports.FindAsync(id);
            return GetODataProperty(import);
        }

        // POST odata/DiamondImports
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Administrator)]
        [HttpPost]
        [ODataRoute("DiamondImports")]
        public async Task<IHttpActionResult> PostDiamondImport(ProductDiamondImport import)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Created(await CreateAsync(import));
        }

        // PUT odata/DiamondImports('id')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Administrator)]
        [HttpPut]
        [ODataRoute("DiamondImports({id})")]
        public Task<IHttpActionResult> PutDiamondImport([FromODataUri] Guid id, ProductDiamondImport import, ODataQueryOptions<ProductDiamondImport> options)
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult((IHttpActionResult)BadRequest(ModelState));
            }

            if (id != import.Id)
            {
                return Task.FromResult((IHttpActionResult)BadRequest("The Id of import does not match the id"));
            }

            var imports = this.database.DiamondImports.Where(r => r.Id == id);

            return PutOrPatchODataAsync(imports, options,
                () => CreateAsync(import),
                () => UpdateAsync(import));
        }

        // PATCH odata/DiamondImports('id')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Administrator)]
        [HttpPatch]
        [ODataRoute("DiamondImports({id})")]
        public Task<IHttpActionResult> PatchDiamondImport([FromODataUri] Guid id, Delta<ProductDiamondImport> patch, ODataQueryOptions<ProductDiamondImport> options)
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult((IHttpActionResult)BadRequest(ModelState));
            }

            var imports = this.database.DiamondImports.Where(r => r.Id == id);

            return PutOrPatchODataAsync(imports, options,
                () =>
                {
                    return CreatePatchAsync(patch);
                },
                async () =>
                {
                    return await UpdatePatchAsync(await imports.FirstAsync(), patch);
                });
        }

        // DELETE odata/DiamondImports('id')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Administrator)]
        [HttpDelete]
        [ODataRoute("DiamondImports({id})")]
        public Task<IHttpActionResult> DeleteDiamondImport([FromODataUri] Guid id, ODataQueryOptions<ProductDiamondImport> options)
        {
            var imports = this.database.DiamondImports.Where(r => r.Id == id);
            return DeleteODataAsync(imports, options, () => imports.First().DeleteAsync(this.database, this.database.DiamondImports));
        }

        #endregion

        #region Diamond operations

        // GET odata/Diamonds
        [ServiceAuthorize]
        [HttpGet]
        [EnableQuery(PageSize = 50)]
        [ODataRoute("Diamonds")]
        public IHttpActionResult GetDiamond(ODataQueryOptions<ProductDiamond> options)
        {
            var diamonds = this.database.Diamonds.AsQueryable();
            return Ok(diamonds);
        }

        // GET odata/Diamonds('id')
        [ServiceAuthorize]
        [HttpGet]
        [ODataRoute("Diamonds({id})")]
        public Task<IHttpActionResult> GetDiamond([FromODataUri] Guid id, ODataQueryOptions<ProductDiamond> options)
        {
            var diamonds = this.database.Diamonds.Where(s => s.Id == id);
            return GetODataSingleAsync(diamonds, options);
        }

        // GET odata/Diamonds('id')/Property
        [ServiceAuthorize]
        [HttpGet]
        [ODataRoute("Diamonds({id})/ReportType")]
        [ODataRoute("Diamonds({id})/ReportNumber")]
        [ODataRoute("Diamonds({id})/Caret")]
        [ODataRoute("Diamonds({id})/Clarity")]
        [ODataRoute("Diamonds({id})/Color")]
        [ODataRoute("Diamonds({id})/Cut")]
        [ODataRoute("Diamonds({id})/Polish")]
        [ODataRoute("Diamonds({id})/Symmetry")]
        [ODataRoute("Diamonds({id})/Fluorescence")]
        [ODataRoute("Diamonds({id})/Comment")]
        [ODataRoute("Diamonds({id})/Status")]
        [ODataRoute("Diamonds({id})/Cost")]
        [ODataRoute("Diamonds({id})/SalePrice")]
        [ODataRoute("Diamonds({id})/Vendor")]
        [ODataRoute("Diamonds({id})/Created")]
        [ODataRoute("Diamonds({id})/CreatedBy")]
        [ODataRoute("Diamonds({id})/Modified")]
        [ODataRoute("Diamonds({id})/ModifiedBy")]
        public async Task<IHttpActionResult> GetDiamondProperty([FromODataUri] Guid id)
        {
            var diamond = await this.database.Diamonds.FindAsync(id);
            return GetODataProperty(diamond);
        }

        // POST odata/Diamonds
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Administrator)]
        [HttpPost]
        [ODataRoute("Diamonds")]
        public async Task<IHttpActionResult> PostDiamond(ProductDiamond diamond)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Created(await CreateAsync(diamond));
        }

        // PUT odata/Diamonds('id')
        [ServiceAuthorize]
        [HttpPut]
        [ODataRoute("Diamonds({id})")]
        public Task<IHttpActionResult> PutDiamond([FromODataUri] Guid id, ProductDiamond diamond, ODataQueryOptions<ProductDiamond> options)
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult((IHttpActionResult)BadRequest(ModelState));
            }

            if (id != diamond.Id)
            {
                return Task.FromResult((IHttpActionResult)BadRequest("The Id of diamond does not match the id"));
            }

            var diamonds = this.database.Diamonds.Where(r => r.Id == id);

            return PutOrPatchODataAsync(diamonds, options,
                () => CreateAsync(diamond),
                () => UpdateAsync(diamond));
        }

        // PATCH odata/Diamonds('id')
        [ServiceAuthorize]
        [HttpPatch]
        [ODataRoute("Diamonds({id})")]
        public Task<IHttpActionResult> PatchDiamond([FromODataUri] Guid id, Delta<ProductDiamond> patch, ODataQueryOptions<ProductDiamond> options)
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult((IHttpActionResult)BadRequest(ModelState));
            }

            var diamonds = this.database.Diamonds.Where(r => r.Id == id);

            return PutOrPatchODataAsync(diamonds, options,
                () =>
                {
                    return CreatePatchAsync(patch);
                },
                async () =>
                {
                    return await UpdatePatchAsync(await diamonds.FirstAsync(), patch);
                });
        }

        // DELETE odata/Diamonds('id')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Administrator)]
        [HttpDelete]
        [ODataRoute("Diamonds({id})")]
        public Task<IHttpActionResult> DeleteDiamond([FromODataUri] Guid id, ODataQueryOptions<ProductDiamond> options)
        {
            var diamonds = this.database.Diamonds.Where(r => r.Id == id);
            return DeleteODataAsync(diamonds, options, () => diamonds.First().DeleteAsync(this.database, this.database.Diamonds));
        }

        #endregion

        #region Diamond import helpers

        private async Task<ProductDiamondImport> CreateAsync(ProductDiamondImport entity)
        {
            var now = DateTimeOffset.UtcNow;
            var user = Thread.CurrentPrincipal.Identity.Name;

            entity.Create(this.database.DiamondImports, now, user);

            if (entity.Products != null)
            {
                foreach (var product in entity.Products)
                {
                    product._ImportId = entity.Id;
                    product.Create(this.database.Diamonds, now, user);
                }
            }

            await this.database.SaveChangesAsync();
            return entity;
        }

        private async Task<ProductDiamondImport> UpdateAsync(ProductDiamondImport entity)
        {
            var now = DateTimeOffset.UtcNow;
            var user = Thread.CurrentPrincipal.Identity.Name;

            if (entity.Products != null)
            {
                foreach (var product in entity.Products)
                {
                    product._ImportId = entity.Id;
                    product.Update(this.database, this.database.Diamonds, now, user);
                }
            }

            entity.Update(this.database, this.database.DiamondImports, now, user);

            await this.database.SaveChangesAsync();
            return entity;
        }

        private Task<ProductDiamondImport> CreatePatchAsync(Delta<ProductDiamondImport> patch)
        {
            var entity = new ProductDiamondImport();
            patch.Patch(entity);
            return CreateAsync(entity);
        }

        private Task<ProductDiamondImport> UpdatePatchAsync(ProductDiamondImport entity, Delta<ProductDiamondImport> patch)
        {
            patch.Patch(entity);
            return UpdateAsync(entity);
        }

        #endregion

        #region Diamond helpers

        private async Task<ProductDiamond> CreateAsync(ProductDiamond entity)
        {
            var now = DateTimeOffset.UtcNow;
            var user = Thread.CurrentPrincipal.Identity.Name;

            entity.Create(this.database.Diamonds, now, user);

            await this.database.SaveChangesAsync();
            return entity;
        }

        private async Task<ProductDiamond> UpdateAsync(ProductDiamond entity)
        {
            var now = DateTimeOffset.UtcNow;
            var user = Thread.CurrentPrincipal.Identity.Name;

            entity.Update(this.database, this.database.Diamonds, now, user);

            await this.database.SaveChangesAsync();
            return entity;
        }

        private Task<ProductDiamond> CreatePatchAsync(Delta<ProductDiamond> patch)
        {
            var entity = new ProductDiamond();
            patch.Patch(entity);
            return CreateAsync(entity);
        }

        private Task<ProductDiamond> UpdatePatchAsync(ProductDiamond entity, Delta<ProductDiamond> patch)
        {
            patch.Patch(entity);
            return UpdateAsync(entity);
        }

        #endregion

        #region Helpers

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.database.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}