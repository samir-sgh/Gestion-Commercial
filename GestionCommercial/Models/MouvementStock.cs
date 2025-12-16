namespace GestionCommercial.Models
{
    public class MouvementStock
    {
        public int MouvementId { get; set; }
        public int ProduitId { get; set; }
        public string NomArticle { get; set; } = string.Empty;

        public int IdDepot { get; set; }
        public string NomDepot { get; set; } = string.Empty;

        public DateTime DateMouvement { get; set; }
        public string TypeMouvement { get; set; } = string.Empty; // Achat, Vente, Transfert+, Transfert-, Ajustement

        public int Qte { get; set; }
        public int? QteAvant { get; set; }
        public int? QteApres { get; set; }

        public string? SourceDoc { get; set; }   // "Achat", "Vente", ...
        public int? IdDoc { get; set; }          // رقم facture / achat...
    }
}
