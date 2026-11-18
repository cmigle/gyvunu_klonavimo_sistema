using System.Text.Encodings.Web;
using System.Text.Unicode;
using GyvunuKlonavimoSistema.Data;
using GyvunuKlonavimoSistema.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRouting(o => o.LowercaseUrls = true);
// Lietuviškos raidės HTML išvedamos tiesiogiai, ne kaip &#x17E;
builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

builder.Services.AddDbContext<KlonavimoDbContext>(o =>
    o.UseMySQL(builder.Configuration.GetConnectionString("KlonavimoDb")!));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/prisijungimas";
        o.AccessDeniedPath = "/prisijungimas/draudziama";
        o.Cookie.Name = "klonavimas";
    });

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();
builder.Services.AddSingleton(builder.Configuration.GetSection("ElPastas").Get<ElPastoNustatymai>() ?? new ElPastoNustatymai());
builder.Services.AddSingleton<IElPastoServeris, SmtpElPastoServeris>();
builder.Services.AddScoped<UzsakymoPatvirtinimas>();
builder.Services.AddSingleton<DokumentuSaugykla>();
var courtListenerToken = builder.Configuration["CourtListener:Token"];
if (string.IsNullOrWhiteSpace(courtListenerToken))
    builder.Services.AddSingleton<ICourtListenerSasaja, DemoCourtListenerSasaja>();
else
    builder.Services.AddHttpClient<ICourtListenerSasaja, CourtListenerApiSasaja>(c =>
    {
        c.BaseAddress = new Uri(CourtListener.Adresas);
        c.DefaultRequestHeaders.Authorization = new("Token", courtListenerToken);
        c.DefaultRequestHeaders.UserAgent.ParseAdd("GyvunuKlonavimoSistema/1.0");
    });

var app = builder.Build();

await DuomenuBaze.ParuostiAsync(app.Services, atkurti: args.Contains("--atkurti-duomenis"));

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/pagrindinis/klaida");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Pagrindinis}/{action=Index}/{id?}");

app.Run();
