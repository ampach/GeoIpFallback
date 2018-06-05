using System;
using System.Collections.Specialized;
using Sitecore.Analytics;
using Sitecore.Analytics.Model;
using Sitecore.Configuration;

namespace GeoIpFallback.Mock
{
    public class SitecoreMockLocationProvider : MockLocationFallbackProviderBase
    {
        private const string LastModifiedCustomValuesKey = "GeoIpFallback.Mocks.LastModified";
        private const string LocationCustomValuesKey = "GeoIpFallback.Mocks.CurrentLocation";

        private string _database;
        public override void Initialize(string name, NameValueCollection config)
        {
            string database = config["database"];
            if (!string.IsNullOrEmpty(database))
            {
                this._database = database;
            }
            else
            {
                this._database = "master";
            }
            base.Initialize(name, config);
        }

        private DateTime LastModified
        {
            get
            {
                if (Tracker.Current.Session.Interaction.CustomValues.ContainsKey(LastModifiedCustomValuesKey))
                {
                    return (DateTime)Tracker.Current.Session.Interaction.CustomValues[LastModifiedCustomValuesKey];
                }

                return DateTime.MinValue;
            }
            set
            {
                Tracker.Current.Session.Interaction.CustomValues.Add(LastModifiedCustomValuesKey, value);
            }
        }

        private WhoIsInformation CurrentLocation
        {
            get
            {
                if (Tracker.Current.Session.Interaction.CustomValues.ContainsKey(LocationCustomValuesKey))
                {
                    return (WhoIsInformation)Tracker.Current.Session.Interaction.CustomValues[LocationCustomValuesKey];
                }

                return null;
            }
            set
            {
                Tracker.Current.Session.Interaction.CustomValues.Add(LocationCustomValuesKey, value);
            }
        }

        public override WhoIsInformation GetMockCurrentLocation()
        {
            return GetMockData();
        }

        private WhoIsInformation GetMockData()
        {
            var result = new WhoIsInformation();
            var managerPath = Settings.GetSetting("GeoIpFallback.Mocks.ManagerPath", "/sitecore/system/Modules/GeoIP Fallback/New GepIP Manager");

            if (!string.IsNullOrWhiteSpace(managerPath))
            {
                var manager = Sitecore.Configuration.Factory.GetDatabase(_database).GetItem(managerPath);

                if (manager != null)
                {
                    var cvCurrentLocation = CurrentLocation;
                    if (manager.Statistics.Updated <= LastModified && cvCurrentLocation != null)
                    {
                        return cvCurrentLocation;
                    }

                    if (cvCurrentLocation != null)
                    {
                        Tracker.Current.Session.Interaction.CustomValues.Remove(LocationCustomValuesKey);
                        Tracker.Current.Session.Interaction.CustomValues.Remove(LastModifiedCustomValuesKey);
                    }

                    Sitecore.Data.Fields.ReferenceField currentLocation = manager.Fields[Constants.CurrentLocationFieldName];
                    if (currentLocation != null && currentLocation.TargetItem != null)
                    {
                        result.Country = currentLocation.TargetItem[Constants.CountryFieldName];
                        result.City = currentLocation.TargetItem[Constants.CityFieldName];
                        result.PostalCode = currentLocation.TargetItem[Constants.PostalCodeFieldName];
                        result.Longitude = currentLocation.TargetItem.Fields[Constants.LongitudeFieldName].HasValue ? double.Parse(currentLocation.TargetItem.Fields[Constants.LongitudeFieldName].Value) : (double?)null;
                        result.Latitude = currentLocation.TargetItem.Fields[Constants.LatitudeFieldName].HasValue ? double.Parse(currentLocation.TargetItem.Fields[Constants.LongitudeFieldName].Value) : (double?)null;
                        result.AreaCode = currentLocation.TargetItem[Constants.AreaCodeFieldName];
                        result.BusinessName = currentLocation.TargetItem[Constants.BusinessNameFieldName];
                        result.Dns = currentLocation.TargetItem[Constants.DnsFieldName];
                        result.Isp = currentLocation.TargetItem[Constants.IspFieldName];
                        result.MetroCode = currentLocation.TargetItem[Constants.MetroCodeFieldName];
                        result.Region = currentLocation.TargetItem[Constants.RegionFieldName];
                        result.Url = currentLocation.TargetItem[Constants.UrlFieldName];

                        CurrentLocation = result;
                        LastModified = manager.Statistics.Updated;

                        return result;
                    }
                }
            }

            result.BusinessName = "Not Available";
            return result;
        }
    }
}
