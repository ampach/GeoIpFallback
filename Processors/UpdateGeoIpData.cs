using System;
using System.Net;
using System.Web.Hosting;
using GeoIpFallback.Mock;
using GeoIpFallback.Providers;
using MaxMind.GeoIP;
using MaxMind.GeoIP2;
using Sitecore.Analytics;
using Sitecore.Analytics.Lookups;
using Sitecore.Analytics.Model;
using Sitecore.Analytics.Pipelines.StartTracking;
using Sitecore.Configuration;

namespace GeoIpFallback.Processors
{
    public class UpdateGeoIpData : StartTrackingProcessor
    {
        private const string CustomValuesKey = "GeoRedirect.Mocks.Enabled";

        private bool IsMockEnabled
        {
            get
            {
                if (Tracker.Current.Session.Interaction.CustomValues.ContainsKey(CustomValuesKey))
                {
                    return (bool)Tracker.Current.Session.Interaction.CustomValues[CustomValuesKey];
                }

                var isEnabled = Settings.GetBoolSetting("GeoRedirect.Mocks.Enabled", false);

                Tracker.Current.Session.Interaction.CustomValues.Add(CustomValuesKey, isEnabled);

                return isEnabled;
            }
        }

        public override void Process(StartTrackingArgs args)
        {
            Sitecore.Diagnostics.Assert.IsNotNull(Tracker.Current, "Tracker.Current is not initialized");
            Sitecore.Diagnostics.Assert.IsNotNull(Tracker.Current.Session, "Tracker.Current.Session is not initialized");

            if (Tracker.Current.Session.Interaction == null)
                return;

            if (IsMockEnabled)
            {
                var mock = MockLocationFallbackManager.MockLocationFallbackProvider.GetMockCurrentLocation();
            }

            

            var ip = GeoIpManager.IpHashProvider.ResolveIpAddress(Tracker.Current.Session.Interaction.Ip);
            var stringIp = ip.ToString();

            if (Tracker.Current.Session.Interaction.CustomValues.ContainsKey(stringIp) && UpdateGeoIpDataOverrided(ip))
            {
                Sitecore.Diagnostics.Log.Debug("GeoIPFallback: the fallback version is overrided by data from Sitecore GEO IP service.", this);
                Tracker.Current.Session.Interaction.CustomValues.Remove(stringIp);
                return;
            }

            if (!Tracker.Current.Session.Interaction.UpdateGeoIpData())
            {
                try
                {
                    Sitecore.Diagnostics.Log.Info("GeoIPFallback: Current location was not resolved by Sitecore GEO IP service; Local MaxMind database is requested. IP: " + stringIp, this);

                    var whoIsInformation = LocationFallbackManager.LocationFallbackProvider.Resolve(ip);

                    Tracker.Current.Session.Interaction.SetGeoData(whoIsInformation);
                    Tracker.Current.Session.Interaction.CustomValues.Add(stringIp, whoIsInformation);
                }
                catch (Exception ex)
                {
                    Sitecore.Diagnostics.Log.Error("UpdateGeoIpData: Something was wrong.", this);
                    Sitecore.Diagnostics.Log.Error("Exception:", ex, this);
                }
            }
        }

        private bool UpdateGeoIpDataOverrided(IPAddress ip)
        {
            return UpdateGeoIpDataOverrided(new TimeSpan(0, 0, 0, 0, 0), ip);
        }

        private bool UpdateGeoIpDataOverrided(TimeSpan timeout, IPAddress ip)
        {
            GeoIpResult geoIpData = GeoIpManager.GetGeoIpData(new GeoIpOptions()
            {
                Ip = ip,
                Id = GeoIpManager.IpHashProvider.ComputeGuid(ip),
                MillisecondsTimeout = timeout == TimeSpan.MaxValue ? -1 : (int)Math.Min(timeout.TotalMilliseconds, int.MaxValue)
            });
            if (geoIpData.ResolveState != GeoIpResolveState.Resolved || geoIpData.GeoIpData == null)
                return false;
            Tracker.Current.Session.Interaction.SetGeoData(geoIpData.GeoIpData);
            Tracker.Current.Session.Interaction.UpdateLocationReference();
            return true;
        }
    }
}