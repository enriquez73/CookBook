using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;

namespace Cookbook.Api.Endpoints;

public sealed class RemoteImageDownloader(MediaStorage storage)
{
    private const long MaxImageBytes = 15L * 1024 * 1024;
    private static readonly TimeSpan DownloadTimeout = TimeSpan.FromSeconds(20);

    public async Task<RemoteImageResult> DownloadAsync(string? value, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https")
            || !uri.IsDefaultPort
            || !string.IsNullOrEmpty(uri.UserInfo))
            return RemoteImageResult.Failure("Inserisci un URL HTTP o HTTPS valido.");

        using var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 3,
            AutomaticDecompression = DecompressionMethods.All,
            UseProxy = false,
            ConnectCallback = ConnectToPublicAddressAsync
        };
        using var client = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(DownloadTimeout);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.UserAgent.ParseAdd("Cookbook/1.0");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/jpeg"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/png"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/gif"));

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (!response.IsSuccessStatusCode)
                return RemoteImageResult.Failure("Il server dell'immagine non ha restituito un'immagine valida.");

            var contentType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();
            if (contentType is null || !MediaStorage.PhotoTypes.Contains(contentType))
                return RemoteImageResult.Failure("Il link deve puntare direttamente a un'immagine JPEG, PNG, WebP o GIF.");
            if (response.Content.Headers.ContentLength > MaxImageBytes)
                return RemoteImageResult.Failure("L'immagine supera il limite di 15 MB.");

            await using var source = await response.Content.ReadAsStreamAsync(timeout.Token);
            var fileName = await storage.SaveImageAsync(source, contentType, MaxImageBytes, timeout.Token);
            return fileName is null
                ? RemoteImageResult.Failure("Il file non è un'immagine valida, è vuoto o supera il limite di 15 MB.")
                : RemoteImageResult.Success(fileName, contentType);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return RemoteImageResult.Failure("Il download dell'immagine ha superato il tempo massimo.");
        }
        catch (HttpRequestException)
        {
            return RemoteImageResult.Failure("Non è stato possibile scaricare l'immagine da questo URL.");
        }
        catch (SocketException)
        {
            return RemoteImageResult.Failure("Non è stato possibile raggiungere il server dell'immagine.");
        }
    }

    private static async ValueTask<Stream> ConnectToPublicAddressAsync(
        SocketsHttpConnectionContext context,
        CancellationToken cancellationToken)
    {
        if (context.DnsEndPoint.Port is not (80 or 443))
            throw new HttpRequestException("Porta non consentita per il download dell'immagine.");
        var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, cancellationToken);
        if (addresses.Length == 0 || addresses.Any(address => !IsPublicAddress(address)))
            throw new HttpRequestException("Il server dell'immagine non è pubblico.");

        Exception? lastError = null;
        foreach (var address in addresses)
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            try
            {
                await socket.ConnectAsync(address, context.DnsEndPoint.Port, cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (Exception ex) when (ex is SocketException or OperationCanceledException)
            {
                socket.Dispose();
                lastError = ex;
                if (ex is OperationCanceledException) throw;
            }
        }

        throw new HttpRequestException("Connessione al server dell'immagine non riuscita.", lastError);
    }

    private static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) return IsPublicAddress(address.MapToIPv4());
        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if ((bytes[0] & 0xE0) != 0x20) return false; // Only global unicast 2000::/3.
            if (bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x0D && bytes[3] == 0xB8) return false; // Documentation.
            if (bytes[0] == 0x20 && bytes[1] == 0x01 && (bytes[2] & 0xFE) == 0x00) return false; // Special-purpose 2001::/23.
            if (bytes[0] == 0x20 && bytes[1] == 0x02) return false; // 6to4.
            if (bytes[0] == 0x3F && bytes[1] == 0xFF && (bytes[2] & 0xF0) == 0x00) return false; // Documentation 3fff::/20.
            return true;
        }

        if (address.AddressFamily != AddressFamily.InterNetwork) return false;
        var value = BinaryPrimitives.ReadUInt32BigEndian(bytes);
        return !InRange(value, 0x00000000, 0xFF000000) // 0.0.0.0/8
            && !InRange(value, 0x0A000000, 0xFF000000) // 10.0.0.0/8
            && !InRange(value, 0x64400000, 0xFFC00000) // 100.64.0.0/10
            && !InRange(value, 0x7F000000, 0xFF000000) // 127.0.0.0/8
            && !InRange(value, 0xA9FE0000, 0xFFFF0000) // 169.254.0.0/16
            && !InRange(value, 0xAC100000, 0xFFF00000) // 172.16.0.0/12
            && !InRange(value, 0xC0000000, 0xFFFFFF00) // 192.0.0.0/24
            && !InRange(value, 0xC0000200, 0xFFFFFF00) // 192.0.2.0/24
            && !InRange(value, 0xC0586300, 0xFFFFFF00) // 192.88.99.0/24
            && !InRange(value, 0xC0A80000, 0xFFFF0000) // 192.168.0.0/16
            && !InRange(value, 0xC6120000, 0xFFFE0000) // 198.18.0.0/15
            && !InRange(value, 0xC6336400, 0xFFFFFF00) // 198.51.100.0/24
            && !InRange(value, 0xCB007100, 0xFFFFFF00) // 203.0.113.0/24
            && (value & 0xF0000000) < 0xE0000000; // Multicast and reserved.
    }

    private static bool InRange(uint address, uint network, uint mask) => (address & mask) == network;
}

public sealed record RemoteImageResult(bool IsSuccess, string? FileName, string? ContentType, string? Error)
{
    public static RemoteImageResult Success(string fileName, string contentType) => new(true, fileName, contentType, null);
    public static RemoteImageResult Failure(string error) => new(false, null, null, error);
}
