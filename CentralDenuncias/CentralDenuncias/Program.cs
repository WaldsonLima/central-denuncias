using CentralDenuncias.Context;
using CentralDenuncias.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<Database>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services
    .AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<Database>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);  // Tempo m�ximo de inatividade antes de expirar
    options.Cookie.HttpOnly = true;  // Protege o cookie da sess�o
    options.Cookie.IsEssential = true;  // Necess�rio para conformidade com o GDPR
});
builder.Services.AddControllersWithViews();

// Configura o servi�o para aceitar cabe�alhos de proxy como X-Forwarded-For
builder.Services.Configure<IISOptions>(options =>
{
    options.ForwardClientCertificate = false; // Se voc� n�o usar SSL Client Certificates
});

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true; // Opcional, mas pode ser �til
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Habilita o middleware para usar os cabe�alhos de proxy (X-Forwarded-For)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseSession();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.Use(async (context, next) =>
{
    // Verifica se a URL solicitada n�o � a p�gina de login e o cookie de senha n�o existe
    if (!context.Request.Cookies.ContainsKey("SenhaAutenticada") && context.Request.Path != "/Home/Login")
    {
        context.Response.Redirect("/Home/Login");
    }
    else
    {
        // Caso contr�rio, segue para o pr�ximo middleware ou action
        await next.Invoke();
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapIdentityApi<User>();

app.Run();
