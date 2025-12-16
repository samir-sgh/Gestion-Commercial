namespace GestionCommercial.Models
{
    public class LigneVente
    {
        public int LigneVenteId { get; set; }
        public int VenteId { get; set; }

        public int ProduitId { get; set; }
        public string? NomArticle { get; set; }

        public int QteVendue { get; set; }
        public decimal PrixVenteUnitaire { get; set; }
        public decimal? Remise { get; set; }    // %

        public decimal TotalLigne =>
            Math.Round(QteVendue * PrixVenteUnitaire * (1 - (Remise ?? 0) / 100m), 2);
    }
}
