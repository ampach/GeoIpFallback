using System.Configuration.Provider;
using Sitecore.Diagnostics;

namespace GeoIpFallback.Providers
{
    public class LocationFallbackProviderCollection : ProviderCollection
    {
        public virtual LocationFallbackProviderBase this[string name]
    {
      get
      {
        Assert.ArgumentNotNull((object) name, nameof (name));
        return base[name] as LocationFallbackProviderBase;
      }
    }

    public override void Add(ProviderBase provider)
    {
      Assert.ArgumentNotNull((object) provider, nameof (provider));
        LocationFallbackProviderBase provider1 = provider as LocationFallbackProviderBase;
      Assert.IsNotNull((object) provider1, "The provider type passed to LocationProviderCollection is not assignable to LocationProviderBase. Actual type: {0}", (object) provider.GetType().FullName);
      this.Add(provider1);
    }

    public virtual void Add(LocationFallbackProviderBase provider)
    {
      Assert.ArgumentNotNull((object) provider, nameof (provider));
      base.Add((ProviderBase) provider);
    }

    }
}
