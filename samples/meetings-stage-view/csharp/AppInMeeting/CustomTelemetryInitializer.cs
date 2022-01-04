using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Security.Cryptography;
using System.Text;

namespace AppInMeeting
{
    internal class CustomTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            byte[] tmpSource;
            byte[] tmpHash;
            ISupportProperties propTelemetry = telemetry as ISupportProperties;

            if (propTelemetry != null && !propTelemetry.Properties.ContainsKey("client-ip"))
            {
                tmpSource = ASCIIEncoding.ASCII.GetBytes(telemetry.Context.Location.Ip);
                tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
                propTelemetry.Properties.Add("client_id", ByteArrayToString(tmpHash));
            }
        }
        static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
    }
}