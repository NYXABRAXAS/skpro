using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SuryodaySelfKiosk.Models;

namespace SuryodaySelfKiosk.Services;

/// <summary>Derives a stable, non-reversible key for a customer from their Aadhaar number.</summary>
public static class CustomerKey
{
    public static string FromAadhaar(string aadhaarNumber) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes((aadhaarNumber ?? string.Empty).Trim())));
}

/// <summary>
/// Prototype "customer applications" store. Keeps every application the customer
/// has touched, keyed by ApplicationId, so a returning customer (identified by a
/// mobile-OTP check) can see submitted applications and resume drafts.
///
/// In-memory only – data is lost when the app restarts. Swap for a real
/// repository / database in production.
/// </summary>
public interface IApplicationRepository
{
    void Upsert(CarLoanApplication application);
    CarLoanApplication? GetById(string applicationId);

    /// <summary>Applications belonging to a verified-Aadhaar customer, newest first.</summary>
    IReadOnlyList<CarLoanApplication> GetByAadhaarHash(string aadhaarHash);
}

public class InMemoryApplicationRepository : IApplicationRepository
{
    private readonly ConcurrentDictionary<string, CarLoanApplication> _store = new();

    public void Upsert(CarLoanApplication application)
    {
        if (string.IsNullOrEmpty(application.ApplicationId)) return;

        // Store an isolated copy so later session mutations don't alter history.
        var copy = JsonSerializer.Deserialize<CarLoanApplication>(JsonSerializer.Serialize(application))!;
        copy.LastUpdatedUtc = DateTimeOffset.UtcNow;
        _store[copy.ApplicationId] = copy;
    }

    public CarLoanApplication? GetById(string applicationId) =>
        _store.TryGetValue(applicationId, out var app) ? app : null;

    public IReadOnlyList<CarLoanApplication> GetByAadhaarHash(string aadhaarHash)
    {
        if (string.IsNullOrWhiteSpace(aadhaarHash)) return Array.Empty<CarLoanApplication>();

        return _store.Values
            .Where(a => a.AadhaarVerified && a.AadhaarHash == aadhaarHash)
            .OrderByDescending(a => a.LastUpdatedUtc)
            .ToList();
    }
}
