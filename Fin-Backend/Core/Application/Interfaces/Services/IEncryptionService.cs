using System;

namespace FinTech.Core.Application.Interfaces.Services
{
    public interface IEncryptionService
    {
        string GenerateRandomToken();
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        string Hash(string input);
        bool VerifyHash(string input, string hash);
    }
}
