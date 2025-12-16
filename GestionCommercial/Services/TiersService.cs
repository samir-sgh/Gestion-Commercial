using GestionCommercial.Data;
using GestionCommercial.Models;

namespace GestionCommercial.Services
{
    public class TiersService
    {
        private readonly TiersRepository _repo;

        public TiersService(TiersRepository repo)
        {
            _repo = repo;
        }

        // لائحة التييرز مع فلتر اختياري حسب النوع (Client / Fournisseur / ...)
        public Task<List<Tiers>> GetTiers(string? typeFilter = null)
            => _repo.GetTiersAsync(typeFilter);

        // لائحة التييرز حسب النوع مباشرة
        public Task<List<Tiers>> GetTiersByType(string type)
            => _repo.GetTiersByTypeAsync(type);

        // واحد التييرز بالـ ID
        public Task<Tiers?> GetTiersById(int idTiers)
            => _repo.GetTiersByIdAsync(idTiers);

        // إنشاء تيرز جديد
        public Task<int> CreateTiers(Tiers tiers)
            => _repo.CreateTiersAsync(tiers);

        // تعديل تيرز
        public Task UpdateTiers(Tiers tiers)
            => _repo.UpdateTiersAsync(tiers);

        // حذف تيرز
        public Task DeleteTiers(int idTiers)
            => _repo.DeleteTiersAsync(idTiers);
    }
}
