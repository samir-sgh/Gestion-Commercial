using GestionCommercial.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GestionCommercial.Data
{
    public class VenteRepository
    {
        private readonly IConfiguration _config;

        public VenteRepository(IConfiguration config)
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

        // --------- COMBOS: Clients / Depots / Produits ---------

        public async Task<List<Tiers>> GetClientsAsync()
        {
            var list = new List<Tiers>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT id_tiers, nom_tiers, type_tiers
                FROM dbo.Tiers
                WHERE type_tiers = 'Client'
                ORDER BY nom_tiers;", cn);

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new Tiers
                {
                    IdTiers = rdr.GetInt32(0),
                    NomTiers = rdr.GetString(1),
                    TypeTiers = rdr.GetString(2)
                });
            }
            return list;
        }

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
                SELECT produit_id, nom_article, ISNULL(prix_vente,0)
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
                    PrixAchat = rdr.GetDecimal(2)   // ممكن تضيف PrixVente خاص فالموديل من بعد
                });
            }
            return list;
        }

        // --------- LISTE VENTES ---------

        public async Task<List<Vente>> GetListeVentesAsync()
        {
            var list = new List<Vente>();

            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
        SELECT v.vente_id,
               v.date_vente,
               v.type_document,
               v.montant_total,
               v.statut,
               v.remarque,
               v.id_tiers,
               t.nom_tiers
        FROM dbo.Ventes v
        JOIN dbo.Tiers t  ON t.id_tiers = v.id_tiers
        ORDER BY v.date_vente DESC, v.vente_id DESC;", cn);

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                list.Add(new Vente
                {
                    VenteId = rdr.GetInt32(0),
                    DateVente = rdr.IsDBNull(1) ? DateTime.MinValue : rdr.GetDateTime(1),
                    TypeDocument = rdr.GetString(2),
                    MontantTotal = rdr.IsDBNull(3) ? 0 : rdr.GetDecimal(3),
                    Statut = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                    Remarque = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                    IdTiers = rdr.GetInt32(6),
                    ClientNom = rdr.GetString(7),

                    // حيث ما عندناش dépôt فـ Ventes دابا
                    IdDepot = 0,
                    DepotNom = string.Empty
                });
            }

            return list;
        }


        // --------- DETAIL + LIGNES ---------

        public async Task<Vente?> GetVenteAvecLignesAsync(int venteId)
        {
            Vente? vente = null;
            using var cn = CreateConnection();
            await cn.OpenAsync();

            // header
            using (var cmd = new SqlCommand(@"
                SELECT v.vente_id,
                       v.date_vente,
                       v.type_document,
                       v.montant_total,
                       v.statut,
                       v.remarque,
                       v.id_tiers,
                       t.nom_tiers
                FROM dbo.Ventes v
                JOIN dbo.Tiers t ON t.id_tiers = v.id_tiers
                WHERE v.vente_id = @id;", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = venteId;
                using var rdr = await cmd.ExecuteReaderAsync();
                if (await rdr.ReadAsync())
                {
                    vente = new Vente
                    {
                        VenteId = rdr.GetInt32(0),
                        DateVente = rdr.GetDateTime(1),
                        TypeDocument = rdr.GetString(2),
                        MontantTotal = rdr.IsDBNull(3) ? 0 : rdr.GetDecimal(3),
                        Statut = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                        Remarque = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                        IdTiers = rdr.GetInt32(6),
                        ClientNom = rdr.GetString(7)
                    };
                }
            }

            if (vente == null) return null;

            // lignes
            using (var cmdL = new SqlCommand(@"
                SELECT lv.ligne_vente_id,
                       lv.produit_id,
                       p.nom_article,
                       lv.qte_vendue,
                       lv.prix_vente_unitaire,
                       lv.remise
                FROM dbo.LignesVentes lv
                JOIN dbo.produits p ON p.produit_id = lv.produit_id
                WHERE lv.vente_id = @id;", cn))
            {
                cmdL.Parameters.Add("@id", SqlDbType.Int).Value = venteId;
                using var rdrL = await cmdL.ExecuteReaderAsync();
                while (await rdrL.ReadAsync())
                {
                    vente.Lignes.Add(new LigneVente
                    {
                        LigneVenteId = rdrL.GetInt32(0),
                        VenteId = venteId,
                        ProduitId = rdrL.GetInt32(1),
                        NomArticle = rdrL.GetString(2),
                        QteVendue = rdrL.GetInt32(3),
                        PrixVenteUnitaire = rdrL.GetDecimal(4),
                        Remise = rdrL.IsDBNull(5) ? null : rdrL.GetDecimal(5)
                    });
                }
            }

            return vente;
        }

        // --------- INSERT VENTE + LIGNES ---------

        public async Task<int> CreerVenteAsync(Vente vente)
        {
            using var cn = CreateConnection();
            await cn.OpenAsync();
            using var tx = cn.BeginTransaction();

            try
            {
                using var cmdV = new SqlCommand(@"
                    INSERT INTO dbo.Ventes
                        (id_tiers, date_vente, type_document,
                         montant_total, remarque, statut)
                    VALUES
                        (@client, @date, @type,
                         @montant, @remarque, @statut);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tx);

                cmdV.Parameters.Add("@client", SqlDbType.Int).Value = vente.IdTiers;
                cmdV.Parameters.Add("@date", SqlDbType.Date).Value = vente.DateVente;
                cmdV.Parameters.Add("@type", SqlDbType.NVarChar, 50).Value = vente.TypeDocument;
                cmdV.Parameters.Add("@montant", SqlDbType.Decimal).Value = vente.MontantTotal;
                cmdV.Parameters.Add("@remarque", SqlDbType.NVarChar, 500).Value = (object?)vente.Remarque ?? DBNull.Value;
                cmdV.Parameters.Add("@statut", SqlDbType.NVarChar, 50).Value = (object?)vente.Statut ?? DBNull.Value;

                int newId = Convert.ToInt32(await cmdV.ExecuteScalarAsync());

                foreach (var l in vente.Lignes)
                {
                    using var cmdL = new SqlCommand(@"
                        INSERT INTO dbo.LignesVentes
                            (vente_id, produit_id, qte_vendue, prix_vente_unitaire, remise, total_ligne)
                        VALUES
                            (@vente, @prod, @qte, @prix, @remise, @total);", cn, tx);

                    cmdL.Parameters.Add("@vente", SqlDbType.Int).Value = newId;
                    cmdL.Parameters.Add("@prod", SqlDbType.Int).Value = l.ProduitId;
                    cmdL.Parameters.Add("@qte", SqlDbType.Int).Value = l.QteVendue;
                    cmdL.Parameters.Add("@prix", SqlDbType.Decimal).Value = l.PrixVenteUnitaire;
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
