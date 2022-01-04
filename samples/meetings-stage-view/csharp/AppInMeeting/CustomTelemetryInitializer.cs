using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Net;
using System.Net.Sockets;
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

            if (propTelemetry != null && !propTelemetry.Properties.ContainsKey("client_id"))
            {
                tmpSource = ASCIIEncoding.ASCII.GetBytes(GetLocalIPAddress());
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

        static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}