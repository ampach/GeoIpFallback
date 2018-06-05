using System.Configuration.Provider;

namespace GeoIpFallback.Mock
{
    public abstract class MockLocationFallbackProviderBase : ProviderBase
    {
        public abstract Sitecore.Analytics.Model.WhoIsInformation GetMockCurrentLocation();
    }
}
