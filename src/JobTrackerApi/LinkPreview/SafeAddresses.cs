using System.Net;
using System.Net.Sockets;

// Keeps link previews away from private networks (SSRF protection)
public static class SafeAddresses
{
    // True for loopback, private, link-local (cloud metadata), and other internal ranges
    public static bool IsBlocked(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();

        if (IPAddress.IsLoopback(address)) return true;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var b = address.GetAddressBytes();
            return b[0] == 0                                  // 0.0.0.0/8
                || b[0] == 10                                 // 10.0.0.0/8
                || (b[0] == 100 && b[1] >= 64 && b[1] <= 127) // 100.64.0.0/10
                || (b[0] == 169 && b[1] == 254)               // 169.254.0.0/16 (cloud metadata)
                || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)  // 172.16.0.0/12
                || (b[0] == 192 && b[1] == 168)               // 192.168.0.0/16
                || b[0] >= 224;                               // multicast and reserved
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            var b = address.GetAddressBytes();
            return address.IsIPv6LinkLocal
                || address.IsIPv6SiteLocal
                || address.IsIPv6Multicast
                || (b[0] & 0xFE) == 0xFC                      // fc00::/7 (unique local)
                || address.Equals(IPAddress.IPv6None);
        }

        return true;
    }

    // An HTTP handler that only ever connects to public addresses, even after redirects
    public static SocketsHttpHandler CreateHandler()
    {
        return new SocketsHttpHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
            ConnectTimeout = TimeSpan.FromSeconds(5),
            ConnectCallback = async (context, cancellationToken) =>
            {
                var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, cancellationToken);
                var address = addresses.FirstOrDefault(a => !IsBlocked(a))
                    ?? throw new BlockedAddressException(context.DnsEndPoint.Host);

                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
                try
                {
                    await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port), cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            }
        };
    }
}

// Thrown when a link points somewhere private
public class BlockedAddressException(string host)
    : Exception($"{host} points to a private address.");
