using GestionCommercial.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GestionCommercial.Data
{
    public class ModePaiementRepository
    {
        private readonly IConfiguration _config;

        public ModePaiementRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection CreateConnection()
        {
            var cs = _config.GetConnectionString("DefaultConnection");
            return new SqlConnection(cs);
        }

        public async Task<List<ModePaiement>> GetModesAsync()
        {
            var list = new List<ModePaiement>();

            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT IdModePaiement, Code, Libelle, Actif
                FROM ModePaiement
                WHERE Actif = 1
                ORDER BY Libelle;", cn);

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                list.Add(new ModePaiement
                {
                    IdModePaiement = rdr.GetInt32(0),
                    Code = rdr.GetString(1),
                    Libelle = rdr.GetString(2),
                    Actif = rdr.GetBoolean(3)
                });
            }

            return list;
        }

        public async Task<int> CreateModeAsync(ModePaiement m)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO ModePaiement (Code, Libelle, Actif)
                VALUES (@code, @lib, @actif);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", cn);

            cmd.Parameters.Add("@code", SqlDbType.NVarChar, 50).Value = m.Code;
            cmd.Parameters.Add("@lib", SqlDbType.NVarChar, 100).Value = m.Libelle;
            cmd.Parameters.Add("@actif", SqlDbType.Bit).Value = m.Actif;

            await cn.OpenAsync();
            var id = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(id);
        }
    }
}
