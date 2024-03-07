using System;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    //from https://stackoverflow.com/questions/2010466/
    public class GeoLocationParsing
    {
        private const double EarthRadiusInMiles = 3956.0;
        private static double EarthRadiusInKilometers{ get { return EarthRadiusInMiles * 1.60934;}}

        /// <summary>
        /// Distance between two sets of latitudes and longitudes in miles
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <returns>distance</returns>
        public static double GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            return GetDistance(lat1, lng1, lat2, lng2, GeoCodeCalcMeasurement.Miles);
        }

        /// <summary>
        /// Distance between two sets of latitudes and longitudes 
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lng1"></param>
        /// <param name="lat2"></param>
        /// <param name="lng2"></param>
        /// <param name="m">miles or kilometers</param>
        /// <returns>distance</returns>
        public static double GetDistance(double lat1, double lng1, double lat2, double lng2, GeoCodeCalcMeasurement m)
        {
            double radius = EarthRadiusInMiles;

            if (m == GeoCodeCalcMeasurement.Kilometers) radius = EarthRadiusInKilometers;
            return radius * 2 * Math.Asin(Math.Min(1, Math.Sqrt((Math.Pow(Math.Sin((lat2.ToRadian() - lat1.ToRadian()) / 2.0), 2.0) + Math.Cos(lat1.ToRadian()) * Math.Cos(lat2.ToRadian()) * Math.Pow(Math.Sin((lng2.ToRadian() - lng1.ToRadian()) / 2.0), 2.0)))));
        }

        public enum GeoCodeCalcMeasurement
        {
            Miles = 0,
            Kilometers = 1
        }

    }
}
