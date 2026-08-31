using System.Collections.Concurrent;
using System.Text.Json;
using SuryodaySelfKiosk.Models;

namespace SuryodaySelfKiosk.Services;

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

    /// <summary>Applications whose (verified) mobile number matches, newest first.</summary>
    IReadOnlyList<CarLoanApplication> GetByMobile(string mobileNumber);
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

    public IReadOnlyList<CarLoanApplication> GetByMobile(string mobileNumber)
    {
        if (string.IsNullOrWhiteSpace(mobileNumber)) return Array.Empty<CarLoanApplication>();

        return _store.Values
            .Where(a => a.MobileVerified && a.MobileNumber == mobileNumber)
            .OrderByDescending(a => a.LastUpdatedUtc)
            .ToList();
    }
}
