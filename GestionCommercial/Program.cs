using GestionCommercial.Components;
using GestionCommercial.Data;
using GestionCommercial.Services;
using Microsoft.AspNetCore.Components.Authorization;
using GestionCommercial.Security;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// ⭐ MudBlazor Services
builder.Services.AddMudServices();

// Repositories
builder.Services.AddScoped<AchatRepository>();
builder.Services.AddScoped<VenteRepository>();
builder.Services.AddScoped<StockRepository>();
builder.Services.AddScoped<DepotRepository>();
builder.Services.AddScoped<UtilisateurRepository>();
builder.Services.AddScoped<TiersRepository>();
builder.Services.AddScoped<ProduitRepository>();
builder.Services.AddScoped<ModePaiementRepository>();

// Services
builder.Services.AddScoped<AchatService>();
builder.Services.AddScoped<VenteService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<DepotService>();
builder.Services.AddScoped<UtilisateurService>();
builder.Services.AddScoped<TiersService>();
builder.Services.AddScoped<ProduitService>();
builder.Services.AddScoped<MarqueService>();
builder.Services.AddScoped<ModePaiementService>();


// Authentication
builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();