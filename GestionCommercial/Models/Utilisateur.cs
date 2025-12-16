namespace GestionCommercial.Models
{
    public class Utilisateur
    {
        public int IdUtilisateur { get; set; }
        public string NomUtilisateur { get; set; } = string.Empty;
        public string MotDePasse { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool Actif { get; set; } = true;
        public DateTime DateCreation { get; set; }
    }
}
