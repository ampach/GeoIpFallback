# GeoIp Fallback Module

# Overview

The **GeoIP Fallback** is a simple Sitcore module that improves a current location resolving process with additional ability to fake your current location with a specific one for testing purpose. Usually, Sitecore uses the GeoIP service which works fine. But technically the Geo Data for unknown (in case a contact visits the site the first time) IP addresses would be available only for the second request to server. Potentially, it, can cause issues for us. For example, if we have configured some personalization rules based on a visitor country, they would be applied only for the second and further requests.

## Main features

- Resolving current location for the first request based on the local MaxMind database
- Mock current location for development machine

Sitecore module can be downloaded by the following link: [GeoIP Fallback Module](https://github.com/ampach/GeoIpFallback/Module/GeoIP Fallback-1.0.zip) 

# Resolving current location for the first request

The module allows to resolve current location for the first request based on a local MaxMind database. This data would be updated with information coming from Sitecore GeoIP services as we consider that the data from the online service would be more accurate.

Local database based location resolving for the first request starts to work immediately after package installation.

# Set fake current location for development machine

If you are developer or QA engineer, you sometimes need to look at a site behavior if it was located in different location. For example in case when you want to check how a page will look with applied personalization based on the current country. The module allows to do that in a few steps:

1. Set `true` value for the `GeoIpFallback.Mocks.Enabled` setting in the `GeoIpFallback.Core.config`
2. Navigate to `/sitecore/system/Modules/GeoIP Fallback/New GepIP Manager` item in sitecore and choose current location:

![Set fake location](https://user-images.githubusercontent.com/1925984/40304688-5df75f90-5d00-11e8-91ba-1a3ded1414ae.png)

Extra location can be added under New GeoIP Manager with any information that you want to mock.

The current location will be updated immediately after GeoIP Manager is saved and page reloaded.

> IMPORTANT! When GeoIp Mocks is enabled, local MAxMind database and Sitecore’s GeoIP service are not utilized. You need to disable it if you want to come back to the default sitecore behavior.

# Settings

All module settings are currently included in only one configuration file which located by the following path: `/App_Config/Include/GeoIpFallback/GeoIpFallback.Core.config`

## Common settings:

| Setting name  | Description | Default value |
| ------------- | ------------- | ------------- |
| `GeoIpFallback.Mocks.Enabled`  | Enables or disables mocking of the current location process  | `false`  |
| `GeoIpFallback.Mocks.ManagerPath`  | Defines the Sitecore path where Mock Manager is located  | `/sitecore/system/Modules/GeoIP Fallback/New GepIP Manager`  |
| `mockLocationFallbackManager`  | Defines a list of Mock Location providers. Allows us to choose which one are currently using  | `default`  |
| `locationFallbackManager`  | Defines a list of Location Fallback Managers. Allows us to choose which would be currently used  | `geoIp2`  |

## Location Fallback Manager Settings

Location fallback manager represents a list of fallback providers. 

Each of them has the following settings:

| Setting name  | Description | Default value |
| ------------- | ------------- | ------------- |
| `name`  | Name of provider  |   |
| `database`  | Defines a path where the local MaxMind database is located.  | for geoIp: `~/app_data/GeoLiteCity.dat`; for geoIp2: `~/app_data/GeoLite2-City.mmdb`  |

There are two predefined providers are available to switch between each other:
- geoIp - is obsolete; MaxMind doesn’t support it anymore ([https://dev.maxmind.com/geoip/legacy/geolite/](https://dev.maxmind.com/geoip/legacy/geolite/)). 
- geoIp2 - is actual MaxMind database

Additional fallback providers can be added there. It should be based on `GeoIpFallback.Providers.LocationFallbackProviderBase` class and implement Resolve method which returns `Sitecore.Analytics.Model.WhoIsInformation` object.

## Mock Location Fallback Manager Settings

Mock location fallback manager represents a list of mock location providers.
 
Predefined provider has the following settings:	

| Setting name  | Description | Default value |
| ------------- | ------------- | ------------- |
| `name`  | Name of provider  |   |
| `database`  | Defines a Sitecore database where a mock items are stored.  | `master`  |

Additional mock location providers can be added there. It should be based on `GeoIpFallback.Mock.MockLocationFallbackProviderBase` class and implement `GetMockCurrentLocation` method which returns `Sitecore.Analytics.Model.WhoIsInformation` object.
