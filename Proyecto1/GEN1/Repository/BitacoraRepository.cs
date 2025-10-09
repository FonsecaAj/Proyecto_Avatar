using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using GEN1.Entities;

namespace GEN1.Repository
{
    public class BitacoraRepository : IBitacoraRepository
    {
        private readonly string _connectionString;

        public BitacoraRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<int> CrearBitacoraAsync(Bitacora bitacora)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO Bitacora (Fecha_Registro, Usuario, Descripcion, Tipo_Accion) 
                       VALUES (@FechaRegistro, @Usuario, @Descripcion, @TipoAccion);
                       SELECT CAST(SCOPE_IDENTITY() as int)";

            return await connection.ExecuteScalarAsync<int>(sql, bitacora);
        }
    }
}