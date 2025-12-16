using System;

namespace GestionCommercial.Models
{
    public class LigneAchat
    {
        public int LigneAchatId { get; set; }
        public int AchatId { get; set; }

        public int ProduitId { get; set; }
        public string? NomArticle { get; set; }

        public int QteAchetee { get; set; }
        public decimal PrixAchat { get; set; }
        public decimal? Remise { get; set; }   // %

        public decimal TotalLigne =>
            Math.Round(QteAchetee * PrixAchat * (1 - (Remise ?? 0) / 100m), 2);
    }
}
