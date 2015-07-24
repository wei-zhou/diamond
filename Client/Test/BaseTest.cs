namespace Home.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Home.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json.Linq;

    public abstract class BaseTest
    {
        public TestContext TestContext { get; set; }

        protected static DateTimeOffset GenerateFakeNow()
        {
            var random = new Random(DateTime.UtcNow.Millisecond);
            var interval = random.Next(-365, 0);
            return DateTimeOffset.UtcNow.AddDays(interval);
        }

        protected static T GenerateEnumValue<T>()
        {
            var random = new Random(DateTime.UtcNow.Millisecond);
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(random.Next(0, values.Length - 1));
        }

        protected static void FillEntityCommonValues(Entity entity, DateTimeOffset time)
        {
            Assert.IsNotNull(entity);

            entity.Id = Guid.NewGuid();
            entity.Created = time;
            entity.CreatedBy = "test";
            entity.Modified = time;
            entity.ModifiedBy = "test";
        }

        protected static void CompareCommon(Entity entity, JToken token)
        {
            Assert.IsNotNull(entity);
            Assert.IsNotNull(token);

            Assert.AreEqual(entity.Id, (Guid)token["Id"]);
            Assert.AreEqual(entity.Created, (DateTimeOffset)token["Created"]);
            Assert.AreEqual(entity.CreatedBy, (string)token["CreatedBy"]);
            Assert.AreEqual(entity.Modified, (DateTimeOffset)token["Modified"]);
            Assert.AreEqual(entity.ModifiedBy, (string)token["ModifiedBy"]);
            CollectionAssert.AreEqual(entity.RowVersion, (byte[])token["RowVersion"]);
        }

        protected static void CompareChangedCommon(Entity entity, JToken token)
        {
            Assert.IsNotNull(entity);
            Assert.IsNotNull(token);

            Assert.AreEqual(entity.Id, (Guid)token["Id"]);
            Assert.AreEqual(entity.Created, (DateTimeOffset)token["Created"]);
            Assert.AreEqual(entity.CreatedBy, (string)token["CreatedBy"]);
            Assert.AreNotEqual(entity.Modified, (DateTimeOffset)token["Modified"]);
            Assert.AreEqual(entity.ModifiedBy, (string)token["ModifiedBy"]);
            CollectionAssert.AreNotEqual(entity.RowVersion, (byte[])token["RowVersion"]);
        }

        protected static void CompareCollection<TEntity>(IEnumerable<TEntity> entities, IEnumerable<JToken> tokens, Action<TEntity, JToken> comparer)
            where TEntity : Entity
        {
            Assert.IsNotNull(entities);
            Assert.IsNotNull(tokens);

            Assert.AreEqual(entities.Count(), tokens.Count());
            foreach (var entity in entities)
            {
                var token = tokens.Single(t => (Guid)t["Id"] == entity.Id);
                comparer(entity, token);
            }
        }

        protected static void CompareOpenProperties(IDictionary<string, object> properties, JToken token)
        {
            Assert.IsNotNull(properties);
            Assert.IsNotNull(token);

            foreach (var key in properties.Keys)
            {
                Assert.AreEqual(properties[key].ToString(), (string)token[key]);
            }
        }

        protected static void CompareLazyProperty<TEntity>(Func<IEnumerable<TEntity>> getProperty, JToken token, string propertyName, Action<TEntity, JToken> comparer)
            where TEntity : Entity
        {
            Assert.IsNotNull(comparer);

            var property = default(IEnumerable<TEntity>);
            var isNull = false;
            try
            {
                property = getProperty();
            }
            catch (ObjectDisposedException)
            {
                isNull = true;
            }

            var array = (JArray)token[propertyName];
            if (isNull)
            {
                Assert.IsNull(array);
            }
            else
            {
                CompareCollection(property, array, comparer);
            }
        }

        protected static T? ConvertToEnum<T>(JToken token)
            where T : struct
        {
            var text = (string)token;
            var value = (T)Enum.Parse(typeof(T), text);
            return text == null ? new T?() : new T?(value);
        }
    }
}
