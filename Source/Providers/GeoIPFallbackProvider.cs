using System;
using System.Collections.Specialized;
using System.Net;
using System.Web.Hosting;
using MaxMind.GeoIP;
using Sitecore.Analytics.Model;
using Sitecore.Configuration;

namespace GeoIpFallback.Providers
{
    public class GeoIPFallbackProvider : LocationFallbackProviderBase
    {
        private string _databasePath;
        public override void Initialize(string name, NameValueCollection config)
        {
            string databasePath = config["database"];
            if (!string.IsNullOrEmpty(databasePath))
            {
                this._databasePath = databasePath;
            }
            else
            {
                this._databasePath = "~/app_data/GeoLiteCity.dat";
            }
            base.Initialize(name, config);
        }

        public override WhoIsInformation Resolve(IPAddress ip)
        {
            var whoIsInformation = new WhoIsInformation();

            var lookUpService = new LookupService(HostingEnvironment.MapPath(_databasePath), LookupService.GEOIP_STANDARD);
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

            return whoIsInformation;
        }
    }
}
