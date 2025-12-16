namespace GestionCommercial.Models
{
    public class ProduitStock
    {
        public int ProduitId { get; set; }
        public string NomArticle { get; set; } = string.Empty;

        public int IdDepot { get; set; }
        public string NomDepot { get; set; } = string.Empty;

        public int QuantiteReelle { get; set; }
    }
}

