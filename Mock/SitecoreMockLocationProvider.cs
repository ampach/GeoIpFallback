using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Analytics.Model;

namespace GeoIpFallback.Mock
{
    public class SitecoreMockLocationProvider : MockLocationFallbackProviderBase
    {
        private string _database;
        public override void Initialize(string name, NameValueCollection config)
        {
            string database = config["database"];
            if (!string.IsNullOrEmpty(database))
            {
                this._database = database;
            }
            else
            {
                this._database = "master";
            }
            base.Initialize(name, config);
        }

        public override WhoIsInformation GetMockCurrentLocation()
        {
            return new WhoIsInformation();
        }
    }
}
