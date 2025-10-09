namespace GEN1.Entities
{
    public class Bitacora
    {
        public int IdBitacora { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TipoAccion { get; set; } = string.Empty;
    }
}
