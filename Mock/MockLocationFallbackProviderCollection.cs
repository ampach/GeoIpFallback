using System.Configuration.Provider;
using Sitecore.Diagnostics;

namespace GeoIpFallback.Mock
{
    public class MockLocationFallbackProviderCollection : ProviderCollection
    {
        public virtual MockLocationFallbackProviderBase this[string name]
        {
            get
            {
                Assert.ArgumentNotNull((object)name, nameof(name));
                return base[name] as MockLocationFallbackProviderBase;
            }
        }

        public override void Add(ProviderBase provider)
        {
            Assert.ArgumentNotNull((object)provider, nameof(provider));
            MockLocationFallbackProviderBase provider1 = provider as MockLocationFallbackProviderBase;
            Assert.IsNotNull((object)provider1, "The provider type passed to MockLocationFallbackProviderCollection is not assignable to MockLocationFallbackProviderBase. Actual type: {0}", (object)provider.GetType().FullName);
            this.Add(provider1);
        }

        public virtual void Add(MockLocationFallbackProviderBase provider)
        {
            Assert.ArgumentNotNull((object)provider, nameof(provider));
            base.Add((ProviderBase)provider);
        }

    }
}
