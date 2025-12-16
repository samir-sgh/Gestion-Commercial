namespace GestionCommercial.Models
{
    public class ModePaiement
    {
        public int IdModePaiement { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Libelle { get; set; } = string.Empty;
        public bool Actif { get; set; } = true;
    }
}
