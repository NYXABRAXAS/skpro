using System.Text.Json;
using SuryodaySelfKiosk.Models;

namespace SuryodaySelfKiosk.Services;

/// <summary>
/// Wraps <see cref="ISession"/> as the prototype store for the in-progress application,
/// and mirrors every save into <see cref="IApplicationRepository"/> so a returning
/// customer can list / resume past applications.
/// Swap both for a real distributed cache / repository in production.
/// </summary>
public class ApplicationStateService(IHttpContextAccessor accessor, IApplicationRepository repository)
{
    private const string Key = "CarLoanApplication";

    private ISession Session =>
        accessor.HttpContext?.Session ?? throw new InvalidOperationException("Session not available.");

    public CarLoanApplication GetOrCreate()
    {
        var existing = Get();
        if (existing is not null) return existing;

        var created = NewApplication();
        Save(created);
        return created;
    }

    public CarLoanApplication? Get()
    {
        var json = Session.GetString(Key);
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<CarLoanApplication>(json);
    }

    public void Save(CarLoanApplication application)
    {
        application.LastUpdatedUtc = DateTimeOffset.UtcNow;
        Session.SetString(Key, JsonSerializer.Serialize(application));
        repository.Upsert(application);
    }

    /// <summary>Abandon the current in-session application (history in the repository is kept).</summary>
    public void Reset() => Session.Remove(Key);

    /// <summary>Start a brand-new application, keeping any earlier ones in the repository.</summary>
    public CarLoanApplication StartNew()
    {
        var created = NewApplication();
        Save(created);
        return created;
    }

    /// <summary>Load an existing application from the repository into the session to resume it.</summary>
    public CarLoanApplication? Resume(string applicationId)
    {
        var app = repository.GetById(applicationId);
        if (app is null) return null;

        // fresh copy in the session
        var copy = JsonSerializer.Deserialize<CarLoanApplication>(JsonSerializer.Serialize(app))!;
        Session.SetString(Key, JsonSerializer.Serialize(copy));
        return copy;
    }

    private static CarLoanApplication NewApplication() => new()
    {
        ApplicationId = $"APP-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}"
    };
}
