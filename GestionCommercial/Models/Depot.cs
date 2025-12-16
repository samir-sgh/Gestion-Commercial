namespace GestionCommercial.Models
{
    public class Depot
    {
        public int IdDepot { get; set; }
        public string NomDepot { get; set; } = string.Empty;
        public string? CodeDepot { get; set; }
        public string? Adresse { get; set; }
        public bool Actif { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
