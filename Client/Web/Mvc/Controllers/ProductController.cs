namespace Home.Mvc.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using System.Web;
    using System.Web.Mvc;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Home.Mvc.Models;
    using Home.Services.ProductService;

    [MvcHandleError]
    [MvcAuthorize]
    [RequireAjax]
    public class ProductController : Controller
    {
        // CSV sample
        // 123,GB,"""Good"""
        // 223,hg,",12G"
        [HttpPost]
        public async Task<ActionResult> UploadDiamondFile(HttpPostedFileBase file, ProductFileUploadType type, bool header)
        {
            if (file == null || file.ContentLength == 0 || !this.ModelState.IsValid)
            {
                return new HttpStatusCodeResult(HttpStatusCode.PreconditionFailed);
            }

            var transaction = default(TransactionScope);
            var database = default(ProductDbContext);
            var streamReader = default(StreamReader);
            var csvReader = default(CsvReader);
            try
            {
                streamReader = new StreamReader(file.InputStream);
                csvReader = new CsvReader(streamReader, new CsvConfiguration()
                {
                    Delimiter = ",",
                    HasHeaderRecord = header,
                    IgnoreHeaderWhiteSpace = false,
                    Quote = '\"',
                    QuoteAllFields = false,
                    QuoteNoFields = false,
                    ThrowOnBadData = true,
                    TrimFields = true,
                });
                transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled);
                database = new ProductDbContext();

                var now = DateTimeOffset.UtcNow;
                var user = Thread.CurrentPrincipal.Identity.Name;

                var import = new ProductDiamondImport()
                {
                    Id = Guid.NewGuid(),
                    Created = now,
                    CreatedBy = user,
                    Modified = now,
                    ModifiedBy = user,
                };
                database.DiamondImports.Add(import);

                var count = 0;
                switch (type)
                {
                    case ProductFileUploadType.Append:
                        while (csvReader.Read())
                        {
                            var diamond = New(csvReader, import, now, user);
                            diamond.Id = Guid.NewGuid();
                            database.Diamonds.Add(diamond);
                            ++count;
                        }
                        break;
                    case ProductFileUploadType.Replace:
                        await database.Database.ExecuteSqlCommandAsync("DELETE tc_product_diamond");
                        while (csvReader.Read())
                        {
                            var diamond = New(csvReader, import, now, user);
                            diamond.Id = Guid.NewGuid();
                            database.Diamonds.Add(diamond);
                            ++count;
                        }
                        break;
                    case ProductFileUploadType.Update:
                        var diamonds = database.Diamonds.ToArray();
                        while (csvReader.Read())
                        {
                            var index = csvReader.GetField<int>(0);
                            var diamond = diamonds.First(d => d.Index == index);
                            SetValues(csvReader, diamond, import, now, user, true);
                            ++count;
                        }
                        break;
                }

                import.Count = count;
                await database.SaveChangesAsync();
                transaction.Complete();
            }
            finally
            {
                if (csvReader != null) csvReader.Dispose();
                if (streamReader != null) streamReader.Dispose();
                if (database != null) database.Dispose();
                if (transaction != null) transaction.Dispose();
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private static string GetString(CsvReader reader, int index)
        {
            var value = reader.GetField<string>(index);
            return string.IsNullOrEmpty(value) ? default(string) : value;
        }

        private static T? GetEnum<T>(CsvReader reader, int index)
            where T : struct
        {
            var value = reader.GetField<string>(index);
            return string.IsNullOrEmpty(value) ? new T?() : new T?((T)Enum.Parse(typeof(T), value));
        }

        private static decimal? GetDecimal(CsvReader reader, int index)
        {
            var value = reader.GetField<string>(index);
            return string.IsNullOrEmpty(value) ? new decimal?() : new decimal?(decimal.Parse(value));
        }

        private static ProductDiamond New(CsvReader reader, ProductDiamondImport import, DateTimeOffset now, string user)
        {
            var diamond = new ProductDiamond();
            SetValues(reader, diamond, import, now, user, false);
            return diamond;
        }

        private static void SetValues(CsvReader reader, ProductDiamond diamond, ProductDiamondImport import, DateTimeOffset now, string user, bool update)
        {
            if (!update)
            {
                diamond.Created = now;
                diamond.CreatedBy = user;
            }

            diamond.Modified = now;
            diamond.ModifiedBy = user;
            diamond.Status = ProductStatus.Active;
            diamond.Index = reader.GetField<int>(0);
            diamond.Cost = reader.GetField<decimal>(1);
            diamond.SalePrice = reader.GetField<decimal>(2);
            diamond.Vendor = GetString(reader, 3);
            diamond._ImportId = import.Id;
            diamond.ReportType = GetEnum<ProductDiamondReport>(reader, 4);
            diamond.ReportNumber = GetString(reader, 5);
            diamond.Caret = GetDecimal(reader, 6);
            diamond.Clarity = GetEnum<ProductDiamondClarity>(reader, 7);
            diamond.Color = GetEnum<ProductDiamondColor>(reader, 8);
            diamond.Cut = GetEnum<ProductDiamondCut>(reader, 9);
            diamond.Polish = GetEnum<ProductDiamondPolish>(reader, 10);
            diamond.Symmetry = GetEnum<ProductDiamondSymmetry>(reader, 11);
            diamond.Fluorescence = GetString(reader, 12);
            var comment = default(string);
            if (reader.TryGetField<string>(13, out comment))
            {
                diamond.Comment = string.IsNullOrEmpty(comment) ? default(string) : comment;
            }
            else
            {
                diamond.Comment = null;
            }
        }
    }
}