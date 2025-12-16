//using GestionCommercial.Data;
//using GestionCommercial.Models;

//namespace GestionCommercial.Services
//{
//    public class AchatService
//    {
//        private readonly AchatRepository _repo;

//        public AchatService(AchatRepository repo)
//        {
//            _repo = repo;
//        }

//        public Task<List<Tiers>> GetFournisseurs() => _repo.GetFournisseursAsync();
//        public Task<List<Depot>> GetDepots() => _repo.GetDepotsAsync();
//        public Task<List<Produit>> GetProduits() => _repo.GetProduitsAsync();
//        public Task<List<Achat>> GetListeAchats() => _repo.GetListeAchatsAsync();
//        public Task<Achat?> GetAchat(int id) => _repo.GetAchatAvecLignesAsync(id);
//        public Task<int> CreerAchat(Achat achat) => _repo.CreerAchatAsync(achat);
//    }
//}

using GestionCommercial.Data;
using GestionCommercial.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestionCommercial.Services
{
    public class AchatService
    {
        private readonly AchatRepository _repo;
        private readonly TiersService _tiersService; // ⭐ AJOUTER TiersService ici

        public AchatService(AchatRepository repo, TiersService tiersService) // ⭐ AJOUTER TiersService dans le constructeur
        {
            _repo = repo;
            _tiersService = tiersService;
        }

        // ⭐ CORRECTION: Utiliser TiersService pour obtenir les fournisseurs
        // Cela rend la méthode GetFournisseursAsync dans AchatRepository obsolète.
        public Task<List<Tiers>> GetFournisseurs()
            => _tiersService.GetTiersByType(TiersTypes.Fournisseur);

        // Le reste des méthodes ne change pas
        public Task<List<Depot>> GetDepots() => _repo.GetDepotsAsync();
        public Task<List<Produit>> GetProduits() => _repo.GetProduitsAsync();
        public Task<List<Achat>> GetListeAchats() => _repo.GetListeAchatsAsync();
        public Task<Achat?> GetAchat(int id) => _repo.GetAchatAvecLignesAsync(id);
        public Task<int> CreerAchat(Achat achat) => _repo.CreerAchatAsync(achat);
    }
}
