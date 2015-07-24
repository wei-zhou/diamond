namespace Home.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Results;
    using System.Web.OData;
    using System.Web.OData.Query;

    public abstract class BaseODataController : ODataController
    {
        protected async Task<IHttpActionResult> GetODataSingleAsync<T>(IQueryable<T> queryable, ODataQueryOptions<T> options)
            where T : class
        {
            Debug.Assert(queryable != null);
            Debug.Assert(options != null);

            if (!(await queryable.AnyAsync()))
            {
                return NotFound();
            }

            if (options.IfNoneMatch != null)
            {
                queryable = options.IfNoneMatch.ApplyTo(queryable);
                if (queryable.Any())
                {
                    return Ok(queryable.First());
                }

                return StatusCode(HttpStatusCode.NotModified);
            }

            return Ok(queryable.First());
        }

        protected IHttpActionResult GetODataProperty<T>(T entity)
            where T : class
        {
            if (entity == null)
            {
                return NotFound();
            }

            var propertyName = this.Url.Request.RequestUri.Segments.Last();
            var propertyValue = ControllerHelper.GetPropertyValueFromModel(entity, propertyName);
            return (propertyValue == null) ? StatusCode(HttpStatusCode.NoContent) : ControllerHelper.GetOKHttpActionResult(this, propertyValue);
        }

        protected async Task<IHttpActionResult> PutOrPatchODataAsync<T>(IQueryable<T> queryable, ODataQueryOptions<T> options, Func<Task<T>> create, Func<Task<T>> update)
            where T : class
        {
            Debug.Assert(queryable != null);
            Debug.Assert(options != null);
            Debug.Assert(create != null);
            Debug.Assert(update != null);

            if (!await queryable.AnyAsync() && options.IfMatch == null)
            {
                return Created(await create());
            }

            if (options.IfMatch != null)
            {
                if (options.IfMatch.ApplyTo(queryable).Any())
                {
                    return Updated(await update());
                }

                return StatusCode(HttpStatusCode.PreconditionFailed);
            }

            if (options.IfNoneMatch != null)
            {
                if (options.IfNoneMatch.IsAny)
                {
                    return Created(await create());
                }

                if (options.IfNoneMatch.ApplyTo(queryable).Any())
                {
                    return Updated(await update());
                }

                return StatusCode(HttpStatusCode.PreconditionFailed);
            }

            return PreconditionRequired();
        }

        protected async Task<IHttpActionResult> DeleteODataAsync<T>(IQueryable<T> queryable, ODataQueryOptions<T> options, Func<Task> delete)
            where T : class
        {
            Debug.Assert(queryable != null);
            Debug.Assert(options != null);
            Debug.Assert(delete != null);

            if (!await queryable.AnyAsync())
            {
                return NotFound();
            }

            if (options.IfMatch != null)
            {
                if (options.IfMatch.ApplyTo(queryable).Any())
                {
                    await delete();
                    return StatusCode(HttpStatusCode.NoContent);
                }

                return StatusCode(HttpStatusCode.PreconditionFailed);
            }

            if (options.IfNoneMatch != null)
            {
                if (options.IfNoneMatch.IsAny)
                {
                    return StatusCode(HttpStatusCode.PreconditionFailed);
                }

                if (options.IfNoneMatch.ApplyTo(queryable).Any())
                {
                    await delete();
                    return StatusCode(HttpStatusCode.NoContent);
                }

                return StatusCode(HttpStatusCode.PreconditionFailed);
            }

            return PreconditionRequired();
        }

        protected IHttpActionResult PreconditionRequired()
        {
            return new StatusCodeResult((HttpStatusCode)428, this);
        }
    }
}