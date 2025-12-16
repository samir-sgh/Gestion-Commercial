using System;
using System.ComponentModel.DataAnnotations;

namespace GestionCommercial.Models
{
    public class Produit
    {
        public int ProduitId { get; set; }

        [Required]
        [StringLength(150)]
        public string NomArticle { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Reference { get; set; }

        [StringLength(100)]
        public string? Marque { get; set; }

        [Range(0, 999999999)]
        public decimal Quantite { get; set; }

        [Range(0, 999999999)]
        public decimal PrixAchat { get; set; }

        [Range(0, 999999999)]
        public decimal PrixVente { get; set; }

        [Range(0, 999999999)]
        public decimal PrixGros { get; set; }

        [StringLength(50)]
        public string? CodeBarre { get; set; }

        [Range(0, int.MaxValue)]
        public int StockMin { get; set; }

        public bool Favoris { get; set; }

        public int? IdDepot { get; set; }

        public DateTime? DateCreation { get; set; }

        // الصورة مخزّنة فـ varbinary(max)
        public byte[]? ImageData { get; set; }

        public int? IdMarque { get; set; }

        // Helper للعرض فالـ <img>
        public string? ImageBase64 =>
            ImageData == null ? null : $"data:image/png;base64,{Convert.ToBase64String(ImageData)}";
    }
}
