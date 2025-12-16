using GestionCommercial.Data;
using GestionCommercial.Models;

namespace GestionCommercial.Services
{
    public class DepotService
    {
        private readonly DepotRepository _repo;

        public DepotService(DepotRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Depot>> GetDepots(bool includeInactive = false)
            => _repo.GetDepotsAsync(includeInactive);

        public Task<Depot?> GetDepot(int id)
            => _repo.GetDepotByIdAsync(id);

        public Task<int> CreateDepot(Depot d)
            => _repo.CreateDepotAsync(d);

        public Task UpdateDepot(Depot d)
            => _repo.UpdateDepotAsync(d);

        public Task SetActif(int id, bool actif)
            => _repo.SetActifAsync(id, actif);
    }
}
