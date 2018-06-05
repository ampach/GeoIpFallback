using System;
using System.Collections.Specialized;
using System.Net;
using System.Web.Hosting;
using MaxMind.GeoIP2;
using Sitecore.Analytics.Model;
using Sitecore.Configuration;

namespace GeoIpFallback.Providers
{
    public class GeoIP2FallbackProvider : LocationFallbackProviderBase
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
                this._databasePath = "~/app_data/GeoLite2-City.mmdb";
            }
            base.Initialize(name, config);
        }

        public override WhoIsInformation Resolve(IPAddress ip)
        {
            var whoIsInformation = new WhoIsInformation();

            using (var reader = new DatabaseReader(HostingEnvironment.MapPath(_databasePath)))
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