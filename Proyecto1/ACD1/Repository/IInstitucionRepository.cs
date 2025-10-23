﻿using ACD1.Entities;

namespace ACD1.Repository
{
    public interface IInstitucionRepository
    {
        Task<int> CrearAsync(Institucion institucion);
        Task ActualizarAsync(Institucion institucion);
        Task EliminarAsync(int idInstitucion);
        Task<IEnumerable<Institucion>> ObtenerTodosAsync();
        Task<Institucion?> ObtenerPorIdAsync(int idInstitucion);
    }
}