using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoIpFallback.Mock
{
    public static class MockLocationFallbackManager
    {
        private static readonly Sitecore.Configuration.ProviderHelper<MockLocationFallbackProviderBase, MockLocationFallbackProviderCollection> ProviderHelper = 
            new Sitecore.Configuration.ProviderHelper<MockLocationFallbackProviderBase, MockLocationFallbackProviderCollection>("mockLocationFallbackManager");

        static MockLocationFallbackManager()
        {

        }

        public static MockLocationFallbackProviderBase MockLocationFallbackProvider
        {
            get
            {
                return MockLocationFallbackManager.ProviderHelper.Provider;
            }
        }
    }
}
