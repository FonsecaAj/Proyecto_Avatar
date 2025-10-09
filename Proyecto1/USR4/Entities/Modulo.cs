namespace USR4.Entities
{
    public class Modulo
    {
        public int IdModulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}