namespace ACD6.Entities
{
    public class Profesor
    {
        public int IdProfesor { get; set; }
        public string NumeroIdentificacion { get; set; } = string.Empty;
        public int IdTipoIdentificacion { get; set; }
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Telefonos { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool Activo { get; set; }
    }
}
