using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoIpFallback.Providers
{
    public static class LocationFallbackManager
    {
        private static readonly Sitecore.Configuration.ProviderHelper<LocationFallbackProviderBase, LocationFallbackProviderCollection> ProviderHelper = new Sitecore.Configuration.ProviderHelper<LocationFallbackProviderBase, LocationFallbackProviderCollection>("locationFallbackManager");

        static LocationFallbackManager()
        {
            
        }

        public static LocationFallbackProviderBase LocationFallbackProvider
        {
            get
            {
                return LocationFallbackManager.ProviderHelper.Provider;
            }
        }
    }
}
