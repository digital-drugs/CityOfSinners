using BEPUutilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elton_Comon_Files
{
    public static class Server_Helper
    {
        /// <summary>
        /// постоянное значение широты в метрах
        /// </summary>        
        const float Latitude_Value = 110574f;
        const float Longitude_Value = 111320f;

        public static float[] GPS_To_Unity(float GPS_Latitude, float GPS_Altitude, float GPS_Longtitude)
        {
            //Vector3 Result = Vector3.Zero;
            float[] Result = new float[3];

            Result[0] = Latitude_Value * GPS_Latitude;
            Result[1] = GPS_Altitude;
            Result[2] = Longitude_Value * (float)Math.Cos(GPS_Latitude);

            return Result;
        }
    }
}
