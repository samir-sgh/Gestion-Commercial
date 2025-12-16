using GestionCommercial.Data;
using GestionCommercial.Models;

namespace GestionCommercial.Services
{
    public class ModePaiementService
    {
        private readonly ModePaiementRepository _repo;

        public ModePaiementService(ModePaiementRepository repo)
        {
            _repo = repo;
        }

        public Task<List<ModePaiement>> GetModes() => _repo.GetModesAsync();

        public Task<int> CreateMode(ModePaiement m) => _repo.CreateModeAsync(m);
    }
}
