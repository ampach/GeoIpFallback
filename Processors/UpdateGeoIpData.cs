﻿using System;
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
                Sitecore.Diagnostics.Log.Debug("GeoIPFallback: the fallback version is overrided by data from Sitecore GEO IP service.", this);
                Tracker.Current.Session.Interaction.CustomValues.Remove(stringIp);
                return;
            }

            if (!Tracker.Current.Session.Interaction.UpdateGeoIpData())
            {
                try
                {
                    Sitecore.Diagnostics.Log.Info("GeoIPFallback: Current location was not resolved by Sitecore GEO IP service; Local MaxMind database is requested. IP: " + stringIp, this);

                    var whoIsInformation = new WhoIsInformation();

                    var geoLitePath = Settings.GetSetting("GeoRedirect.DatabaseLocation.GeoLiteCity", "~/app_data/GeoLiteCity.dat");

                    var lookUpService = new LookupService(HostingEnvironment.MapPath(geoLitePath), LookupService.GEOIP_STANDARD);
                    var location = lookUpService.getLocation(ip);

                    

                    if (location != null)
                    {
                        Sitecore.Diagnostics.Log.Info("GeoIPFallback: current location was resolved by local MaxMind database.", this);

                        whoIsInformation.Country = location.countryCode ?? string.Empty;
                        Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Country: " + whoIsInformation.Country, this);

                        whoIsInformation.Region = location.regionName ?? string.Empty;
                        Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Region: " + whoIsInformation.Region, this);

                        whoIsInformation.City = location.city ?? string.Empty;
                        Sitecore.Diagnostics.Log.Debug("GeoIPFallback: City: " + whoIsInformation.City, this);

                        whoIsInformation.PostalCode = location.postalCode ?? string.Empty;
                        Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Postal Code: " + whoIsInformation.PostalCode, this);

                        whoIsInformation.Latitude = location.latitude;
                        Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Latitude: " + whoIsInformation.Latitude, this);

                        whoIsInformation.Longitude = location.longitude;
                        Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Longitude: " + whoIsInformation.Longitude, this);

                        whoIsInformation.MetroCode = location.metro_code <= 0 ? string.Empty : location.metro_code.ToString();
                        Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Metro Code: " + whoIsInformation.MetroCode, this);

                        whoIsInformation.AreaCode = location.area_code <= 0 ? string.Empty : location.area_code.ToString();
                        Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Area Code: " + whoIsInformation.AreaCode, this);

                    }
                    else
                    {
                        Sitecore.Diagnostics.Log.Info(
                            "GeoIPFallback: current location was not resolved by local MaxMind database.", this);
                        whoIsInformation.BusinessName = "Not Available";
                    }

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