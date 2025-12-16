namespace GestionCommercial.Models
{
    public class Vente
    {
        public int VenteId { get; set; }
        public int IdTiers { get; set; }          // Client
        public string ClientNom { get; set; } = string.Empty;

        public int IdDepot { get; set; }          // Dépôt d'où sort la marchandise
        public string DepotNom { get; set; } = string.Empty;

        public DateTime DateVente { get; set; } = DateTime.Today;
        public string TypeDocument { get; set; } = "Facture"; // "Ticket", "Facture", "BL"...
        public decimal MontantTotal { get; set; }
        public string? Statut { get; set; } = "Valide";       // plus tard: "Brouillon", "Valide", ...
        public string? Remarque { get; set; }

        public List<LigneVente> Lignes { get; set; } = new();
    }
}
