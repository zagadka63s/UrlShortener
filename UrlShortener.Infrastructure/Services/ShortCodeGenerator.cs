using System.Security.Cryptography;
using UrlShortener.Application.Abstractions;

namespace UrlShortener.Infrastructure.Services;

/// <summary>
/// URL-safe random short code generator (base62-like alphabet).
/// </summary>
public class ShortCodeGenerator : IShortCodeGenerator
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Next(int length = 7)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];

        return new string(chars);
    }
}
