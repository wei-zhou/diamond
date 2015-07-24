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
    using Home.Services.SecurityService;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class SecurityServiceE2ETest : BaseTest
    {
        private static readonly string SecurityServiceRootUrl = ConfigurationManager.AppSettings["SecurityServiceRootUrl"];

        [TestInitialize]
        public void TestInitialize()
        {
            TestCleanup();

            using (var db = new SecurityDbContext())
            {
                for (int i = 0; i < 108; i++)
                {
                    var user = new User()
                    {
                        Role = GenerateEnumValue<Role>(),
                        LoginName = string.Format("login-name-{0}", i + 1),
                        DisplayName = string.Format("display-name-{0}", i + 1),
                        _Password = string.Format("password-{0}", i + 1),
                        _IsLocked = (i % 3 == 0),
                        _LockDateTimeUtc = DateTimeOffset.UtcNow
                    };
                    FillEntityCommonValues(user, DateTimeOffset.UtcNow);

                    db.Users.Add(user);
                }

                db.SaveChanges();
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            using (var db = new SecurityDbContext())
            {
                db.Database.ExecuteSqlCommand("DELETE tc_employee WHERE created_by <> N'system'");
                db.SaveChanges();
            }
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetUsersServerPaging()
        {
            var tokens = ODataClientHelper.GetServerPaging(SecurityServiceRootUrl + "Users");
            var users = default(IList<User>);
            using (var db = new SecurityDbContext())
            {
                users = db.Users.Where(u => !u._IsLocked).ToArray();
            }

            CompareCollection(users, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetUsersFilterByRole()
        {
            var tokens = ODataClientHelper.GetServerPaging(SecurityServiceRootUrl + "Users?$filter=Role eq Home.Services.SecurityService.Role'Administrator'");
            var users = default(IList<User>);
            using (var db = new SecurityDbContext())
            {
                users = db.Users.Where(u => !u._IsLocked && u.Role == Role.Administrator).ToArray();
            }

            CompareCollection(users, tokens, Compare);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetUserById_IfNoneMatch_Match()
        {
            User user;
            string etag;
            GetUserAndETag(out user, out etag);

            ODataClientHelper.InvokeGet(string.Format("{0}Users({1})", SecurityServiceRootUrl, user.Id),
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
        public void GetUserById_IfNoneMatch_NotMatch()
        {
            User user;
            string etag;
            GetUserAndETag(out user, out etag);
            UpdateUser(user.Id);

            var result = ODataClientHelper.InvokeGet(string.Format("{0}Users({1})", SecurityServiceRootUrl, user.Id),
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.IfNoneMatch.Add(new EntityTagHeaderValue(etag.Substring(2), true));
                });
            CompareChanged(user, result);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetUserById_IfNoneMatch_Any()
        {
            var user = GetUser();
            var result = ODataClientHelper.InvokeGet(string.Format("{0}Users({1})", SecurityServiceRootUrl, user.Id),
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.IfNoneMatch.Add(EntityTagHeaderValue.Any);
                });
            Compare(user, result);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetUserById_Ok()
        {
            var user = GetUser();
            var result = ODataClientHelper.InvokeGet(string.Format("{0}Users({1})", SecurityServiceRootUrl, user.Id));
            Compare(user, result);
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetUserById_NotFound()
        {
            ODataClientHelper.InvokeGet(string.Format("{0}Users({1})", SecurityServiceRootUrl, Guid.Empty),
                null,
                httpResponse =>
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, httpResponse.StatusCode);
                    return false;
                });
        }

        [TestMethod]
        [TestCategory("E2ETest")]
        public void GetUserPropertyValue()
        {
            var user = GetUser();
            var result = ODataClientHelper.InvokeGet(string.Format("{0}Users({1})/LoginName", SecurityServiceRootUrl, user.Id));
            Assert.AreEqual(user.LoginName, (string)result);
        }

        private static void Compare(User user, JToken token)
        {
            CompareCommon(user, token);
            Assert.AreEqual(user.LoginName, (string)token["LoginName"]);
            Assert.AreEqual(user.DisplayName, (string)token["DisplayName"]);
            Assert.AreEqual(user.Role, (Role)Enum.Parse(typeof(Role), (string)token["Role"]));
        }

        private static void CompareChanged(User user, JToken token)
        {
            CompareChangedCommon(user, token);
            Assert.AreEqual(user.LoginName, (string)token["LoginName"]);
            Assert.AreNotEqual(user.DisplayName, (string)token["DisplayName"]);
            Assert.AreEqual(user.Role, (Role)Enum.Parse(typeof(Role), (string)token["Role"]));
        }

        private static User GetUser()
        {
            using (var db = new SecurityDbContext())
            {
                return db.Users.First(u => !u._IsLocked);
            }
        }

        private static void GetUserAndETag(out User user, out string etag)
        {
            user = GetUser();
            var tag = default(string);
            ODataClientHelper.InvokeGet(string.Format("{0}Users({1})", SecurityServiceRootUrl, user.Id),
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

        private static void UpdateUser(Guid id)
        {
            using (var db = new SecurityDbContext())
            {
                var user = db.Users.Find(id);
                user.DisplayName += " changed";
                user.Modified = DateTimeOffset.UtcNow;
                user.ModifiedBy = "test";
                db.SaveChanges();
            }
        }
    }
}
