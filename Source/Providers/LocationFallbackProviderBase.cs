using System.Configuration.Provider;
using System.Net;

namespace GeoIpFallback.Providers
{
    public abstract class LocationFallbackProviderBase : ProviderBase
    {
        public abstract Sitecore.Analytics.Model.WhoIsInformation Resolve(IPAddress ip);
    }
}