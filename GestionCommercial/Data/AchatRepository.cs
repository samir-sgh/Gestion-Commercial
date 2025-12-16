using GestionCommercial.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GestionCommercial.Data
{
    public class AchatRepository
    {
        private readonly IConfiguration _config;

        public AchatRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection CreateConnection()
        {
            // يجيب ConnString من appsettings.json
            var cs = _config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("ConnectionString 'DefaultConnection' est vide ou manquante.");

            return new SqlConnection(cs);
        }

        //// ========== FOUNISSEURS / PRODUITS / DEPOTS POUR LES COMBOS ==========

        //public async Task<List<Tiers>> GetFournisseursAsync()
        //{
        //    var list = new List<Tiers>();

        //    using var cn = CreateConnection();
        //    using var cmd = new SqlCommand(@"
        //            SELECT id_tiers, nom_tiers, type_tiers
        //            FROM dbo.Tiers
        //            WHERE type_tiers = 'Fournisseur'
        //            ORDER BY nom_tiers;", cn);

        //    await cn.OpenAsync();
        //    using var rdr = await cmd.ExecuteReaderAsync();
        //    while (await rdr.ReadAsync())
        //    {
        //        list.Add(new Tiers
        //        {
        //            IdTiers = rdr.GetInt32(0),
        //            NomTiers = rdr.GetString(1),
        //            TypeTiers = rdr.GetString(2)
        //        });
        //    }

        //    return list;
        //}

        public async Task<List<Depot>> GetDepotsAsync()
        {
            var list = new List<Depot>();

            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                    SELECT id_depot, nom_depot
                    FROM dbo.Depot
                    ORDER BY nom_depot;", cn);

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new Depot
                {
                    IdDepot = rdr.GetInt32(0),
                    NomDepot = rdr.GetString(1)
                });
            }
            return list;
        }

        public async Task<List<Produit>> GetProduitsAsync()
        {
            var list = new List<Produit>();

            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                    SELECT produit_id, nom_article, ISNULL(prix_achat,0)
                    FROM dbo.produits
                    ORDER BY nom_article;", cn);

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new Produit
                {
                    ProduitId = rdr.GetInt32(0),
                    NomArticle = rdr.GetString(1),
                    PrixAchat = rdr.GetDecimal(2)
                });
            }
            return list;
        }

        // ========== LECTURE LISTE ACHATS ==========

        public async Task<List<Achat>> GetListeAchatsAsync()
        {
            var list = new List<Achat>();

            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT a.achat_id,
                       a.date_achat,
                       a.type_document,
                       a.montant_total,
                       a.statut,
                       a.remarque,
                       a.id_tiers,
                       t.nom_tiers,
                       a.id_depot,
                       d.nom_depot
                FROM dbo.Achats a
                JOIN dbo.Tiers  t ON t.id_tiers = a.id_tiers
                JOIN dbo.Depot  d ON d.id_depot = a.id_depot
                ORDER BY a.date_achat DESC, a.achat_id DESC;", cn);

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new Achat
                {
                    AchatId = rdr.GetInt32(0),
                    DateAchat = rdr.GetDateTime(1),
                    TypeDocument = rdr.GetString(2),
                    MontantTotal = rdr.IsDBNull(3) ? 0 : rdr.GetDecimal(3),
                    Statut = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                    Remarque = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                    IdTiers = rdr.GetInt32(6),
                    FournisseurNom = rdr.GetString(7),
                    IdDepot = rdr.GetInt32(8),
                    DepotNom = rdr.GetString(9)
                });
            }

            return list;
        }

        // ========== DETAIL ACHAT ==========

        public async Task<Achat?> GetAchatAvecLignesAsync(int achatId)
        {
            Achat? achat = null;

            using var cn = CreateConnection();
            await cn.OpenAsync();

            // 1) رأس الوثيقة
            using (var cmd = new SqlCommand(@"
                SELECT a.achat_id,
                       a.date_achat,
                       a.type_document,
                       a.montant_total,
                       a.statut,
                       a.remarque,
                       a.id_tiers,
                       t.nom_tiers,
                       a.id_depot,
                       d.nom_depot
                FROM dbo.Achats a
                JOIN dbo.Tiers t ON t.id_tiers = a.id_tiers
                JOIN dbo.Depot d ON d.id_depot = a.id_depot
                WHERE a.achat_id = @id;", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = achatId;

                using var rdr = await cmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync())
                {
                    achat = new Achat
                    {
                        AchatId = rdr.GetInt32(0),
                        DateAchat = rdr.GetDateTime(1),
                        TypeDocument = rdr.GetString(2),
                        MontantTotal = rdr.IsDBNull(3) ? 0 : rdr.GetDecimal(3),
                        Statut = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                        Remarque = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                        IdTiers = rdr.GetInt32(6),
                        FournisseurNom = rdr.GetString(7),
                        IdDepot = rdr.GetInt32(8),
                        DepotNom = rdr.GetString(9)
                    };
                }
            }

            if (achat == null) return null;

            // 2) خطوط الوثيقة
            using (var cmdL = new SqlCommand(@"
                SELECT la.ligne_achat_id,
                       la.produit_id,
                       p.nom_article,
                       la.qte_achetee,
                       la.prix_achat,
                       la.remise,
                       la.total_ligne
                FROM dbo.LignesAchats la
                JOIN dbo.produits p ON p.produit_id = la.produit_id
                WHERE la.achat_id = @id;", cn))
            {
                cmdL.Parameters.Add("@id", SqlDbType.Int).Value = achatId;

                using var rdrL = await cmdL.ExecuteReaderAsync();
                while (await rdrL.ReadAsync())
                {
                    achat.Lignes.Add(new LigneAchat
                    {
                        LigneAchatId = rdrL.GetInt32(0),
                        AchatId = achatId,
                        ProduitId = rdrL.GetInt32(1),
                        NomArticle = rdrL.GetString(2),
                        QteAchetee = rdrL.GetInt32(3),
                        PrixAchat = rdrL.GetDecimal(4),
                        Remise = rdrL.IsDBNull(5) ? null : rdrL.GetDecimal(5)
                    });
                }
            }

            return achat;
        }

        // ========== INSERT ACHAT + LIGNES ==========

        public async Task<int> CreerAchatAsync(Achat achat)
        {
            using var cn = CreateConnection();
            await cn.OpenAsync();
            using var tx = cn.BeginTransaction();

            try
            {
                // 1) Insert head
                using var cmdA = new SqlCommand(@"
                    INSERT INTO dbo.Achats
                        (id_tiers, id_depot, date_achat, type_document,
                         montant_total, statut, remarque)
                    VALUES
                        (@tiers, @depot, @date, @type,
                         @montant, @statut, @remarque);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tx);

                cmdA.Parameters.Add("@tiers", SqlDbType.Int).Value = achat.IdTiers;
                cmdA.Parameters.Add("@depot", SqlDbType.Int).Value = achat.IdDepot;
                cmdA.Parameters.Add("@date", SqlDbType.Date).Value = achat.DateAchat;
                cmdA.Parameters.Add("@type", SqlDbType.NVarChar, 50).Value = achat.TypeDocument;
                cmdA.Parameters.Add("@montant", SqlDbType.Decimal).Value = achat.MontantTotal;
                cmdA.Parameters.Add("@statut", SqlDbType.NVarChar, 50).Value = (object?)achat.Statut ?? DBNull.Value;
                cmdA.Parameters.Add("@remarque", SqlDbType.NVarChar, 500).Value = (object?)achat.Remarque ?? DBNull.Value;

                var newIdObj = await cmdA.ExecuteScalarAsync();
                int newId = Convert.ToInt32(newIdObj);

                // 2) Insert lignes
                foreach (var l in achat.Lignes)
                {
                    using var cmdL = new SqlCommand(@"
                        INSERT INTO dbo.LignesAchats
                            (achat_id, produit_id, qte_achetee, prix_achat, remise, total_ligne)
                        VALUES
                            (@achat, @prod, @qte, @prix, @remise, @total);", cn, tx);

                    cmdL.Parameters.Add("@achat", SqlDbType.Int).Value = newId;
                    cmdL.Parameters.Add("@prod", SqlDbType.Int).Value = l.ProduitId;
                    cmdL.Parameters.Add("@qte", SqlDbType.Int).Value = l.QteAchetee;
                    cmdL.Parameters.Add("@prix", SqlDbType.Decimal).Value = l.PrixAchat;
                    cmdL.Parameters.Add("@remise", SqlDbType.Decimal).Value = (object?)l.Remise ?? DBNull.Value;
                    cmdL.Parameters.Add("@total", SqlDbType.Decimal).Value = l.TotalLigne;

                    await cmdL.ExecuteNonQueryAsync();
                }

                tx.Commit();
                return newId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
