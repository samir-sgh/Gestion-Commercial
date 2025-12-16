using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using GestionCommercial.Models;

namespace GestionCommercial.Data
{
    public class UtilisateurRepository
    {
        private readonly IConfiguration _config;

        public UtilisateurRepository(IConfiguration config)
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

        // ====== Authentification (عندك مسبقاً غالباً) ======
        public async Task<Utilisateur?> AuthenticateAsync(string username, string password)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 id_utilisateur, nom_utilisateur, mot_de_passe, role,
                       ISNULL(actif,1) AS actif, ISNULL(date_creation, GETDATE()) AS date_creation
                FROM dbo.Utilisateurs
                WHERE nom_utilisateur = @u
                  AND mot_de_passe = @p
                  AND ISNULL(actif,1) = 1;", cn);

            cmd.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = username;
            cmd.Parameters.Add("@p", SqlDbType.NVarChar, 255).Value = password;

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            if (!await rdr.ReadAsync())
                return null;

            return new Utilisateur
            {
                IdUtilisateur = rdr.GetInt32(rdr.GetOrdinal("id_utilisateur")),
                NomUtilisateur = rdr.GetString(rdr.GetOrdinal("nom_utilisateur")),
                MotDePasse = rdr.GetString(rdr.GetOrdinal("mot_de_passe")),
                Role = rdr.GetString(rdr.GetOrdinal("role")),
                Actif = rdr.GetBoolean(rdr.GetOrdinal("actif")),
                DateCreation = rdr.GetDateTime(rdr.GetOrdinal("date_creation"))
            };
        }

        // ====== Create ======

        public async Task CreerAsync(Utilisateur u)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO Utilisateurs (nom_utilisateur, mot_de_passe, role, actif, date_creation)
                VALUES (@nom, @pwd, @role, @actif, GETDATE())", cn);

            cmd.Parameters.AddWithValue("@nom", u.NomUtilisateur);
            cmd.Parameters.AddWithValue("@pwd", u.MotDePasse);
            cmd.Parameters.AddWithValue("@role", u.Role);
            cmd.Parameters.AddWithValue("@actif", u.Actif);

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        

        // ====== Liste complète ======
        public async Task<List<Utilisateur>> GetUtilisateursAsync(bool includeInactive = true)
        {
            var list = new List<Utilisateur>();

            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT id_utilisateur, nom_utilisateur, mot_de_passe, role,
                       ISNULL(actif,1) AS actif, ISNULL(date_creation, GETDATE()) AS date_creation
                FROM dbo.Utilisateurs
                WHERE (@all = 1 OR ISNULL(actif,1) = 1)
                ORDER BY nom_utilisateur;", cn);

            cmd.Parameters.Add("@all", SqlDbType.Bit).Value = includeInactive ? 1 : 0;

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new Utilisateur
                {
                    IdUtilisateur = rdr.GetInt32(rdr.GetOrdinal("id_utilisateur")),
                    NomUtilisateur = rdr.GetString(rdr.GetOrdinal("nom_utilisateur")),
                    MotDePasse = rdr.GetString(rdr.GetOrdinal("mot_de_passe")),
                    Role = rdr.GetString(rdr.GetOrdinal("role")),
                    Actif = rdr.GetBoolean(rdr.GetOrdinal("actif")),
                    DateCreation = rdr.GetDateTime(rdr.GetOrdinal("date_creation"))
                });
            }

            return list;
        }

        // ====== Get by Id ======
        public async Task<Utilisateur?> GetByIdAsync(int id)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT id_utilisateur, nom_utilisateur, mot_de_passe, role,
                       ISNULL(actif,1) AS actif, ISNULL(date_creation, GETDATE()) AS date_creation
                FROM dbo.Utilisateurs
                WHERE id_utilisateur = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            if (!await rdr.ReadAsync())
                return null;

            return new Utilisateur
            {
                IdUtilisateur = rdr.GetInt32(rdr.GetOrdinal("id_utilisateur")),
                NomUtilisateur = rdr.GetString(rdr.GetOrdinal("nom_utilisateur")),
                MotDePasse = rdr.GetString(rdr.GetOrdinal("mot_de_passe")),
                Role = rdr.GetString(rdr.GetOrdinal("role")),
                Actif = rdr.GetBoolean(rdr.GetOrdinal("actif")),
                DateCreation = rdr.GetDateTime(rdr.GetOrdinal("date_creation"))
            };
        }

        // ====== Insert ======
        public async Task<int> InsertAsync(Utilisateur u)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO dbo.Utilisateurs
                    (nom_utilisateur, mot_de_passe, role, actif, date_creation)
                VALUES
                    (@nom, @pwd, @role, @actif, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS int);", cn);

            cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 100).Value = u.NomUtilisateur;
            cmd.Parameters.Add("@pwd", SqlDbType.NVarChar, 255).Value = u.MotDePasse;
            cmd.Parameters.Add("@role", SqlDbType.NVarChar, 50).Value = u.Role;
            cmd.Parameters.Add("@actif", SqlDbType.Bit).Value = u.Actif ? 1 : 0;

            await cn.OpenAsync();
            var id = (int)(await cmd.ExecuteScalarAsync() ?? 0);
            return id;
        }

        // ====== Update (nom + role + actif) ======
        public async Task MettreAJourAsync(Utilisateur u)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                UPDATE dbo.Utilisateurs
                SET nom_utilisateur = @nom,
                    role            = @role,
                    actif           = @actif
                WHERE id_utilisateur = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = u.IdUtilisateur;
            cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 100).Value = u.NomUtilisateur;
            cmd.Parameters.Add("@role", SqlDbType.NVarChar, 50).Value = u.Role;
            cmd.Parameters.Add("@actif", SqlDbType.Bit).Value = u.Actif ? 1 : 0;

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ====== Changer mot de passe ======
        public async Task ChangerMotDePasseAsync(int id, string nouveauMdp)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                UPDATE dbo.Utilisateurs
                SET mot_de_passe = @pwd
                WHERE id_utilisateur = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@pwd", SqlDbType.NVarChar, 255).Value = nouveauMdp;

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ====== Supprimer ======
        public async Task SupprimerAsync(int id)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(
                "DELETE FROM dbo.Utilisateurs WHERE id_utilisateur = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
