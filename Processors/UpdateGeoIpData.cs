using System;
using System.Net;
using System.Web.Hosting;
using MaxMind.GeoIP;
using Sitecore.Analytics;
using Sitecore.Analytics.Lookups;
using Sitecore.Analytics.Model;
using Sitecore.Analytics.Pipelines.StartTracking;
using Sitecore.Configuration;

namespace GeoIpFallback.Processors
{
    public class UpdateGeoIpData : StartTrackingProcessor
    {
        public override void Process(StartTrackingArgs args)
        {
            Sitecore.Diagnostics.Assert.IsNotNull(Tracker.Current, "Tracker.Current is not initialized");
            Sitecore.Diagnostics.Assert.IsNotNull(Tracker.Current.Session, "Tracker.Current.Session is not initialized");

            if (Tracker.Current.Session.Interaction == null)
                return;

            var ip = GeoIpManager.IpHashProvider.ResolveIpAddress(Tracker.Current.Session.Interaction.Ip);
            var stringIp = ip.ToString();

            if (Tracker.Current.Session.Interaction.CustomValues.ContainsKey(stringIp) && UpdateGeoIpDataOverrided(ip))
            {
                Tracker.Current.Session.Interaction.CustomValues.Remove(stringIp);
                return;
            }

            if (!Tracker.Current.Session.Interaction.UpdateGeoIpData())
            {
                var whoIsInformation = new WhoIsInformation();

                var geoLitePath = Settings.GetSetting("GeoRedirect.DatabaseLocation.GeoLiteCity", "~/app_data/GeoLiteCity.dat");

                var lookUpService = new LookupService(HostingEnvironment.MapPath(geoLitePath), LookupService.GEOIP_STANDARD);
                var location = lookUpService.getLocation(ip);

                Sitecore.Diagnostics.Log.Info("UpdateGeoIpData: Local MaxMind database was requested. IP: " + stringIp, this);

                if (location != null)
                {
                    Sitecore.Diagnostics.Log.Info("UpdateGeoIpData: current location was resolved by local MaxMind database.", this);
                    whoIsInformation.Country = location.countryCode ?? string.Empty;
                    whoIsInformation.Region = location.regionName ?? string.Empty;
                    whoIsInformation.City = location.city ?? string.Empty;
                    whoIsInformation.PostalCode = location.postalCode ?? string.Empty;
                    whoIsInformation.Latitude = location.latitude;
                    whoIsInformation.Longitude = location.longitude;
                    whoIsInformation.MetroCode = location.metro_code <= 0 ? string.Empty : location.metro_code.ToString();
                    whoIsInformation.AreaCode = location.area_code <= 0 ? string.Empty : location.area_code.ToString();
                }
                else
                {
                    Sitecore.Diagnostics.Log.Info("UpdateGeoIpData: current location was not resolved by local MaxMind database.", this);
                    whoIsInformation.BusinessName = "Not Available";
                }

                Tracker.Current.Session.Interaction.SetGeoData(whoIsInformation);
                Tracker.Current.Session.Interaction.CustomValues.Add(stringIp, whoIsInformation);
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