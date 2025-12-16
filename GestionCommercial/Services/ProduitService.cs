using GestionCommercial.Data;
using GestionCommercial.Models;

namespace GestionCommercial.Services
{
    public class ProduitService
    {
        private readonly ProduitRepository _repo;

        public ProduitService(ProduitRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Produit>> GetProduits() => _repo.GetProduitsAsync();
        public Task<Produit?> GetProduit(int id) => _repo.GetProduitByIdAsync(id);
        public Task<int> CreateProduit(Produit p) => _repo.CreateProduitAsync(p);
        public Task UpdateProduit(Produit p) => _repo.UpdateProduitAsync(p);
        public Task DeleteProduit(int id) => _repo.DeleteProduitAsync(id);

        // ⬇⬇ هادي المهمة باش الماركات يطلعوا فـ datalist
        public Task<List<string>> GetMarques() => _repo.GetMarquesAsync();
        public Task AddMarque(string marque) => _repo.InsertMarqueAsync(marque);


    }
}
