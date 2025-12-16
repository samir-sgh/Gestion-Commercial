using GestionCommercial.Data;
using GestionCommercial.Models;

namespace GestionCommercial.Services
{
    public class StockService
    {
        private readonly StockRepository _repo;

        public StockService(StockRepository repo)
        {
            _repo = repo;
        }

        public Task<List<ProduitStock>> GetStockActuel()
            => _repo.GetStockActuelAsync();

        public Task<List<ProduitStock>> GetStockProduit(int produitId)
            => _repo.GetStockProduitAsync(produitId);

        public Task<bool> TransfererStock(int produitId, int depotSource, int depotDest, int qte)
            => _repo.TransfertStockAsync(produitId, depotSource, depotDest, qte);

        public Task<bool> AjusterStock(int produitId, int depotId, int nouvelleQte)
            => _repo.AjusterStockAsync(produitId, depotId, nouvelleQte);

        public Task<List<MouvementStock>> GetMouvements(
            DateTime? du = null,
            DateTime? au = null,
            int? produitId = null,
            int? depotId = null)
            => _repo.GetMouvementsAsync(du, au, produitId, depotId);

    }
}
