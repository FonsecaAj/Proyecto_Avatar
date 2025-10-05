namespace EjemploMicroServicio.Entities
{
    public class Persona
    {
        public string PersonaId { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        public byte Tipo { get; set; }

        public string? Telefono { get; set; } = null!;

        public string Rol { get; set; } = null!;

        public string? Gender { get; set; } = null!;

        public string Password { get; set; } = string.Empty;
    }
}
