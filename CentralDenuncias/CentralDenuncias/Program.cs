using CentralDenuncias.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<Database>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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


app.Run();
