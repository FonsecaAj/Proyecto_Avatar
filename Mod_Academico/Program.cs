using Mod_Academico;
using Mod_Academico.Repository;
using Mod_Academico.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<HistorialAcademicoRepository>();

builder.Services.AddScoped<IHistorialAcademicoService, HistorialAcademicoService>();

builder.Services.AddHttpClient<BitacoraConsumer>();
builder.Configuration["BitacoraService:BaseUrl"] = "http://localhost:5293"; // URL del servicio GEN1

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapHistorialEndpoints();
app.Run();

