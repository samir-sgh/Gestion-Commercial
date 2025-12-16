using GestionCommercial.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GestionCommercial.Data
{
    public class DepotRepository
    {
        private readonly IConfiguration _config;

        public DepotRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection CreateConnection()
        {
            var cs = _config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("ConnectionString 'DefaultConnection' manquante.");
            return new SqlConnection(cs);
        }

        // ========= Lire tous les dépôts =========
        public async Task<List<Depot>> GetDepotsAsync(bool includeInactive = false)
        {
            var list = new List<Depot>();

            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT id_depot, nom_depot, code_depot, adresse, actif, created_at
                FROM dbo.Depot
                WHERE (@all = 1 OR actif = 1)
                ORDER BY nom_depot;", cn);

            cmd.Parameters.Add("@all", SqlDbType.Bit).Value = includeInactive ? 1 : 0;

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                list.Add(new Depot
                {
                    IdDepot = rdr.GetInt32(0),
                    NomDepot = rdr.GetString(1),
                    CodeDepot = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                    Adresse = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                    Actif = rdr.GetBoolean(4),
                    CreatedAt = rdr.GetDateTime(5)
                });
            }

            return list;
        }

        // ========= Lire un seul dépôt =========
        public async Task<Depot?> GetDepotByIdAsync(int idDepot)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT id_depot, nom_depot, code_depot, adresse, actif, created_at
                FROM dbo.Depot
                WHERE id_depot = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idDepot;

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            if (await rdr.ReadAsync())
            {
                return new Depot
                {
                    IdDepot = rdr.GetInt32(0),
                    NomDepot = rdr.GetString(1),
                    CodeDepot = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                    Adresse = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                    Actif = rdr.GetBoolean(4),
                    CreatedAt = rdr.GetDateTime(5)
                };
            }

            return null;
        }

        // ========= Créer =========
        public async Task<int> CreateDepotAsync(Depot depot)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO dbo.Depot
                    (nom_depot, code_depot, adresse, actif, created_at)
                VALUES
                    (@nom, @code, @adr, @actif, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS int);", cn);

            cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 120).Value = depot.NomDepot;
            cmd.Parameters.Add("@code", SqlDbType.NVarChar, 40).Value =
                (object?)depot.CodeDepot ?? DBNull.Value;
            cmd.Parameters.Add("@adr", SqlDbType.NVarChar, 200).Value =
                (object?)depot.Adresse ?? DBNull.Value;
            cmd.Parameters.Add("@actif", SqlDbType.Bit).Value = depot.Actif;

            await cn.OpenAsync();
            var obj = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(obj);
        }

        // ========= Update =========
        public async Task UpdateDepotAsync(Depot depot)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                UPDATE dbo.Depot
                SET nom_depot = @nom,
                    code_depot = @code,
                    adresse   = @adr,
                    actif     = @actif
                WHERE id_depot = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = depot.IdDepot;
            cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 120).Value = depot.NomDepot;
            cmd.Parameters.Add("@code", SqlDbType.NVarChar, 40).Value =
                (object?)depot.CodeDepot ?? DBNull.Value;
            cmd.Parameters.Add("@adr", SqlDbType.NVarChar, 200).Value =
                (object?)depot.Adresse ?? DBNull.Value;
            cmd.Parameters.Add("@actif", SqlDbType.Bit).Value = depot.Actif;

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ========= Activer / Désactiver (Soft delete) =========
        public async Task SetActifAsync(int idDepot, bool actif)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                UPDATE dbo.Depot
                SET actif = @actif
                WHERE id_depot = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idDepot;
            cmd.Parameters.Add("@actif", SqlDbType.Bit).Value = actif;

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
