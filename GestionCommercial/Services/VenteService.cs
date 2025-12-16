using GestionCommercial.Data;
using GestionCommercial.Models;

namespace GestionCommercial.Services
{
    public class VenteService
    {
        private readonly VenteRepository _repo;

        public VenteService(VenteRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Tiers>> GetClients() => _repo.GetClientsAsync();
        public Task<List<Depot>> GetDepots() => _repo.GetDepotsAsync();
        public Task<List<Produit>> GetProduits() => _repo.GetProduitsAsync();
        public Task<List<Vente>> GetListeVentes() => _repo.GetListeVentesAsync();
        public Task<Vente?> GetVente(int id) => _repo.GetVenteAvecLignesAsync(id);
        public Task<int> CreerVente(Vente v) => _repo.CreerVenteAsync(v);
    }
}
