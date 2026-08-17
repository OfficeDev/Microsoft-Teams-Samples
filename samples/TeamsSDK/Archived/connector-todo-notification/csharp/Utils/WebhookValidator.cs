using System.Net;
using System.Net.Sockets;

namespace TeamsToDoAppConnector.Utils
{
    /// <summary>
    /// Validates webhook destinations before they are stored or called, to prevent
    /// Server-Side Request Forgery (SSRF, CWE-918). A destination is accepted only when
    /// it is an absolute HTTPS URL, its host matches a configured allow-list of trusted
    /// Microsoft connector endpoints, and none of the host's resolved IP addresses fall
    /// into loopback, private, link-local, or other special-use ranges.
    /// </summary>
    public static class WebhookValidator
    {
        /// <summary>
        /// Returns true only when the supplied webhook URL is safe to call.
        /// </summary>
        /// <param name="webhookUrl">The caller-supplied webhook destination.</param>
        /// <param name="allowedHostSuffixes">Configured trusted host suffixes (e.g. ".webhook.office.com").</param>
        /// <param name="failureReason">Human readable reason when validation fails.</param>
        public static bool IsValid(string? webhookUrl, string[]? allowedHostSuffixes, out string failureReason)
        {
            failureReason = string.Empty;

            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                failureReason = "Webhook URL is empty.";
                return false;
            }

            if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out Uri? uri))
            {
                failureReason = "Webhook URL is not a valid absolute URL.";
                return false;
            }

            // Only HTTPS is permitted; this blocks http, file, gopher, etc.
            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                failureReason = "Webhook URL must use the https scheme.";
                return false;
            }

            // Reject embedded credentials (user:pass@host).
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                failureReason = "Webhook URL must not contain user information.";
                return false;
            }

            if (!IsHostAllowed(uri.Host, allowedHostSuffixes))
            {
                failureReason = "Webhook host is not in the allow-list of trusted connector endpoints.";
                return false;
            }

            // Resolve the host and ensure no address points at an internal/special-use range.
            IPAddress[] addresses;
            try
            {
                if (IPAddress.TryParse(uri.Host, out IPAddress? literal))
                {
                    addresses = new[] { literal };
                }
                else
                {
                    addresses = Dns.GetHostAddresses(uri.Host);
                }
            }
            catch (SocketException)
            {
                failureReason = "Webhook host could not be resolved.";
                return false;
            }

            if (addresses == null || addresses.Length == 0)
            {
                failureReason = "Webhook host did not resolve to any address.";
                return false;
            }

            foreach (var address in addresses)
            {
                if (IsPrivateOrReserved(address))
                {
                    failureReason = "Webhook host resolves to a private, loopback, or reserved address.";
                    return false;
                }
            }

            return true;
        }

        private static bool IsHostAllowed(string host, string[]? allowedHostSuffixes)
        {
            if (allowedHostSuffixes == null || allowedHostSuffixes.Length == 0)
            {
                // Fail closed: without a configured allow-list nothing is trusted.
                return false;
            }

            foreach (var suffix in allowedHostSuffixes)
            {
                if (string.IsNullOrWhiteSpace(suffix))
                {
                    continue;
                }

                var normalized = suffix.Trim();
                if (host.Equals(normalized.TrimStart('.'), StringComparison.OrdinalIgnoreCase) ||
                    host.EndsWith(normalized.StartsWith('.') ? normalized : "." + normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPrivateOrReserved(IPAddress address)
        {
            if (address.IsIPv4MappedToIPv6)
            {
                address = address.MapToIPv4();
            }

            if (IPAddress.IsLoopback(address))
            {
                return true;
            }

            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                byte[] bytes = address.GetAddressBytes();

                // 0.0.0.0/8 (unspecified/current network)
                if (bytes[0] == 0) return true;

                // 10.0.0.0/8
                if (bytes[0] == 10) return true;

                // 100.64.0.0/10 (CGNAT)
                if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127) return true;

                // 127.0.0.0/8 (loopback)
                if (bytes[0] == 127) return true;

                // 169.254.0.0/16 (link-local)
                if (bytes[0] == 169 && bytes[1] == 254) return true;

                // 172.16.0.0/12
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;

                // 192.168.0.0/16
                if (bytes[0] == 192 && bytes[1] == 168) return true;

                // 224.0.0.0/4 (multicast) and 240.0.0.0/4 (reserved)
                if (bytes[0] >= 224) return true;

                return false;
            }

            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast)
                {
                    return true;
                }

                // ::/128 unspecified and ::1 loopback are already covered by IsLoopback / below.
                if (address.Equals(IPAddress.IPv6Any) || address.Equals(IPAddress.IPv6Loopback))
                {
                    return true;
                }

                byte[] bytes = address.GetAddressBytes();

                // fc00::/7 unique local addresses
                if ((bytes[0] & 0xFE) == 0xFC)
                {
                    return true;
                }

                return false;
            }

            // Unknown address family: treat as unsafe.
            return true;
        }
    }
}
