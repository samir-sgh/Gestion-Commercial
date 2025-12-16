using GestionCommercial.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GestionCommercial.Data
{
    public class TiersRepository
    {
        private readonly IConfiguration _config;

        public TiersRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection CreateConnection()
        {
            var cs = _config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("ConnectionString 'DefaultConnection' est vide ou manquante.");

            return new SqlConnection(cs);
        }

        // ========= LISTE TIERS (avec filtre optionnel) =========
        public async Task<List<Tiers>> GetTiersAsync(string? typeFilter = null)
        {
            var list = new List<Tiers>();

            using var cn = CreateConnection();

            var sql = @"
                SELECT  id_tiers,
                        nom_tiers,
                        type_tiers,
                        adresse,
                        telephone,
                        email,
                        ville,
                        IdModePaiementDefault
                FROM dbo.Tiers";

            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                sql += " WHERE type_tiers = @type";
            }

            sql += " ORDER BY nom_tiers;";

            using var cmd = new SqlCommand(sql, cn);

            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                cmd.Parameters.Add("@type", SqlDbType.NVarChar, 50).Value = typeFilter;
            }

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                var t = new Tiers
                {
                    IdTiers = rdr.GetInt32(0),
                    NomTiers = rdr.GetString(1),
                    TypeTiers = rdr.GetString(2),
                    Adresse = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                    Telephone = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                    Email = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                    Ville = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                    IdModePaiementDefault = rdr.IsDBNull(7) ? (int?)null : rdr.GetInt32(7)
                };

                list.Add(t);
            }

            return list;
        }

        // ========= GET BY TYPE =========
        public async Task<List<Tiers>> GetTiersByTypeAsync(string type)
        {
            var list = new List<Tiers>();

            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT  id_tiers,
                        nom_tiers,
                        type_tiers,
                        adresse,
                        telephone,
                        email,
                        ville,
                        IdModePaiementDefault
                FROM dbo.Tiers
                WHERE type_tiers = @type
                ORDER BY nom_tiers;", cn);

            cmd.Parameters.Add("@type", SqlDbType.NVarChar, 50).Value = type;

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                var t = new Tiers
                {
                    IdTiers = rdr.GetInt32(0),
                    NomTiers = rdr.GetString(1),
                    TypeTiers = rdr.GetString(2),
                    Adresse = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                    Telephone = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                    Email = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                    Ville = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                    IdModePaiementDefault = rdr.IsDBNull(7) ? (int?)null : rdr.GetInt32(7)
                    
                };

                list.Add(t);
            }

            return list;
        }

        // ========= GET BY ID =========
        public async Task<Tiers?> GetTiersByIdAsync(int idTiers)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT  id_tiers,
                        nom_tiers,
                        type_tiers,
                        adresse,
                        telephone,
                        email,
                        ville,
                        IdModePaiementDefault
                FROM dbo.Tiers
                WHERE id_tiers = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idTiers;

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
            {
                return new Tiers
                {
                    IdTiers = rdr.GetInt32(0),
                    NomTiers = rdr.GetString(1),
                    TypeTiers = rdr.GetString(2),
                    Adresse = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                    Telephone = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                    Email = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                    Ville = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                    IdModePaiementDefault = rdr.IsDBNull(7) ? (int?)null : rdr.GetInt32(7)
                };
            }

            return null;
        }

        // ========= INSERT =========
        public async Task<int> CreateTiersAsync(Tiers t)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO dbo.Tiers
                    (nom_tiers,
                     type_tiers,
                     adresse,
                     telephone,
                     email,
                     ville,
                     IdModePaiementDefault)
                VALUES
                    (@nom,
                     @type,
                     @adr,
                     @tel,
                     @mail,
                     @ville,
                     @idMode);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", cn);

            cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 150).Value = t.NomTiers;
            cmd.Parameters.Add("@type", SqlDbType.NVarChar, 50).Value = t.TypeTiers;
            cmd.Parameters.Add("@adr", SqlDbType.NVarChar, 250).Value = (object?)t.Adresse ?? DBNull.Value;
            cmd.Parameters.Add("@tel", SqlDbType.NVarChar, 30).Value = (object?)t.Telephone ?? DBNull.Value;
            cmd.Parameters.Add("@mail", SqlDbType.NVarChar, 150).Value = (object?)t.Email ?? DBNull.Value;
            cmd.Parameters.Add("@ville", SqlDbType.NVarChar, 100).Value = (object?)t.Ville ?? DBNull.Value;
            cmd.Parameters.Add("@idMode", SqlDbType.Int).Value = (object?)t.IdModePaiementDefault ?? DBNull.Value;

            await cn.OpenAsync();
            var newIdObj = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(newIdObj);
        }

        // ========= UPDATE =========
        public async Task UpdateTiersAsync(Tiers t)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                UPDATE dbo.Tiers
                SET nom_tiers  = @nom,
                    type_tiers = @type,
                    adresse    = @adr,
                    telephone  = @tel,
                    email      = @mail,
                    ville      = @ville,
                    IdModePaiementDefault = @idMode
                WHERE id_tiers = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = t.IdTiers;
            cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 150).Value = t.NomTiers;
            cmd.Parameters.Add("@type", SqlDbType.NVarChar, 50).Value = t.TypeTiers;
            cmd.Parameters.Add("@adr", SqlDbType.NVarChar, 250).Value = (object?)t.Adresse ?? DBNull.Value;
            cmd.Parameters.Add("@tel", SqlDbType.NVarChar, 30).Value = (object?)t.Telephone ?? DBNull.Value;
            cmd.Parameters.Add("@mail", SqlDbType.NVarChar, 150).Value = (object?)t.Email ?? DBNull.Value;
            cmd.Parameters.Add("@ville", SqlDbType.NVarChar, 100).Value = (object?)t.Ville ?? DBNull.Value;
            cmd.Parameters.Add("@idMode", SqlDbType.Int).Value = (object?)t.IdModePaiementDefault ?? DBNull.Value;

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ========= DELETE =========
        public async Task DeleteTiersAsync(int idTiers)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                DELETE FROM dbo.Tiers WHERE id_tiers = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idTiers;

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
