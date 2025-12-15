using System;

namespace FinTech.Core.Application.Interfaces.Services
{
    public interface IMfaProviderFactory
    {
        IMfaProvider GetMfaProvider(string method);
    }
}
