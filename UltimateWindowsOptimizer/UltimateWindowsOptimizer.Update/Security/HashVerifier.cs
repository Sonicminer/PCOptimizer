using System.Security.Cryptography;
using System.Text;

namespace UltimateWindowsOptimizer.Update.Security;

public static class HashVerifier
{
    /// <summary>
    /// Computes SHA-256 of a file and compares to the expected hex string (case-insensitive).
    /// </summary>
    public static async Task<bool> VerifyFileSha256Async(string filePath, string expectedSha256, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(expectedSha256))
            return false;

        var expected = expectedSha256.Trim().ToLowerInvariant().Replace("-", "");
        if (expected.Length != 64)
            return false;

        await using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream, ct).ConfigureAwait(false);
        var actual = Convert.ToHexString(hash).ToLowerInvariant();
        return string.Equals(actual, expected, StringComparison.Ordinal);
    }

    public static async Task<string> ComputeFileSha256Async(string filePath, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream, ct).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string ComputeStringSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
