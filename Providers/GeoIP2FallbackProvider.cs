using System;
using System.Net;
using MaxMind.GeoIP2;
using Sitecore.Analytics.Model;
using Sitecore.Configuration;

namespace GeoIpFallback.Providers
{
    public class GeoIP2FallbackProvider : LocationFallbackProviderBase
    {
        public override WhoIsInformation Resolve(IPAddress ip)
        {
            var geoLite2Path = Settings.GetSetting("GeoRedirect.DatabaseLocation.GeoLite2City", "~/app_data/GeoLite2-City.mmdb");
            var whoIsInformation = new WhoIsInformation();

            using (var reader = new DatabaseReader(geoLite2Path))
            {
                var city = reader.City(ip);

                if (city != null)
                {
                    Sitecore.Diagnostics.Log.Info("GeoIPFallback: current location was resolved by local MaxMind database.", this);

                    whoIsInformation.Country = city.Country.IsoCode;
                    Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Country: " + whoIsInformation.Country, this);

                    whoIsInformation.City = city.City.Name;
                    Sitecore.Diagnostics.Log.Debug("GeoIPFallback: City: " + whoIsInformation.City, this);

                    whoIsInformation.PostalCode = city.Postal.Code;
                    Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Postal Code: " + whoIsInformation.PostalCode, this);

                    whoIsInformation.Latitude = city.Location.Latitude;
                    Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Latitude: " + whoIsInformation.Latitude, this);

                    whoIsInformation.Longitude = city.Location.Longitude;
                    Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Longitude: " + whoIsInformation.Longitude, this);

                    whoIsInformation.MetroCode = city.MostSpecificSubdivision.Name;
                    Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Metro Code: " + whoIsInformation.MetroCode, this);

                    whoIsInformation.AreaCode = city.MostSpecificSubdivision.IsoCode;
                    Sitecore.Diagnostics.Log.Debug("GeoIPFallback: Area Code: " + whoIsInformation.AreaCode, this);

                }
                else
                {
                    Sitecore.Diagnostics.Log.Info(
                        "GeoIPFallback: current location was not resolved by local MaxMind database.", this);
                    whoIsInformation.BusinessName = "Not Available";
                }
            }

            return whoIsInformation;
        }
    }
}