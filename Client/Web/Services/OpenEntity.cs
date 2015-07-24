namespace Home.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class OpenEntity : Entity
    {
        private IDictionary<string, object> dynamicProperties;

        public IDictionary<string, object> DynamicProperties
        {
            get
            {
                if (this.dynamicProperties == null)
                {
                    this.dynamicProperties = new Dictionary<string, object>();
                }

                return this.dynamicProperties;
            }
            set { this.dynamicProperties = value; }
        }

        public string _DynamicProperties
        {
            get { return this.dynamicProperties.ToRepositoryDynamicProperties(); }
            set { this.dynamicProperties = value.ToServiceDynamicProperties(); }
        }
    }
}
