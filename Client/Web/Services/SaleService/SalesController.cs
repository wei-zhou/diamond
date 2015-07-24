namespace Home.Services.SaleService
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

    [ServiceHandleError(ServiceName = ServiceHandleErrorHelper.SalesServiceName)]
    public class SalesController : BaseODataController
    {
        private readonly SaleDbContext database = new SaleDbContext();

        #region Sale header operations

        // GET odata/Sales
        [ServiceAuthorize]
        [HttpGet]
        [EnableQuery(PageSize = 50)]
        [ODataRoute("Sales")]
        public IHttpActionResult Get(ODataQueryOptions<SaleHeader> options)
        {
            var sales = this.database.SaleHeaders.AsQueryable();
            var expands = options.GetExpandPropertyNames();
            if (expands.Contains("SaleLineItems")) sales = sales.Include(s => s.Items);
            if (expands.Contains("CustomerContacts")) sales = sales.Include(s => s.CustomerContacts);

            return Ok(sales);
        }

        // GET odata/Sales('id')
        [ServiceAuthorize]
        [HttpGet]
        [EnableQuery]
        [ODataRoute("Sales({id})")]
        public Task<IHttpActionResult> Get([FromODataUri] Guid id, ODataQueryOptions<SaleHeader> options)
        {
            var sales = this.database.SaleHeaders.Where(s => s.Id == id);
            var expands = options.GetExpandPropertyNames();
            if (expands.Contains("SaleLineItems")) sales = sales.Include(s => s.Items);
            if (expands.Contains("CustomerContacts")) sales = sales.Include(s => s.CustomerContacts);

            return GetODataSingleAsync(sales, options);
        }

        // GET odata/Sales('id')/Property
        [ServiceAuthorize]
        [HttpGet]
        [ODataRoute("Sales({id})/NumberText")]
        [ODataRoute("Sales({id})/DayNumber")]
        [ODataRoute("Sales({id})/TotalNumber")]
        [ODataRoute("Sales({id})/SalesPersonName")]
        [ODataRoute("Sales({id})/CustomerName")]
        [ODataRoute("Sales({id})/Status")]
        [ODataRoute("Sales({id})/Created")]
        [ODataRoute("Sales({id})/CreatedBy")]
        [ODataRoute("Sales({id})/Modified")]
        [ODataRoute("Sales({id})/ModifiedBy")]
        public async Task<IHttpActionResult> GetProperty([FromODataUri] Guid id)
        {
            var sale = await this.database.SaleHeaders.FindAsync(id);
            return GetODataProperty(sale);
        }

        // POST odata/Sales
        [ServiceAuthorize]
        [HttpPost]
        [ODataRoute("Sales")]
        public async Task<IHttpActionResult> Post(SaleHeader sale)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Created(await CreateAsync(sale));
        }

        // PUT odata/Sales('id')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Manager)]
        [HttpPut]
        [ODataRoute("Sales({id})")]
        public Task<IHttpActionResult> Put([FromODataUri] Guid id, SaleHeader sale, ODataQueryOptions<SaleHeader> options)
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult((IHttpActionResult)BadRequest(ModelState));
            }

            if (id != sale.Id)
            {
                return Task.FromResult((IHttpActionResult)BadRequest("The Id of sale does not match the id"));
            }

            var sales = this.database.SaleHeaders.Where(r => r.Id == id);

            return PutOrPatchODataAsync(sales, options,
                () => CreateAsync(sale),
                () => UpdateAsync(sale));
        }

        // PATCH odata/Sales('id')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Manager)]
        [HttpPatch]
        [ODataRoute("Sales({id})")]
        public Task<IHttpActionResult> Patch([FromODataUri] Guid id, Delta<SaleHeader> patch, ODataQueryOptions<SaleHeader> options)
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult((IHttpActionResult)BadRequest(ModelState));
            }

            var sales = this.database.SaleHeaders.Where(r => r.Id == id);

            return PutOrPatchODataAsync(sales, options,
                () =>
                {
                    return CreatePatchAsync(patch);
                },
                async () =>
                {
                    return await UpdatePatchAsync(await sales.FirstAsync(), patch);
                });
        }

        // DELETE odata/Sales('id')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Manager)]
        [HttpDelete]
        [ODataRoute("Sales({id})")]
        public Task<IHttpActionResult> Delete([FromODataUri] Guid id, ODataQueryOptions<SaleHeader> options)
        {
            var sales = this.database.SaleHeaders.Where(r => r.Id == id);
            return DeleteODataAsync(sales, options, () => sales.First().DeleteAsync(this.database, this.database.SaleHeaders));
        }

        #endregion

        #region Sale line item operations

        // GET odata/Sales('id')/Items
        [ServiceAuthorize]
        [HttpGet]
        [EnableQuery(PageSize = 50)]
        [ODataRoute("Sales({id})/Items")]
        public IHttpActionResult GetSaleLineItems([FromODataUri] Guid id, ODataQueryOptions<SaleLineItem> options)
        {
            var items = this.database.SaleLineItems.Where(r => r._SaleHeaderId == id);
            return Ok(items);
        }

        // GET odata/Sales('id')/Items('itemId')
        [ServiceAuthorize]
        [HttpGet]
        [ODataRoute("Sales({id})/Items({itemId})")]
        public Task<IHttpActionResult> GetTransaction([FromODataUri] Guid id, [FromODataUri] Guid itemId, ODataQueryOptions<SaleLineItem> options)
        {
            var items = this.database.SaleLineItems.Where(r => r.Id == itemId && r._SaleHeaderId == id);
            return GetODataSingleAsync(items, options);
        }

        // GET odata/Sales('id')/Items('itemId')/Property
        [ServiceAuthorize]
        [HttpGet]
        [ODataRoute("Sales({id})/Items({itemId})/ProductName")]
        [ODataRoute("Sales({id})/Items({itemId})/ProductDescription")]
        [ODataRoute("Sales({id})/Items({itemId})/Quantity")]
        [ODataRoute("Sales({id})/Items({itemId})/UnitPrice")]
        [ODataRoute("Sales({id})/Items({itemId})/Status")]
        [ODataRoute("Sales({id})/Items({itemId})/Created")]
        [ODataRoute("Sales({id})/Items({itemId})/CreatedBy")]
        [ODataRoute("Sales({id})/Items({itemId})/Modified")]
        [ODataRoute("Sales({id})/Items({itemId})/ModifiedBy")]
        public async Task<IHttpActionResult> GetTransactionProperty([FromODataUri] Guid id, [FromODataUri] Guid itemId)
        {
            var item = await this.database.SaleLineItems.FindAsync(itemId);
            if (item == null || item._SaleHeaderId != id)
            {
                return NotFound();
            }

            return GetODataProperty(item);
        }

        // POST odata/Sales('id')/Items
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Manager)]
        [HttpPost]
        [ODataRoute("Sales({id})/Items")]
        public async Task<IHttpActionResult> PostTransaction([FromODataUri] Guid id, SaleLineItem item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            item._SaleHeaderId = id;
            return Created(await item.CreateAsync(this.database, this.database.SaleLineItems));
        }

        // PUT odata/Sales('id')/Items('itemId')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Manager)]
        [HttpPut]
        [ODataRoute("Sales({id})/Items({itemId})")]
        public Task<IHttpActionResult> PutTransaction([FromODataUri] Guid id, [FromODataUri] Guid itemId, SaleLineItem item, ODataQueryOptions<SaleLineItem> options)
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult((IHttpActionResult)BadRequest(ModelState));
            }

            if (itemId != item.Id)
            {
                return Task.FromResult((IHttpActionResult)BadRequest("The Id of item does not match the itemId"));
            }

            var items = this.database.SaleLineItems.Where(r => r.Id == itemId && r._SaleHeaderId == id);

            return PutOrPatchODataAsync(items, options,
                () =>
                {
                    item._SaleHeaderId = id;
                    return item.CreateAsync(this.database, this.database.SaleLineItems);
                },
                () =>
                {
                    item._SaleHeaderId = id;
                    return item.UpdateAsync(this.database, this.database.SaleLineItems);
                });
        }

        // PATCH odata/Sales('id')/Items('itemId')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Manager)]
        [HttpPatch]
        [ODataRoute("Sales({id})/Items({itemId})")]
        public Task<IHttpActionResult> PatchTransaction([FromODataUri] Guid id, [FromODataUri] Guid itemId, Delta<SaleLineItem> patch, ODataQueryOptions<SaleLineItem> options)
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult((IHttpActionResult)BadRequest(ModelState));
            }

            var items = this.database.SaleLineItems.Where(r => r.Id == itemId && r._SaleHeaderId == id);

            return PutOrPatchODataAsync(items, options,
                () =>
                {
                    return patch.CreatePatchAsync(this.database, this.database.SaleLineItems, item => item._SaleHeaderId = id);
                },
                async () =>
                {
                    return await patch.UpdatePatchAsync(await items.FirstAsync(), this.database, this.database.SaleLineItems);
                });
        }

        // DELETE odata/Sales('id')/Items('itemId')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Manager)]
        [HttpDelete]
        [ODataRoute("Sales({id})/Items({itemId})")]
        public Task<IHttpActionResult> DeleteTransaction([FromODataUri] Guid id, [FromODataUri] Guid itemId, ODataQueryOptions<SaleLineItem> options)
        {
            var items = this.database.SaleLineItems.Where(r => r.Id == itemId && r._SaleHeaderId == id);
            return DeleteODataAsync(items, options, () => items.First().DeleteAsync(this.database, this.database.SaleLineItems));
        }

        #endregion

        #region Customer contact operations

        // GET odata/Sales('id')/CustomerContacts
        [ServiceAuthorize]
        [HttpGet]
        [EnableQuery(PageSize = 50)]
        [ODataRoute("Sales({id})/CustomerContacts")]
        public IHttpActionResult GetContacts([FromODataUri] Guid id, ODataQueryOptions<Contact> options)
        {
            var contacts = this.database.Contacts.Where(r => r._SaleHeaderId == id);
            return Ok(contacts);
        }

        // GET odata/Sales('id')/CustomerContacts('contactId')
        [ServiceAuthorize]
        [HttpGet]
        [ODataRoute("Sales({id})/CustomerContacts({contactId})")]
        public Task<IHttpActionResult> GetContact([FromODataUri] Guid id, [FromODataUri] Guid contactId, ODataQueryOptions<Contact> options)
        {
            var contacts = this.database.Contacts.Where(r => r.Id == contactId && r._SaleHeaderId == id);
            return GetODataSingleAsync(contacts, options);
        }

        // GET odata/Sales('id')/CustomerContacts('contactId')/Property
        [ServiceAuthorize]
        [HttpGet]
        [ODataRoute("Sales({id})/CustomerContacts({contactId})/Method")]
        [ODataRoute("Sales({id})/CustomerContacts({contactId})/Value")]
        [ODataRoute("Sales({id})/CustomerContacts({contactId})/Created")]
        [ODataRoute("Sales({id})/CustomerContacts({contactId})/CreatedBy")]
        [ODataRoute("Sales({id})/CustomerContacts({contactId})/Modified")]
        [ODataRoute("Sales({id})/CustomerContacts({contactId})/ModifiedBy")]
        public async Task<IHttpActionResult> GetContactProperty([FromODataUri] Guid id, [FromODataUri] Guid contactId)
        {
            var contact = await this.database.Contacts.FindAsync(contactId);
            if (contact == null || contact._SaleHeaderId != id)
            {
                return NotFound();
            }

            return GetODataProperty(contact);
        }

        // POST odata/Sales('id')/CustomerContacts
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Manager)]
        [HttpPost]
        [ODataRoute("Sales({id})/CustomerContacts")]
        public async Task<IHttpActionResult> PostContact([FromODataUri] Guid id, Contact contact)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            contact._SaleHeaderId = id;
            return Created(await contact.CreateAsync(this.database, this.database.Contacts));
        }

        // PUT odata/Sales('id')/CustomerContacts('contactId')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Manager)]
        [HttpPut]
        [ODataRoute("Sales({id})/CustomerContacts({contactId})")]
        public Task<IHttpActionResult> PutContact([FromODataUri] Guid id, [FromODataUri] Guid contactId, Contact contact, ODataQueryOptions<Contact> options)
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult((IHttpActionResult)BadRequest(ModelState));
            }

            if (contactId != contact.Id)
            {
                return Task.FromResult((IHttpActionResult)BadRequest("The Id of contact does not match the contactId"));
            }

            var contacts = this.database.Contacts.Where(r => r.Id == contactId && r._SaleHeaderId == id);

            return PutOrPatchODataAsync(contacts, options,
                () =>
                {
                    contact._SaleHeaderId = id;
                    return contact.CreateAsync(this.database, this.database.Contacts);
                },
                () =>
                {
                    contact._SaleHeaderId = id;
                    return contact.UpdateAsync(this.database, this.database.Contacts);
                });
        }

        // PATCH odata/Sales('id')/CustomerContacts('contactId')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Manager)]
        [HttpPatch]
        [ODataRoute("Sales({id})/CustomerContacts({contactId})")]
        public Task<IHttpActionResult> PatchContact([FromODataUri] Guid id, [FromODataUri] Guid contactId, Delta<Contact> patch, ODataQueryOptions<Contact> options)
        {
            if (!ModelState.IsValid)
            {
                return Task.FromResult((IHttpActionResult)BadRequest(ModelState));
            }

            var contacts = this.database.Contacts.Where(r => r.Id == contactId && r._SaleHeaderId == id);

            return PutOrPatchODataAsync(contacts, options,
                () =>
                {
                    return patch.CreatePatchAsync(this.database, this.database.Contacts, contact => contact._SaleHeaderId = id);
                },
                async () =>
                {
                    return await patch.UpdatePatchAsync(await contacts.FirstAsync(), this.database, this.database.Contacts);
                });
        }

        // DELETE odata/Sales('id')/CustomerContacts('contactId')
        [ServiceAuthorize(Roles = SecurityServiceHelper.RoleConstants.Manager)]
        [HttpDelete]
        [ODataRoute("Sales({id})/CustomerContacts({contactId})")]
        public Task<IHttpActionResult> DeleteContact([FromODataUri] Guid id, [FromODataUri] Guid contactId, ODataQueryOptions<Contact> options)
        {
            var contacts = this.database.Contacts.Where(r => r.Id == contactId && r._SaleHeaderId == id);
            return DeleteODataAsync(contacts, options, () => contacts.First().DeleteAsync(this.database, this.database.Contacts));
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

        private async Task<SaleHeader> CreateAsync(SaleHeader entity)
        {
            var now = DateTimeOffset.UtcNow;
            var user = Thread.CurrentPrincipal.Identity.Name;

            var start = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, 0, TimeSpan.Zero);
            var end = new DateTimeOffset(now.Year, now.Month, now.Day, 23, 59, 59, 999, TimeSpan.Zero);
            var dayNumbers = this.database.SaleHeaders.Where(s => s.Created >= start && s.Created <= end).Select(s => s.DayNumber);
            if (await dayNumbers.AnyAsync())
            {
                entity.DayNumber = (await dayNumbers.MaxAsync()) + 1;
            }
            else
            {
                entity.DayNumber = 1;
            }
            var totalNumbers = this.database.SaleHeaders.Select(s => s.TotalNumber);
            if (await totalNumbers.AnyAsync())
            {
                entity.TotalNumber = (await totalNumbers.MaxAsync()) + 1;
            }
            else
            {
                entity.TotalNumber = 1;
            }
            entity.Status = SaleStatus.Ok;
            entity.Create(this.database.SaleHeaders, now, user);
            entity.NumberText = entity.GenerateSaleNumber();

            if (entity.CustomerContacts != null)
            {
                foreach (var contact in entity.CustomerContacts)
                {
                    contact._SaleHeaderId = entity.Id;
                    contact.Create(this.database.Contacts, now, user);
                }
            }

            if (entity.Items != null)
            {
                foreach (var item in entity.Items)
                {
                    item._SaleHeaderId = entity.Id;
                    item.Status = SaleStatus.Ok;
                    item.Create(this.database.SaleLineItems, now, user);
                }
            }

            await this.database.SaveChangesAsync();
            return entity;
        }

        private async Task<SaleHeader> UpdateAsync(SaleHeader entity)
        {
            var now = DateTimeOffset.UtcNow;
            var user = Thread.CurrentPrincipal.Identity.Name;

            if (entity.CustomerContacts != null)
            {
                foreach (var contact in entity.CustomerContacts)
                {
                    contact._SaleHeaderId = entity.Id;
                    contact.Update(this.database, this.database.Contacts, now, user);
                }
            }

            if (entity.Items != null)
            {
                foreach (var item in entity.Items)
                {
                    item._SaleHeaderId = entity.Id;
                    item.Update(this.database, this.database.SaleLineItems, now, user);
                }
            }

            entity.Update(this.database, this.database.SaleHeaders, now, user);

            await this.database.SaveChangesAsync();
            return entity;
        }

        private Task<SaleHeader> CreatePatchAsync(Delta<SaleHeader> patch)
        {
            var entity = new SaleHeader();
            patch.Patch(entity);
            return CreateAsync(entity);
        }

        private Task<SaleHeader> UpdatePatchAsync(SaleHeader entity, Delta<SaleHeader> patch)
        {
            patch.Patch(entity);
            return UpdateAsync(entity);
        }

        #endregion
    }
}
