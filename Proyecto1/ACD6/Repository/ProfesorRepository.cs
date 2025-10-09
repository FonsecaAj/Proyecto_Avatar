using Dapper;
using ACD6.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ACD6.Repository
{
    public class ProfesorRepository : IProfesorRepository
    {
        private readonly string _connectionString;

        public ProfesorRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<int> CrearAsync(Profesor profesor)
        {
            using var connection = CreateConnection();

            var sql = @"INSERT INTO PROFESOR (NUMERO_IDENTIFICACION, ID_TIPO_IDENTIFICACION, EMAIL, NOMBRE_COMPLETO, FECHA_NACIMIENTO, TELEFONOS) 
                       VALUES (@NumeroIdentificacion, @IdTipoIdentificacion, @Email, @NombreCompleto, @FechaNacimiento, @Telefonos);
                       SELECT CAST(SCOPE_IDENTITY() as int)";

            var id = await connection.ExecuteScalarAsync<int>(sql, profesor);
            return id;
        }

        public async Task ActualizarAsync(Profesor profesor)
        {
            using var connection = CreateConnection();

            var sql = @"UPDATE PROFESOR 
                       SET NUMERO_IDENTIFICACION = @NumeroIdentificacion,
                           ID_TIPO_IDENTIFICACION = @IdTipoIdentificacion,
                           EMAIL = @Email,
                           NOMBRE_COMPLETO = @NombreCompleto,
                           FECHA_NACIMIENTO = @FechaNacimiento,
                           TELEFONOS = @Telefonos,
                           FECHA_MODIFICACION = GETDATE()
                       WHERE ID_PROFESOR = @IdProfesor";

            await connection.ExecuteAsync(sql, profesor);
        }

        public async Task EliminarAsync(int idProfesor)
        {
            using var connection = CreateConnection();
            var sql = "DELETE FROM PROFESOR WHERE ID_PROFESOR = @IdProfesor";
            await connection.ExecuteAsync(sql, new { IdProfesor = idProfesor });
        }

        public async Task<IEnumerable<Profesor>> ObtenerTodosAsync()
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_PROFESOR as IdProfesor, 
                              NUMERO_IDENTIFICACION as NumeroIdentificacion,
                              ID_TIPO_IDENTIFICACION as IdTipoIdentificacion,
                              EMAIL as Email,
                              NOMBRE_COMPLETO as NombreCompleto,
                              FECHA_NACIMIENTO as FechaNacimiento,
                              TELEFONOS as Telefonos,
                              FECHA_CREACION as FechaCreacion,
                              FECHA_MODIFICACION as FechaModificacion,
                              ACTIVO as Activo
                       FROM PROFESOR";
            return await connection.QueryAsync<Profesor>(sql);
        }

        public async Task<Profesor?> ObtenerPorIdAsync(int idProfesor)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_PROFESOR as IdProfesor, 
                              NUMERO_IDENTIFICACION as NumeroIdentificacion,
                              ID_TIPO_IDENTIFICACION as IdTipoIdentificacion,
                              EMAIL as Email,
                              NOMBRE_COMPLETO as NombreCompleto,
                              FECHA_NACIMIENTO as FechaNacimiento,
                              TELEFONOS as Telefonos,
                              FECHA_CREACION as FechaCreacion,
                              FECHA_MODIFICACION as FechaModificacion,
                              ACTIVO as Activo
                       FROM PROFESOR 
                       WHERE ID_PROFESOR = @IdProfesor";
            return await connection.QueryFirstOrDefaultAsync<Profesor>(sql, new { IdProfesor = idProfesor });
        }

        public async Task<Profesor?> ObtenerPorEmailAsync(string email)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID_PROFESOR as IdProfesor, 
                              NUMERO_IDENTIFICACION as NumeroIdentificacion,
                              ID_TIPO_IDENTIFICACION as IdTipoIdentificacion,
                              EMAIL as Email,
                              NOMBRE_COMPLETO as NombreCompleto,
                              FECHA_NACIMIENTO as FechaNacimiento,
                              TELEFONOS as Telefonos,
                              FECHA_CREACION as FechaCreacion,
                              FECHA_MODIFICACION as FechaModificacion,
                              ACTIVO as Activo
                       FROM PROFESOR 
                       WHERE EMAIL = @Email";
            return await connection.QueryFirstOrDefaultAsync<Profesor>(sql, new { Email = email });
        }
    }
}