using System.ComponentModel.DataAnnotations;

namespace GestionCommercial.Models
{
    public class Tiers
    {
        public int IdTiers { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(150)]
        public string NomTiers { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le type est obligatoire")]
        [StringLength(50)]
        public string TypeTiers { get; set; } = TiersTypes.Fournisseur;

        [StringLength(250)]
        public string? Adresse { get; set; }

        [StringLength(30)]
        public string? Telephone { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? Ville { get; set; }
        public int? IdModePaiementDefault { get; set; }
        //public string? ModePaiementDefautLibelle { get; set; }
    }

    public static class TiersTypes
    {
        public const string Fournisseur = "Fournisseur";
        public const string Client = "Client";
        public const string ClientFournisseur = "ClientFournisseur";
    }
}
