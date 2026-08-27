using SuryodaySelfKiosk.Configuration;
using SuryodaySelfKiosk.Services;
using SuryodaySelfKiosk.Services.Interfaces;
using SuryodaySelfKiosk.Services.Mock;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// --- Configuration -----------------------------------------------------------
builder.Services.Configure<SelfKioskOptions>(
    builder.Configuration.GetSection(SelfKioskOptions.SectionName));

// --- Session (prototype state store, no database) ---------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Suryoday.SelfKiosk.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// --- Application services ---------------------------------------------------
builder.Services.AddScoped<ApplicationStateService>();
builder.Services.AddSingleton<IQrCodeService, QrCodeService>();

// --- Integration seams: mock implementations (replace for production) ------
builder.Services.AddScoped<IOtpService, MockOtpService>();
builder.Services.AddScoped<IAadhaarService, MockAadhaarService>();
builder.Services.AddScoped<IPanService, MockPanService>();
builder.Services.AddScoped<IBureauService, MockBureauService>();
builder.Services.AddScoped<IBreService, MockBreService>();
builder.Services.AddScoped<IBankEmployeeService, MockBankEmployeeService>();
builder.Services.AddScoped<ILosService, MockLosService>();
builder.Services.AddSingleton<IAuditService, MockAuditService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    // HTTPS redirect only outside Development so plain http://localhost works for local testing.
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Kiosk}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
