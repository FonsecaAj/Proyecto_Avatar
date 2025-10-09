using Dapper;
using USR4.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace USR4.Repository
{
    public class ModuloRepository : IModuloRepository
    {
        private readonly string _connectionString;

        public ModuloRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<int> CrearAsync(Modulo modulo)
        {
            using var connection = CreateConnection();
            var sql = @"INSERT INTO MODULO (NOMBRE) 
                       OUTPUT INSERTED.ID_MODULO
                       VALUES (@Nombre)";

            return await connection.ExecuteScalarAsync<int>(sql, new { modulo.Nombre });
        }

        public async Task ActualizarAsync(Modulo modulo)
        {
            using var connection = CreateConnection();
            var sql = @"UPDATE MODULO 
                       SET NOMBRE = @Nombre,
                           FECHA_MODIFICACION = GETDATE()
                       WHERE ID_MODULO = @IdModulo";

            await connection.ExecuteAsync(sql, new { modulo.Nombre, modulo.IdModulo });
        }

        public async Task EliminarAsync(int id)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM MODULO WHERE ID_MODULO = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<Modulo>> ObtenerTodosAsync()
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_MODULO as IdModulo, 
                              NOMBRE as Nombre, 
                              FECHA_CREACION as FechaCreacion, 
                              FECHA_MODIFICACION as FechaModificacion 
                       FROM MODULO";
            return await connection.QueryAsync<Modulo>(sql);
        }

        public async Task<Modulo?> ObtenerPorIdAsync(int id)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_MODULO as IdModulo, 
                              NOMBRE as Nombre, 
                              FECHA_CREACION as FechaCreacion, 
                              FECHA_MODIFICACION as FechaModificacion 
                       FROM MODULO 
                       WHERE ID_MODULO = @Id";
            return await connection.QueryFirstOrDefaultAsync<Modulo>(sql, new { Id = id });
        }

        public async Task<Modulo?> ObtenerPorNombreAsync(string nombre)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_MODULO as IdModulo, 
                              NOMBRE as Nombre, 
                              FECHA_CREACION as FechaCreacion, 
                              FECHA_MODIFICACION as FechaModificacion 
                       FROM MODULO 
                       WHERE LOWER(LTRIM(RTRIM(NOMBRE))) = LOWER(@Nombre)";
            return await connection.QueryFirstOrDefaultAsync<Modulo>(sql, new { Nombre = nombre.Trim() });
        }
    }
}