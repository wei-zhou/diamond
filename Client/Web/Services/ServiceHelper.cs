namespace Home.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.OData.Builder;
    using System.Xml;
    using System.Xml.Linq;

    public static class ServiceHelper
    {
        public static IDictionary<string, object> ToServiceDynamicProperties(this string repository)
        {
            var result = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(repository))
            {
                var xmlDoc = XDocument.Parse(repository, LoadOptions.None);
                foreach (var xmlElementProperty in xmlDoc.Root.Elements("property"))
                {
                    var key = xmlElementProperty.Attribute("name").Value;
                    var value = xmlElementProperty.Value;
                    result.Add(key, value);
                }
            }

            return result;
        }

        public static string ToRepositoryDynamicProperties(this IDictionary<string, object> service)
        {
            var xmlDoc = new XDocument(new XElement("properties"));

            if (service != null)
            {
                foreach (var item in service)
                {
                    var xmlElementProperty = new XElement("property", item.Value);
                    xmlElementProperty.Add(new XAttribute("name", item.Key));
                    xmlDoc.Root.Add(xmlElementProperty);
                }
            }

            return xmlDoc.ToString(SaveOptions.DisableFormatting);
        }

        public static void ConfigureCommonProperties<T>(this EntityTypeConfiguration<T> config)
            where T : Entity
        {
            Debug.Assert(config != null);

            // TODO: the metadata nullable is true, fix it
            config.Property(c => c.CreatedBy).IsRequired();
            config.Property(c => c.ModifiedBy).IsRequired();
        }
    }
}
