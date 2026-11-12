using ArvoreAVL.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Registra o serviço da árvore AVL
builder.Services.AddSingleton<ArvoreAVLService>();

// Registra o histórico de rotações (Singleton para manter os dados entre as requisições)
builder.Services.AddSingleton<HistoricoRotacoesService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();