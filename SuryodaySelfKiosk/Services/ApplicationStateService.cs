using System.Text.Json;
using SuryodaySelfKiosk.Models;

namespace SuryodaySelfKiosk.Services;

/// <summary>
/// Wraps <see cref="ISession"/> as the prototype store for the in-progress application.
/// Swap this for a real distributed cache / repository in production.
/// </summary>
public class ApplicationStateService(IHttpContextAccessor accessor)
{
    private const string Key = "CarLoanApplication";

    private ISession Session =>
        accessor.HttpContext?.Session ?? throw new InvalidOperationException("Session not available.");

    public CarLoanApplication GetOrCreate()
    {
        var json = Session.GetString(Key);
        if (!string.IsNullOrEmpty(json))
        {
            var existing = JsonSerializer.Deserialize<CarLoanApplication>(json);
            if (existing is not null) return existing;
        }

        var created = new CarLoanApplication
        {
            ApplicationId = $"APP-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}"
        };
        Save(created);
        return created;
    }

    public CarLoanApplication? Get()
    {
        var json = Session.GetString(Key);
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<CarLoanApplication>(json);
    }

    public void Save(CarLoanApplication application) =>
        Session.SetString(Key, JsonSerializer.Serialize(application));

    public void Reset() => Session.Remove(Key);
}
