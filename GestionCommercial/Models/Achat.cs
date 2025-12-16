using System;
using System.Collections.Generic;

namespace GestionCommercial.Models
{
    public class Achat
    {
        public int AchatId { get; set; }
        public int IdTiers { get; set; }
        public string FournisseurNom { get; set; } = string.Empty;

        public int IdDepot { get; set; }
        public string DepotNom { get; set; } = string.Empty;

        public DateTime DateAchat { get; set; } = DateTime.Today;
        public string TypeDocument { get; set; } = "Facture";  // Commande / BL / Facture
        public decimal MontantTotal { get; set; }
        public string? Statut { get; set; } = "Brouillon";
        public string? Remarque { get; set; }

        public List<LigneAchat> Lignes { get; set; } = new();
    }
}
