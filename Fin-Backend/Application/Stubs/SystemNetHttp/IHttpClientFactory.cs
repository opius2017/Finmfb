using System.Net.Http;

namespace System.Net.Http
{
    // Minimal local stub to satisfy project references during compile-fix
    public interface IHttpClientFactory
    {
        HttpClient CreateClient(string name);
    }
}
