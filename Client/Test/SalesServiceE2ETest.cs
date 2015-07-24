namespace Home.Test
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Home.Services;
    using Home.Services.SaleService;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class SalesServiceE2ETest : BaseTest
    {
        #region Initialize & cleanup

        private static readonly string SalesServiceRootUrl = ConfigurationManager.AppSettings["SalesServiceRootUrl"];

        [TestInitialize]
        public void TestInitialize()
        {
            TestCleanup();

            var totalNumber = default(int);
            using (var db = new SaleDbContext())
            {
                if (db.SaleHeaders.Any())
                {
                    totalNumber = db.SaleHeaders.Max(s => s.TotalNumber);
                }
            }

            var sales = new List<SaleHeader>(55);

            for (int i = 0; i < sales.Capacity; i++)
            {
                var now = GenerateFakeNow();
                var random = new Random(i);

                var sale = new SaleHeader()
                {
                    DayNumber = sales.Count(s => s.Created.Date == now.Date) + 1,
                    TotalNumber = totalNumber + i + 1,
                    SalesPersonName = GenerateSalesPersonName(i),
                    CustomerName = string.Format("customer {0}", i + 1),
                    Status = GenerateEnumValue<SaleStatus>(),
                };
                sale.NumberText = sale.GenerateSaleNumber();
                sale.CustomerContacts = new[]
                {
                    new Contact()
                    {
                        Method = ContactMethod.Phone,
                        Value = string.Format("contact method phone number {0}", i + 1),
                    },
                    new Contact()
                    {
                        Method = GenerateEnumValue<ContactMethod>(),
                        Value = string.Format("contact method {0}", i + 1),
                    },
                };
                sale.Items = new List<SaleLineItem>();
                var length = random.Next(1, 5);
                for (int j = 0; j < length; j++)
                {
                    var line = new SaleLineItem()
                    {
                        ProductName = GenerateProductName(),
                        ProductDescription = null,
                        Quantity = random.Next(1, 2),
                        UnitPrice = (decimal)((random.NextDouble() + 0.1) * 10000),
                        Status = GenerateEnumValue<SaleStatus>(),
                    };
                    if (j % 2 == 0)
                    {
                        line.DynamicProperties.Add("Certificate", GenerateCertificate());
                        line.DynamicProperties.Add("Cut", GenerateCut());
                        line.DynamicProperties.Add("Caret", GenerateCaret());
                        line.DynamicProperties.Add("Color", GenerateColor());
                        line.DynamicProperties.Add("Clarity", GenerateClarity());
                    }

                    sale.Items.Add(line);
                }

                FillCommonValues(sale, now);

                sales.Add(sale);
            }

            using (var db = new SaleDbContext())
            {
                db.SaleHeaders.AddRange(sales);
                db.SaleLineItems.AddRange(sales.SelectMany(s => s.Items));
                db.Contacts.AddRange(sales.SelectMany(s => s.CustomerContacts));
                db.SaveChanges();
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            using (var db = new SaleDbContext())
            {
                db.Database.ExecuteSqlCommand("DELETE tc_customer_contact");
                db.Database.ExecuteSqlCommand("DELETE tc_sale_line");
                db.Database.ExecuteSqlCommand("DELETE tc_sale_header");
                db.SaveChanges();
            }
        }

        #endregion

        #region Sale - GET

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSales_Expand_All()
        {
            var tokens = ODataClientHelper.GetServerPaging(SalesServiceRootUrl + "Sales?$expand=Items,CustomerContacts");
            var purchases = default(IList<SaleHeader>);
            using (var db = new SaleDbContext())
            {
                purchases = db.SaleHeaders.Include("Items").Include("CustomerContacts").ToArray();
            }

            CompareCollection(purchases, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSales_Expand_Items()
        {
            var tokens = ODataClientHelper.GetServerPaging(SalesServiceRootUrl + "Sales?$expand=Items");
            var purchases = default(IList<SaleHeader>);
            using (var db = new SaleDbContext())
            {
                purchases = db.SaleHeaders.Include("Items").ToArray();
            }

            CompareCollection(purchases, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSales_Expand_CustomerContacts()
        {
            var tokens = ODataClientHelper.GetServerPaging(SalesServiceRootUrl + "Sales?$expand=CustomerContacts");
            var purchases = default(IList<SaleHeader>);
            using (var db = new SaleDbContext())
            {
                purchases = db.SaleHeaders.Include("CustomerContacts").ToArray();
            }

            CompareCollection(purchases, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleById_Expand_All()
        {
            var purchase = GetSale();
            var token = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})?$expand=Items,CustomerContacts", SalesServiceRootUrl, purchase.Id));
            using (var db = new SaleDbContext())
            {
                purchase = db.SaleHeaders.Include("Items").Include("CustomerContacts").First(s => s.Id == purchase.Id);
            }

            Compare(purchase, token);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleById_Expand_Items()
        {
            var purchase = GetSale();
            var token = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})?$expand=Items", SalesServiceRootUrl, purchase.Id));
            using (var db = new SaleDbContext())
            {
                purchase = db.SaleHeaders.Include("Items").First(s => s.Id == purchase.Id);
            }

            Compare(purchase, token);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleById_Expand_CustomerContacts()
        {
            var purchase = GetSale();
            var token = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})?$expand=CustomerContacts", SalesServiceRootUrl, purchase.Id));
            using (var db = new SaleDbContext())
            {
                purchase = db.SaleHeaders.Include("CustomerContacts").First(s => s.Id == purchase.Id);
            }

            Compare(purchase, token);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSalesServerPaging()
        {
            var tokens = ODataClientHelper.GetServerPaging(SalesServiceRootUrl + "Sales");
            var purchases = default(IList<SaleHeader>);
            using (var db = new SaleDbContext())
            {
                purchases = db.SaleHeaders.ToArray();
            }

            CompareCollection(purchases, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSalesFilterByTotalNumber()
        {
            var tokens = ODataClientHelper.GetServerPaging(SalesServiceRootUrl + "Sales?$filter=TotalNumber gt 50");
            var purchases = default(IList<SaleHeader>);
            using (var db = new SaleDbContext())
            {
                purchases = db.SaleHeaders.Where(s => s.TotalNumber > 50).ToArray();
            }

            CompareCollection(purchases, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleById_IfNoneMatch_Match()
        {
            SaleHeader purchase;
            string etag;
            GetSaleAndETag(out purchase, out etag);

            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, purchase.Id),
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                httpResponse =>
                {
                    Assert.AreEqual(HttpStatusCode.NotModified, httpResponse.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleById_IfNoneMatch_NotMatch()
        {
            SaleHeader purchase;
            string etag;
            GetSaleAndETag(out purchase, out etag);
            UpdateSale(purchase.Id);

            var result = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, purchase.Id),
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });
            CompareChanged(purchase, result);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleById_IfNoneMatch_Any()
        {
            var purchase = GetSale();
            var result = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, purchase.Id),
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                });
            Compare(purchase, result);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleById_Ok()
        {
            var purchase = GetSale();
            var result = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, purchase.Id));
            Compare(purchase, result);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleById_NotFound()
        {
            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, Guid.Empty),
                null,
                httpResponse =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, httpResponse.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSalePropertyValue()
        {
            var purchase = GetSale();
            var result = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerName", SalesServiceRootUrl, purchase.Id));
            Assert.AreEqual(purchase.CustomerName, (string)result);
        }

        #endregion

        #region Sale - POST

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PostSale_Sale()
        {
            var location = default(string);

            var token = new JObject();
            token["SalesPersonName"] = "sale person post";
            token["CustomerName"] = "customer post";
            ODataClientHelper.InvokePost(string.Format("{0}Sales", SalesServiceRootUrl),
                token.ToString(Formatting.None),
                null,
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("sale person post", token["SalesPersonName"]);
            Assert.AreEqual("customer post", token["CustomerName"]);
        }

        #endregion

        #region Sale - POST - collection navigation property

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PostSale_Sale_CollectionNavigationProperty()
        {
            var location = default(string);

            var token = new JObject();
            token["SalesPersonName"] = "sale person post";
            token["CustomerName"] = "customer post";
            var contact1 = new JObject();
            contact1["Method"] = "Phone";
            contact1["Value"] = "phone 1";
            var contact2 = new JObject();
            contact2["Method"] = "Phone";
            contact2["Value"] = "phone 2";
            var contacts = new JArray();
            contacts.Add(contact1);
            contacts.Add(contact2);
            token["CustomerContacts"] = contacts;
            var transaction1 = new JObject();
            transaction1["ProductName"] = "product 1";
            transaction1["ProductDescription"] = "description 1";
            transaction1["Quantity"] = 1;
            transaction1["UnitPrice"] = 100000;
            var transaction2 = new JObject();
            transaction2["ProductName"] = "product 2";
            transaction2["ProductDescription"] = "description 2";
            transaction2["Quantity"] = 2;
            transaction2["UnitPrice"] = 200000;
            var transactions = new JArray();
            transactions.Add(transaction1);
            transactions.Add(transaction2);
            token["Items"] = transactions;
            ODataClientHelper.InvokePost(string.Format("{0}Sales", SalesServiceRootUrl),
                token.ToString(Formatting.None),
                null,
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location + "?$expand=CustomerContacts,Items");
            Assert.AreEqual("sale person post", token["SalesPersonName"]);
            Assert.AreEqual("customer post", token["CustomerName"]);
            contacts = (JArray)token["CustomerContacts"];
            Assert.IsTrue(contacts.Any(t => (string)t["Method"] == "Phone" && (string)t["Value"] == "phone 1"));
            Assert.IsTrue(contacts.Any(t => (string)t["Method"] == "Phone" && (string)t["Value"] == "phone 2"));
            transactions = (JArray)token["Items"];
            Assert.IsTrue(transactions.Any(t => (string)t["ProductName"] == "product 1" && (string)t["ProductDescription"] == "description 1" && (int)t["Quantity"] == 1 && (int)t["UnitPrice"] == 100000));
            Assert.IsTrue(transactions.Any(t => (string)t["ProductName"] == "product 2" && (string)t["ProductDescription"] == "description 2" && (int)t["Quantity"] == 2 && (int)t["UnitPrice"] == 200000));
        }

        #endregion

        #region Sale - PUT

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSale_NoneMatch()
        {
            var location = default(string);

            var token = new JObject();
            token["Id"] = Guid.Empty;
            token["SalesPersonName"] = "new sale person";
            token["CustomerName"] = "new customer";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})", SalesServiceRootUrl, Guid.Empty),
                token.ToString(Formatting.None),
                null,
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("new sale person", token["SalesPersonName"]);
            Assert.AreEqual("new customer", token["CustomerName"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSale_IfMatch_Match()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            token["SalesPersonName"] = "new sale person";
            token["CustomerName"] = "new customer";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            Assert.AreEqual("new sale person", token["SalesPersonName"]);
            Assert.AreEqual("new customer", token["CustomerName"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSale_IfMatch_NotMatch()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);
            UpdateSale(entity.Id);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            token["SalesPersonName"] = "new sale person";
            token["CustomerName"] = "new customer";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSale_IfMatch_Any()
        {
            var entity = GetSale();

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            token["SalesPersonName"] = "new sale person";
            token["CustomerName"] = "new customer";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(EntityTagHeaderValue.Any);
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            Assert.AreEqual("new sale person", token["SalesPersonName"]);
            Assert.AreEqual("new customer", token["CustomerName"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSale_IfNoneMatch_Match()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            token["SalesPersonName"] = "new sale person";
            token["CustomerName"] = "new customer";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSale_IfNoneMatch_NotMatch()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);
            UpdateSale(entity.Id);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            token["SalesPersonName"] = "new sale person";
            token["CustomerName"] = "new customer";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            Assert.AreEqual("new sale person", token["SalesPersonName"]);
            Assert.AreEqual("new customer", token["CustomerName"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSale_IfNoneMatch_Any()
        {
            var entity = GetSale();

            var location = default(string);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            token["SalesPersonName"] = "new sale person";
            token["CustomerName"] = "new customer";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                },
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("new sale person", token["SalesPersonName"]);
            Assert.AreEqual("new customer", token["CustomerName"]);
        }

        #endregion

        #region Sale - PUT - collection navigation property

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSale_Sale_CollectionNavigationProperty()
        {
            var location = default(string);

            var token = new JObject();
            token["SalesPersonName"] = "sale person post";
            token["CustomerName"] = "customer post";
            var contact1 = new JObject();
            contact1["Method"] = "Phone";
            contact1["Value"] = "phone 1";
            var contact2 = new JObject();
            contact2["Method"] = "Phone";
            contact2["Value"] = "phone 2";
            var contacts = new JArray();
            contacts.Add(contact1);
            contacts.Add(contact2);
            token["CustomerContacts"] = contacts;
            var transaction1 = new JObject();
            transaction1["ProductName"] = "product 1";
            transaction1["ProductDescription"] = "description 1";
            transaction1["Quantity"] = 1;
            transaction1["UnitPrice"] = 100000;
            var transaction2 = new JObject();
            transaction2["ProductName"] = "product 2";
            transaction2["ProductDescription"] = "description 2";
            transaction2["Quantity"] = 2;
            transaction2["UnitPrice"] = 200000;
            var transactions = new JArray();
            transactions.Add(transaction1);
            transactions.Add(transaction2);
            token["Items"] = transactions;
            ODataClientHelper.InvokePost(string.Format("{0}Sales", SalesServiceRootUrl),
                token.ToString(Formatting.None),
                null,
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location + "?$expand=CustomerContacts,Items");

            token.Remove("@odata.context");
            token.Remove("CustomerContacts@odata.context");
            token.Remove("Items@odata.context");
            token["SalesPersonName"] = "new sale person";
            contacts = (JArray)token["CustomerContacts"];
            contacts[0]["Method"] = "QQ";
            transactions = (JArray)token["Items"];
            transactions[0]["ProductName"] = "new product";
            ODataClientHelper.InvokePut(
                location,
                token.ToString(Formatting.None), client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(EntityTagHeaderValue.Any);
                });

            token = (JObject)ODataClientHelper.InvokeGet(location + "?$expand=CustomerContacts,Items");
            Assert.AreEqual("new sale person", token["SalesPersonName"]);
            contacts = (JArray)token["CustomerContacts"];
            Assert.AreEqual(2, contacts.Count);
            Assert.IsTrue(contacts.Any(t => (string)t["Method"] == "QQ"));
            transactions = (JArray)token["Items"];
            Assert.AreEqual(2, transactions.Count);
            Assert.IsTrue(transactions.Any(t => (string)t["ProductName"] == "new product"));
        }

        #endregion

        #region Sale - PATCH

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSale_NoneMatch()
        {
            var location = default(string);

            var token = new JObject();
            token["Id"] = Guid.Empty;
            token["SalesPersonName"] = "new sale person";
            token["CustomerName"] = "new customer";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})", SalesServiceRootUrl, Guid.Empty),
                token.ToString(Formatting.None),
                null,
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("new sale person", token["SalesPersonName"]);
            Assert.AreEqual("new customer", token["CustomerName"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSale_IfMatch_Match()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            token["SalesPersonName"] = "new sale person";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            Assert.AreEqual("new sale person", token["SalesPersonName"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSale_IfMatch_NotMatch()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);
            UpdateSale(entity.Id);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            token["SalesPersonName"] = "new sale person";
            token["CustomerName"] = "new customer";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSale_IfMatch_Any()
        {
            var entity = GetSale();

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            token["SalesPersonName"] = "new sale person";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(EntityTagHeaderValue.Any);
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            Assert.AreEqual("new sale person", token["SalesPersonName"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSale_IfNoneMatch_Match()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            token["SalesPersonName"] = "new sale person";
            token["CustomerName"] = "new customer";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSale_IfNoneMatch_NotMatch()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);
            UpdateSale(entity.Id);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            token["SalesPersonName"] = "new sale person";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            Assert.AreEqual("new sale person", token["SalesPersonName"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSale_IfNoneMatch_Any()
        {
            var entity = GetSale();

            var location = default(string);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id));
            token["SalesPersonName"] = "new sale person";
            token["CustomerName"] = "new customer";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                },
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("new sale person", token["SalesPersonName"]);
            Assert.AreEqual("new customer", token["CustomerName"]);
        }

        #endregion

        #region Sale - DELETE

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSale_NoneMatch()
        {
            var entity = GetSale();

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                null,
                response =>
                {
                    Assert.AreEqual((HttpStatusCode)428, response.StatusCode);
                    return false;
                });

            Assert.IsNotNull(ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id)));
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSale_IfMatch_Match()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                null,
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSale_IfMatch_NotMatch()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);
            UpdateSale(entity.Id);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });

            Assert.IsNotNull(ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id)));
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSale_IfMatch_Any()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(EntityTagHeaderValue.Any);
                });

            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                null,
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSale_IfNoneMatch_Match()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });

            Assert.IsNotNull(ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id)));
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSale_IfNoneMatch_NotMatch()
        {
            SaleHeader entity;
            string etag;
            GetSaleAndETag(out entity, out etag);
            UpdateSale(entity.Id);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                null,
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSale_IfNoneMatch_Any()
        {
            var entity = GetSale();

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });

            Assert.IsNotNull(ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, entity.Id)));
        }

        #endregion

        #region Item - GET

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleLineItemsServerPaging()
        {
            var transaction = GetSaleLineItem();
            var tokens = ODataClientHelper.GetServerPaging(string.Format("{0}Sales({1})/Items", SalesServiceRootUrl, transaction._SaleHeaderId));
            var transactions = default(IList<SaleLineItem>);
            using (var db = new SaleDbContext())
            {
                transactions = db.SaleLineItems.Where(d => d._SaleHeaderId == transaction._SaleHeaderId).ToArray();
            }

            CompareCollection(transactions, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleLineItemsFilterByProductName()
        {
            var transaction = GetSaleLineItem();
            var tokens = ODataClientHelper.GetServerPaging(string.Format("{0}Sales({1})/Items?$filter=ProductName eq '{2}'", SalesServiceRootUrl, transaction._SaleHeaderId, transaction.ProductName));
            var transactions = default(IList<SaleLineItem>);
            using (var db = new SaleDbContext())
            {
                transactions = db.SaleLineItems.Where(d => d._SaleHeaderId == transaction._SaleHeaderId && d.ProductName == transaction.ProductName).ToArray();
            }

            CompareCollection(transactions, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleLineItemById_IfNoneMatch_Match()
        {
            SaleLineItem transaction;
            string etag;
            GetSaleLineItemAndETag(out transaction, out etag);

            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, transaction._SaleHeaderId, transaction.Id),
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                httpResponse =>
                {
                    Assert.AreEqual(HttpStatusCode.NotModified, httpResponse.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleLineItemById_IfNoneMatch_NotMatch()
        {
            SaleLineItem transaction;
            string etag;
            GetSaleLineItemAndETag(out transaction, out etag);
            UpdateSaleLineItem(transaction.Id);

            var result = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, transaction._SaleHeaderId, transaction.Id),
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });
            CompareChanged(transaction, result);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleLineItemById_IfNoneMatch_Any()
        {
            var transaction = GetSaleLineItem();
            var result = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, transaction._SaleHeaderId, transaction.Id),
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                });
            Compare(transaction, result);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleLineItemById_Ok()
        {
            var transaction = GetSaleLineItem();
            var result = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, transaction._SaleHeaderId, transaction.Id));
            Compare(transaction, result);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleLineItemById_NotFound()
        {
            var transaction = GetSaleLineItem();
            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, transaction._SaleHeaderId, Guid.Empty),
                null,
                httpResponse =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, httpResponse.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetSaleLineItemPropertyValue()
        {
            var transaction = GetSaleLineItem();
            var result = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})/ProductName", SalesServiceRootUrl, transaction._SaleHeaderId, transaction.Id));
            Assert.AreEqual(transaction.ProductName, (string)result);
        }

        #endregion

        #region Item - POST

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PostSaleLineItem()
        {
            var purchase = GetSale();
            var location = default(string);

            var token = new JObject();
            token["ProductName"] = "new product name";
            token["ProductDescription"] = "new product description";
            token["Quantity"] = 2;
            token["UnitPrice"] = 10000M;
            ODataClientHelper.InvokePost(string.Format("{0}Sales({1})/Items", SalesServiceRootUrl, purchase.Id),
                token.ToString(Formatting.None),
                null,
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("new product name", token["ProductName"]);
            Assert.AreEqual("new product description", token["ProductDescription"]);
            Assert.AreEqual(2, token["Quantity"]);
            Assert.AreEqual(10000M, token["UnitPrice"]);
        }

        #endregion

        #region Item - PUT

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSaleLineItem_NoneMatch()
        {
            var entity = GetSaleLineItem();
            var location = default(string);

            var token = new JObject();
            token["Id"] = Guid.Empty;
            token["ProductName"] = "new product name";
            token["ProductDescription"] = "new product description";
            token["Quantity"] = 2;
            token["UnitPrice"] = 10000M;
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, Guid.Empty),
                token.ToString(Formatting.None),
                null,
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("new product name", token["ProductName"]);
            Assert.AreEqual("new product description", token["ProductDescription"]);
            Assert.AreEqual(2, token["Quantity"]);
            Assert.AreEqual(10000M, token["UnitPrice"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSaleLineItem_IfMatch_Match()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["ProductName"] = "new product name";
            token["ProductDescription"] = "new product description";
            token["Quantity"] = 2;
            token["UnitPrice"] = 10000M;
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            Assert.AreEqual("new product name", token["ProductName"]);
            Assert.AreEqual("new product description", token["ProductDescription"]);
            Assert.AreEqual(2, token["Quantity"]);
            Assert.AreEqual(10000M, token["UnitPrice"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSaleLineItem_IfMatch_NotMatch()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);
            UpdateSaleLineItem(entity.Id);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["ProductName"] = "new product name";
            token["ProductDescription"] = "new product description";
            token["Quantity"] = 2;
            token["UnitPrice"] = 10000M;
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSaleLineItem_IfMatch_Any()
        {
            var entity = GetSaleLineItem();

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["ProductName"] = "new product name";
            token["ProductDescription"] = "new product description";
            token["Quantity"] = 2;
            token["UnitPrice"] = 10000M;
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(EntityTagHeaderValue.Any);
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            Assert.AreEqual("new product name", token["ProductName"]);
            Assert.AreEqual("new product description", token["ProductDescription"]);
            Assert.AreEqual(2, token["Quantity"]);
            Assert.AreEqual(10000M, token["UnitPrice"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSaleLineItem_IfNoneMatch_Match()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["ProductName"] = "new product name";
            token["ProductDescription"] = "new product description";
            token["Quantity"] = 2;
            token["UnitPrice"] = 10000M;
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSaleLineItem_IfNoneMatch_NotMatch()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);
            UpdateSaleLineItem(entity.Id);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["ProductName"] = "new product name";
            token["ProductDescription"] = "new product description";
            token["Quantity"] = 2;
            token["UnitPrice"] = 10000M;
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            Assert.AreEqual("new product name", token["ProductName"]);
            Assert.AreEqual("new product description", token["ProductDescription"]);
            Assert.AreEqual(2, token["Quantity"]);
            Assert.AreEqual(10000M, token["UnitPrice"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutSaleLineItem_IfNoneMatch_Any()
        {
            var entity = GetSaleLineItem();
            var location = default(string);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["ProductName"] = "new product name";
            token["ProductDescription"] = "new product description";
            token["Quantity"] = 2;
            token["UnitPrice"] = 10000M;
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                },
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("new product name", token["ProductName"]);
            Assert.AreEqual("new product description", token["ProductDescription"]);
            Assert.AreEqual(2, token["Quantity"]);
            Assert.AreEqual(10000M, token["UnitPrice"]);
        }

        #endregion

        #region Item - PATCH

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSaleLineItem_NoneMatch()
        {
            var purchase = GetSale();
            var location = default(string);

            var token = new JObject();
            token["Id"] = Guid.Empty;
            token["ProductName"] = "new product name";
            token["ProductDescription"] = "new product description";
            token["Quantity"] = 2;
            token["UnitPrice"] = 10000M;
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, purchase.Id, Guid.Empty),
                token.ToString(Formatting.None),
                null,
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("new product name", token["ProductName"]);
            Assert.AreEqual("new product description", token["ProductDescription"]);
            Assert.AreEqual(2, token["Quantity"]);
            Assert.AreEqual(10000M, token["UnitPrice"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSaleLineItem_IfMatch_Match()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["ProductName"] = "new product name";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            Assert.AreEqual("new product name", token["ProductName"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSaleLineItem_IfMatch_NotMatch()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);
            UpdateSaleLineItem(entity.Id);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["ProductName"] = "new product name";
            token["ProductDescription"] = "new product description";
            token["Quantity"] = 2;
            token["UnitPrice"] = 10000M;
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSaleLineItem_IfMatch_Any()
        {
            var entity = GetSaleLineItem();

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["ProductName"] = "new product name";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(EntityTagHeaderValue.Any);
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            Assert.AreEqual("new product name", token["ProductName"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSaleLineItem_IfNoneMatch_Match()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["ProductName"] = "new product name";
            token["ProductDescription"] = "new product description";
            token["Quantity"] = 2;
            token["UnitPrice"] = 10000M;
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSaleLineItem_IfNoneMatch_NotMatch()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);
            UpdateSaleLineItem(entity.Id);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["ProductName"] = "new product name";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            Assert.AreEqual("new product name", token["ProductName"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchSaleLineItem_IfNoneMatch_Any()
        {
            var entity = GetSaleLineItem();

            var location = default(string);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["ProductName"] = "new product name";
            token["ProductDescription"] = "new product description";
            token["Quantity"] = 2;
            token["UnitPrice"] = 10000M;
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                },
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("new product name", token["ProductName"]);
            Assert.AreEqual("new product description", token["ProductDescription"]);
            Assert.AreEqual(2, token["Quantity"]);
            Assert.AreEqual(10000M, token["UnitPrice"]);
        }

        #endregion

        #region Item - DELETE

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSaleLineItem_NoneMatch()
        {
            var entity = GetSaleLineItem();

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                null,
                response =>
                {
                    Assert.AreEqual((HttpStatusCode)428, response.StatusCode);
                    return false;
                });

            Assert.IsNotNull(ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id)));
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSaleLineItem_IfMatch_Match()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                null,
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSaleLineItem_IfMatch_NotMatch()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);
            UpdateSaleLineItem(entity.Id);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });

            Assert.IsNotNull(ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id)));
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSaleLineItem_IfMatch_Any()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(EntityTagHeaderValue.Any);
                });

            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                null,
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSaleLineItem_IfNoneMatch_Match()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });

            Assert.IsNotNull(ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id)));
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSaleLineItem_IfNoneMatch_NotMatch()
        {
            SaleLineItem entity;
            string etag;
            GetSaleLineItemAndETag(out entity, out etag);
            UpdateSaleLineItem(entity.Id);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                null,
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteSaleLineItem_IfNoneMatch_Any()
        {
            var entity = GetSaleLineItem();

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });

            Assert.IsNotNull(ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id)));
        }

        #endregion

        #region Contact - GET

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetContactsServerPaging()
        {
            var contact = GetContact();
            var tokens = ODataClientHelper.GetServerPaging(string.Format("{0}Sales({1})/CustomerContacts", SalesServiceRootUrl, contact._SaleHeaderId));
            var contacts = default(IList<Contact>);
            using (var db = new SaleDbContext())
            {
                contacts = db.Contacts.Where(c => c._SaleHeaderId == contact._SaleHeaderId).ToArray();
            }

            CompareCollection(contacts, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetContactsFilterByMethod()
        {
            var contact = GetContact();
            var tokens = ODataClientHelper.GetServerPaging(string.Format("{0}Sales({1})/CustomerContacts?$filter=Method eq Home.Services.SaleService.ContactMethod'Phone'", SalesServiceRootUrl, contact._SaleHeaderId));
            var contacts = default(IList<Contact>);
            using (var db = new SaleDbContext())
            {
                contacts = db.Contacts.Where(c => c._SaleHeaderId == contact._SaleHeaderId && c.Method == ContactMethod.Phone).ToArray();
            }

            CompareCollection(contacts, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetContactById_IfNoneMatch_Match()
        {
            Contact contact;
            string etag;
            GetContactAndETag(out contact, out etag);

            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, contact._SaleHeaderId, contact.Id),
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                httpResponse =>
                {
                    Assert.AreEqual(HttpStatusCode.NotModified, httpResponse.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetContactById_IfNoneMatch_NotMatch()
        {
            Contact contact;
            string etag;
            GetContactAndETag(out contact, out etag);
            UpdateContact(contact.Id);

            var result = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, contact._SaleHeaderId, contact.Id),
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });
            CompareChanged(contact, result);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetContactById_IfNoneMatch_Any()
        {
            var contact = GetContact();
            var result = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, contact._SaleHeaderId, contact.Id),
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                });
            Compare(contact, result);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetContactById_Ok()
        {
            var contact = GetContact();
            var result = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, contact._SaleHeaderId, contact.Id));
            Compare(contact, result);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetContactById_NotFound()
        {
            var contact = GetContact();
            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, contact._SaleHeaderId, Guid.Empty),
                null,
                httpResponse =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, httpResponse.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetContactPropertyValue()
        {
            var contact = GetContact();
            var result = ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})/Value", SalesServiceRootUrl, contact._SaleHeaderId, contact.Id));
            Assert.AreEqual(contact.Value, (string)result);
        }

        #endregion

        #region Contact - POST

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PostContact_Contact()
        {
            var purchase = GetSale();
            var location = default(string);

            var token = new JObject();
            token["Method"] = "QQ";
            token["Value"] = "new value";
            ODataClientHelper.InvokePost(string.Format("{0}Sales({1})/CustomerContacts", SalesServiceRootUrl, purchase.Id),
                token.ToString(Formatting.None),
                null,
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("QQ", token["Method"]);
            Assert.AreEqual("new value", token["Value"]);
        }

        #endregion

        #region Contact - PUT

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutContact_NoneMatch()
        {
            var entity = GetContact();
            var location = default(string);

            var token = new JObject();
            token["Id"] = Guid.Empty;
            token["Method"] = "QQ";
            token["Value"] = "new value";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, Guid.Empty),
                token.ToString(Formatting.None),
                null,
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("QQ", token["Method"]);
            Assert.AreEqual("new value", token["Value"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutContact_IfMatch_Match()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["Method"] = "QQ";
            token["Value"] = "new value";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            Assert.AreEqual("QQ", token["Method"]);
            Assert.AreEqual("new value", token["Value"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutContact_IfMatch_NotMatch()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);
            UpdateContact(entity.Id);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["Method"] = "QQ";
            token["Value"] = "new value";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutContact_IfMatch_Any()
        {
            var entity = GetContact();

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["Method"] = "QQ";
            token["Value"] = "new value";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(EntityTagHeaderValue.Any);
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            Assert.AreEqual("QQ", token["Method"]);
            Assert.AreEqual("new value", token["Value"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutContact_IfNoneMatch_Match()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["Method"] = "QQ";
            token["Value"] = "new value";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutContact_IfNoneMatch_NotMatch()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);
            UpdateContact(entity.Id);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["Method"] = "QQ";
            token["Value"] = "new value";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            Assert.AreEqual("QQ", token["Method"]);
            Assert.AreEqual("new value", token["Value"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PutContact_IfNoneMatch_Any()
        {
            var entity = GetContact();
            var location = default(string);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["Method"] = "QQ";
            token["Value"] = "new value";
            ODataClientHelper.InvokePut(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                },
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("QQ", token["Method"]);
            Assert.AreEqual("new value", token["Value"]);
        }

        #endregion

        #region Contact - PATCH

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchContact_NoneMatch()
        {
            var purchase = GetSale();
            var location = default(string);

            var token = new JObject();
            token["Id"] = Guid.Empty;
            token["Method"] = "QQ";
            token["Value"] = "new value";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, purchase.Id, Guid.Empty),
                token.ToString(Formatting.None),
                null,
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("QQ", token["Method"]);
            Assert.AreEqual("new value", token["Value"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchContact_IfMatch_Match()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["Method"] = "QQ";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            Assert.AreEqual("QQ", token["Method"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchContact_IfMatch_NotMatch()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);
            UpdateContact(entity.Id);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["Method"] = "QQ";
            token["Value"] = "new value";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchContact_IfMatch_Any()
        {
            var entity = GetContact();

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["Method"] = "QQ";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(EntityTagHeaderValue.Any);
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            Assert.AreEqual("QQ", token["Method"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchContact_IfNoneMatch_Match()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["Method"] = "QQ";
            token["Value"] = "new value";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchContact_IfNoneMatch_NotMatch()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);
            UpdateContact(entity.Id);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["Method"] = "QQ";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            Assert.AreEqual("QQ", token["Method"]);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void PatchContact_IfNoneMatch_Any()
        {
            var entity = GetContact();

            var location = default(string);

            var token = (JObject)ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id));
            token["Method"] = "QQ";
            token["Value"] = "new value";
            ODataClientHelper.InvokePatch(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                token.ToString(Formatting.None),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                },
                response =>
                {
                    location = response.Headers.Location.ToString();
                    return true;
                });

            token = (JObject)ODataClientHelper.InvokeGet(location);
            Assert.AreEqual("QQ", token["Method"]);
            Assert.AreEqual("new value", token["Value"]);
        }

        #endregion

        #region Contact - DELETE

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteContact_NoneMatch()
        {
            var entity = GetContact();

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                null,
                response =>
                {
                    Assert.AreEqual((HttpStatusCode)428, response.StatusCode);
                    return false;
                });

            Assert.IsNotNull(ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id)));
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteContact_IfMatch_Match()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                null,
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteContact_IfMatch_NotMatch()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);
            UpdateContact(entity.Id);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });

            Assert.IsNotNull(ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id)));
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteContact_IfMatch_Any()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfMatch.Add(EntityTagHeaderValue.Any);
                });

            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                null,
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteContact_IfNoneMatch_Match()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });

            Assert.IsNotNull(ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id)));
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteContact_IfNoneMatch_NotMatch()
        {
            Contact entity;
            string etag;
            GetContactAndETag(out entity, out etag);
            UpdateContact(entity.Id);

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });

            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                null,
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void DeleteContact_IfNoneMatch_Any()
        {
            var entity = GetContact();

            ODataClientHelper.InvokeDelete(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id),
                client =>
                {
                    client.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                },
                response =>
                {
                    Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
                    return false;
                });

            Assert.IsNotNull(ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, entity._SaleHeaderId, entity.Id)));
        }

        #endregion

        #region Helpers

        private static void Compare(SaleHeader purchase, JToken token)
        {
            CompareCommon(purchase, token);
            Assert.AreEqual(purchase.DayNumber, (int)token["DayNumber"]);
            Assert.AreEqual(purchase.TotalNumber, (int)token["TotalNumber"]);
            Assert.AreEqual(purchase.NumberText, (string)token["NumberText"]);
            Assert.AreEqual(purchase.SalesPersonName, (string)token["SalesPersonName"]);
            Assert.AreEqual(purchase.Status, (SaleStatus)Enum.Parse(typeof(SaleStatus), (string)token["Status"]));
            Assert.AreEqual(purchase.CustomerName, (string)token["CustomerName"]);
            CompareLazyProperty(() => purchase.Items, token, "Items", Compare);
            CompareLazyProperty(() => purchase.CustomerContacts, token, "CustomerContacts", Compare);
        }

        private static void Compare(SaleLineItem transaction, JToken token)
        {
            CompareCommon(transaction, token);
            Assert.AreEqual(transaction.ProductName, (string)token["ProductName"]);
            Assert.AreEqual(transaction.ProductDescription, (string)token["ProductDescription"]);
            Assert.AreEqual(transaction.Quantity, (int)token["Quantity"]);
            Assert.AreEqual(transaction.UnitPrice, (decimal)token["UnitPrice"]);
            Assert.AreEqual(transaction.Status, (SaleStatus)Enum.Parse(typeof(SaleStatus), (string)token["Status"]));
            CompareOpenProperties(transaction.DynamicProperties, token);
        }

        private static void Compare(Contact contact, JToken token)
        {
            CompareCommon(contact, token);
            Assert.AreEqual(contact.Method, (ContactMethod)Enum.Parse(typeof(ContactMethod), (string)token["Method"]));
            Assert.AreEqual(contact.Value, (string)token["Value"]);
        }

        private static void CompareChanged(SaleHeader purchase, JToken token)
        {
            CompareChangedCommon(purchase, token);
            Assert.AreEqual(purchase.DayNumber, (int)token["DayNumber"]);
            Assert.AreEqual(purchase.TotalNumber, (int)token["TotalNumber"]);
            Assert.AreEqual(purchase.NumberText, (string)token["NumberText"]);
            Assert.AreEqual(purchase.SalesPersonName, (string)token["SalesPersonName"]);
            Assert.AreEqual(purchase.Status, (SaleStatus)Enum.Parse(typeof(SaleStatus), (string)token["Status"]));
            Assert.AreNotEqual(purchase.CustomerName, (string)token["CustomerName"]);
            CompareLazyProperty(() => purchase.Items, token, "Items", Compare);
            CompareLazyProperty(() => purchase.CustomerContacts, token, "CustomerContacts", Compare);
        }

        private static void CompareChanged(SaleLineItem transaction, JToken token)
        {
            CompareChangedCommon(transaction, token);
            Assert.AreEqual(transaction.ProductName, (string)token["ProductName"]);
            Assert.AreNotEqual(transaction.ProductDescription, (string)token["ProductDescription"]);
            Assert.AreEqual(transaction.Quantity, (int)token["Quantity"]);
            Assert.AreEqual(transaction.UnitPrice, (decimal)token["UnitPrice"]);
            Assert.AreEqual(transaction.Status, (SaleStatus)Enum.Parse(typeof(SaleStatus), (string)token["Status"]));
            CompareOpenProperties(transaction.DynamicProperties, token);
        }

        private static void CompareChanged(Contact contact, JToken token)
        {
            CompareChangedCommon(contact, token);
            Assert.AreEqual(contact.Method, (ContactMethod)Enum.Parse(typeof(ContactMethod), (string)token["Method"]));
            Assert.AreNotEqual(contact.Value, (string)token["Value"]);
        }

        private static SaleHeader GetSale()
        {
            using (var db = new SaleDbContext())
            {
                return db.SaleHeaders.First();
            }
        }

        private static SaleLineItem GetSaleLineItem()
        {
            using (var db = new SaleDbContext())
            {
                return db.SaleLineItems.First();
            }
        }

        private static Contact GetContact()
        {
            using (var db = new SaleDbContext())
            {
                return db.Contacts.First();
            }
        }

        private static void GetSaleAndETag(out SaleHeader purchase, out string etag)
        {
            purchase = GetSale();
            var tag = default(string);
            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})", SalesServiceRootUrl, purchase.Id),
                null,
                response =>
                {
                    var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var obj = JObject.Parse(json);
                    tag = (string)obj["@odata.etag"];
                    return false;
                });
            etag = tag;
        }

        private static void GetSaleLineItemAndETag(out SaleLineItem transaction, out string etag)
        {
            transaction = GetSaleLineItem();
            var tag = default(string);
            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/Items({2})", SalesServiceRootUrl, transaction._SaleHeaderId, transaction.Id),
                null,
                response =>
                {
                    var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var obj = JObject.Parse(json);
                    tag = (string)obj["@odata.etag"];
                    return false;
                });
            etag = tag;
        }

        private static void GetContactAndETag(out Contact contact, out string etag)
        {
            contact = GetContact();
            var tag = default(string);
            ODataClientHelper.InvokeGet(string.Format("{0}Sales({1})/CustomerContacts({2})", SalesServiceRootUrl, contact._SaleHeaderId, contact.Id),
                null,
                response =>
                {
                    var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var obj = JObject.Parse(json);
                    tag = (string)obj["@odata.etag"];
                    return false;
                });
            etag = tag;
        }

        private static void UpdateSale(Guid id)
        {
            using (var db = new SaleDbContext())
            {
                var purchase = db.SaleHeaders.Find(id);
                purchase.CustomerName += " changed";
                purchase.Modified = DateTimeOffset.UtcNow;
                purchase.ModifiedBy = "test";
                db.SaveChanges();
            }
        }

        private static void UpdateSaleLineItem(Guid id)
        {
            using (var db = new SaleDbContext())
            {
                var transaction = db.SaleLineItems.Find(id);
                transaction.ProductDescription += " changed";
                transaction.Modified = DateTimeOffset.UtcNow;
                transaction.ModifiedBy = "test";
                db.SaveChanges();
            }
        }

        private static void UpdateContact(Guid id)
        {
            using (var db = new SaleDbContext())
            {
                var contact = db.Contacts.Find(id);
                contact.Value += " changed";
                contact.Modified = DateTimeOffset.UtcNow;
                contact.ModifiedBy = "test";
                db.SaveChanges();
            }
        }

        private static string GenerateSalesPersonName(int seed)
        {
            if (seed % 2 == 0) return "lvleiyan";
            else if (seed % 3 == 0) return "xiaoji";
            return "guyanpin";
        }

        private static string GenerateProductName()
        {
            var random = new Random(DateTime.UtcNow.Millisecond);
            var values = new[] { "裸钻", "女戒", "男戒", "对戒", "吊坠", "项链", "手链", "耳钉", "翡翠", "碧玺", "男戒托", "女戒托", "黄金", "铂金" };
            return values[random.Next(0, values.Length - 1)];
        }

        private static object GenerateCertificate()
        {
            var random = new Random(DateTime.UtcNow.Millisecond);
            return (random.NextDouble() * 1234567890).ToString();
        }

        private static object GenerateClarity()
        {
            var random = new Random(DateTime.UtcNow.Millisecond);
            var values = new[] { "EX", "VG", "G", "P" };
            return values[random.Next(0, values.Length - 1)];
        }

        private static object GenerateColor()
        {
            var random = new Random(DateTime.UtcNow.Millisecond);
            var values = new[] { "D", "E", "F", "G", "H", "I", "J" };
            return values[random.Next(0, values.Length - 1)];
        }

        private static object GenerateCaret()
        {
            var random = new Random(DateTime.UtcNow.Millisecond);
            return (random.NextDouble() * random.Next(1, 10)).ToString("#.##");
        }

        private static object GenerateCut()
        {
            var random = new Random(DateTime.UtcNow.Millisecond);
            var values = new[] { "EX", "VG", "G", "P" };
            return values[random.Next(0, values.Length - 1)];
        }

        private static void FillCommonValues(SaleHeader purchase, DateTimeOffset time)
        {
            FillEntityCommonValues(purchase, time);

            foreach (var contact in purchase.CustomerContacts)
            {
                FillEntityCommonValues(contact, time);
                contact._SaleHeaderId = purchase.Id;
            }

            foreach (var transaction in purchase.Items)
            {
                FillEntityCommonValues(transaction, time);
                transaction._SaleHeaderId = purchase.Id;
            }
        }

        #endregion
    }
}
