using Adm_Notificaciones;
using Adm_Notificaciones.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<BitacoraConsumer>();
builder.Services.AddHttpClient<AutenticacionService>();
builder.Services.AddScoped<NotificacionService>();

builder.Configuration["BitacoraService:BaseUrl"] = "http://localhost:5293";
builder.Configuration["AutenticacionApiUrl"] = "http://localhost:5233";

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapNotificacionEndpoints();
app.Run();
