using Adm_Notas;
using Adm_Notas.Entities;
using Adm_Notas.Repository;
using Adm_Notas.Services;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==== Conexión BD y Repositorios ====
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<RubroRepository>();
builder.Services.AddScoped<NotaRepository>();

// ==== Servicios ====
builder.Services.AddScoped<IRubroServices, RubroServices>();
builder.Services.AddScoped<INotasServices, NotaService>();

// ==== Bitácora (para microservicio GEN1) ====
builder.Services.AddHttpClient<BitacoraConsumer>();
builder.Configuration["BitacoraService:BaseUrl"] = "http://localhost:5293"; // cambia el puerto al de tu servicio GEN1

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapear endpoints
app.MapRubrosEndpoints();

app.Run();
