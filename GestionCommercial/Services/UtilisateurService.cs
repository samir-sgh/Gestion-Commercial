using GestionCommercial.Data;
using GestionCommercial.Models;

namespace GestionCommercial.Services
{
    public class UtilisateurService
    {
        private readonly UtilisateurRepository _repo;

        public UtilisateurService(UtilisateurRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Utilisateur>> GetUtilisateursAsync(bool includeInactive = true)
            => _repo.GetUtilisateursAsync(includeInactive);

        public Task<Utilisateur?> GetByIdAsync(int id)
            => _repo.GetByIdAsync(id);

        public Task<int> CreerUtilisateurAsync(Utilisateur u)
            => _repo.InsertAsync(u);

        public Task MettreAJourUtilisateurAsync(Utilisateur u)
            => _repo.MettreAJourAsync(u);

        public Task ChangerMotDePasseAsync(int id, string nouveauMdp)
            => _repo.ChangerMotDePasseAsync(id, nouveauMdp);

        public Task SupprimerAsync(int id)
            => _repo.SupprimerAsync(id);

        public Task<Utilisateur?> AuthenticateAsync(string username, string password)
            => _repo.AuthenticateAsync(username, password);

        public Task CreerAsync(Utilisateur u, string? plainPassword)
        {
            // كنشفرو كلمة السر فقط إذا كانت معمرة
            if (!string.IsNullOrWhiteSpace(plainPassword))
                u.MotDePasse = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            return _repo.CreerAsync(u);
        }

        public Task MettreAJourAsync(Utilisateur u, string? plainPassword)
        {
            // لا نبدل الباس إلا إذا وضعه المستخدم
            if (!string.IsNullOrWhiteSpace(plainPassword))
                u.MotDePasse = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            return _repo.MettreAJourAsync(u);
        }

    }
}
