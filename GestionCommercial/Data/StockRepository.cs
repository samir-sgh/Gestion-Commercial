using GestionCommercial.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GestionCommercial.Data
{
    public class StockRepository
    {
        private readonly IConfiguration _config;

        public StockRepository(IConfiguration config)
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

        // ================== STOCK ACTUEL (tous dépôts) ==================

        public async Task<List<ProduitStock>> GetStockActuelAsync()
        {
            var list = new List<ProduitStock>();

            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT sa.produit_id,
                       p.nom_article,
                       sa.id_depot,
                       d.nom_depot,
                       ISNULL(sa.quantite_reelle, 0) AS quantite_reelle
                FROM dbo.StockActuel sa
                JOIN dbo.produits p ON p.produit_id = sa.produit_id
                JOIN dbo.Depot    d ON d.id_depot    = sa.id_depot
                ORDER BY p.nom_article, d.nom_depot;", cn);

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                list.Add(new ProduitStock
                {
                    ProduitId = rdr.GetInt32(0),
                    NomArticle = rdr.GetString(1),
                    IdDepot = rdr.GetInt32(2),
                    NomDepot = rdr.GetString(3),
                    QuantiteReelle = rdr.IsDBNull(4) ? 0 : rdr.GetInt32(4)
                });
            }

            return list;
        }

        // ================== STOCK PAR PRODUIT ==================

        public async Task<List<ProduitStock>> GetStockProduitAsync(int produitId)
        {
            var list = new List<ProduitStock>();

            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT sa.id_depot,
                       d.nom_depot,
                       ISNULL(sa.quantite_reelle, 0) AS quantite_reelle
                FROM dbo.StockActuel sa
                JOIN dbo.Depot d ON d.id_depot = sa.id_depot
                WHERE sa.produit_id = @p
                ORDER BY d.nom_depot;", cn);

            cmd.Parameters.Add("@p", SqlDbType.Int).Value = produitId;

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                list.Add(new ProduitStock
                {
                    ProduitId = produitId,
                    IdDepot = rdr.GetInt32(0),
                    NomDepot = rdr.GetString(1),
                    QuantiteReelle = rdr.IsDBNull(2) ? 0 : rdr.GetInt32(2)
                });
            }

            return list;
        }

        // ================== TRANSFERT STOCK ==================

        public async Task<bool> TransfertStockAsync(int produitId, int depotSource, int depotDest, int quantite)
        {
            if (quantite <= 0) return false;

            using var cn = CreateConnection();
            await cn.OpenAsync();
            using var tr = cn.BeginTransaction();

            try
            {
                // ---- 1) الكمية قبل النقل فالمصدر
                int qteSourceAvant;
                using (var cmdCheck = new SqlCommand(@"
            SELECT ISNULL(quantite_reelle,0)
            FROM dbo.StockActuel
            WHERE produit_id=@p AND id_depot=@d;", cn, tr))
                {
                    cmdCheck.Parameters.Add("@p", SqlDbType.Int).Value = produitId;
                    cmdCheck.Parameters.Add("@d", SqlDbType.Int).Value = depotSource;

                    var obj = await cmdCheck.ExecuteScalarAsync();
                    qteSourceAvant = obj == null ? 0 : Convert.ToInt32(obj);
                }

                if (qteSourceAvant < quantite)
                    throw new InvalidOperationException("Quantité insuffisante dans le dépôt source.");

                int qteSourceApres = qteSourceAvant - quantite;

                // ---- 2) نقص من المصدر
                using (var cmdSrc = new SqlCommand(@"
            UPDATE dbo.StockActuel
            SET quantite_reelle = quantite_reelle - @q,
                date_derniere_maj = GETDATE()
            WHERE produit_id=@p AND id_depot=@d;", cn, tr))
                {
                    cmdSrc.Parameters.Add("@q", SqlDbType.Int).Value = quantite;
                    cmdSrc.Parameters.Add("@p", SqlDbType.Int).Value = produitId;
                    cmdSrc.Parameters.Add("@d", SqlDbType.Int).Value = depotSource;

                    await cmdSrc.ExecuteNonQueryAsync();
                }

                // ---- 3) quantité قبل وبعد فالوجهة
                int qteDestAvant;
                using (var cmdCheckDest = new SqlCommand(@"
            SELECT ISNULL(quantite_reelle,0)
            FROM dbo.StockActuel
            WHERE produit_id=@p AND id_depot=@d;", cn, tr))
                {
                    cmdCheckDest.Parameters.Add("@p", SqlDbType.Int).Value = produitId;
                    cmdCheckDest.Parameters.Add("@d", SqlDbType.Int).Value = depotDest;

                    var obj = await cmdCheckDest.ExecuteScalarAsync();
                    qteDestAvant = obj == null ? 0 : Convert.ToInt32(obj);
                }

                int qteDestApres = qteDestAvant + quantite;

                // ---- 4) زيد فالوجهة (update أو insert)
                int rows;
                using (var cmdDestUpdate = new SqlCommand(@"
            UPDATE dbo.StockActuel
            SET quantite_reelle = quantite_reelle + @q,
                date_derniere_maj = GETDATE()
            WHERE produit_id=@p AND id_depot=@d;", cn, tr))
                {
                    cmdDestUpdate.Parameters.Add("@q", SqlDbType.Int).Value = quantite;
                    cmdDestUpdate.Parameters.Add("@p", SqlDbType.Int).Value = produitId;
                    cmdDestUpdate.Parameters.Add("@d", SqlDbType.Int).Value = depotDest;

                    rows = await cmdDestUpdate.ExecuteNonQueryAsync();
                }

                if (rows == 0)
                {
                    using var cmdDestInsert = new SqlCommand(@"
                INSERT INTO dbo.StockActuel
                    (produit_id, id_depot, quantite_reelle, date_derniere_maj)
                VALUES
                    (@p, @d, @q, GETDATE());", cn, tr);

                    cmdDestInsert.Parameters.Add("@p", SqlDbType.Int).Value = produitId;
                    cmdDestInsert.Parameters.Add("@d", SqlDbType.Int).Value = depotDest;
                    cmdDestInsert.Parameters.Add("@q", SqlDbType.Int).Value = quantite;

                    await cmdDestInsert.ExecuteNonQueryAsync();
                }

                // ---- 5) سجّل حركتين فـ MouvementsStock
                await LogMouvementAsync(
                    cn, tr,
                    produitId,
                    depotSource,
                    "Transfert-",
                    quantite,
                    qteSourceAvant,
                    qteSourceApres,
                    "Transfert",
                    null);

                await LogMouvementAsync(
                    cn, tr,
                    produitId,
                    depotDest,
                    "Transfert+",
                    quantite,
                    qteDestAvant,
                    qteDestApres,
                    "Transfert",
                    null);

                tr.Commit();
                return true;
            }
            catch
            {
                tr.Rollback();
                throw;
            }
        }


        // ================== AJUSTER STOCK (mettre une nouvelle quantité) ==================

        public async Task<bool> AjusterStockAsync(int produitId, int depotId, int nouvelleQuantite)
        {
            using var cn = CreateConnection();
            await cn.OpenAsync();
            using var tr = cn.BeginTransaction();

            try
            {
                // الكمية قبل
                int qteAvant;
                using (var cmdCheck = new SqlCommand(@"
            SELECT ISNULL(quantite_reelle,0)
            FROM dbo.StockActuel
            WHERE produit_id=@p AND id_depot=@d;", cn, tr))
                {
                    cmdCheck.Parameters.Add("@p", SqlDbType.Int).Value = produitId;
                    cmdCheck.Parameters.Add("@d", SqlDbType.Int).Value = depotId;

                    var obj = await cmdCheck.ExecuteScalarAsync();
                    qteAvant = obj == null ? 0 : Convert.ToInt32(obj);
                }

                int qteApres = nouvelleQuantite;

                // UPDATE
                int rows;
                using (var cmd = new SqlCommand(@"
            UPDATE dbo.StockActuel
            SET quantite_reelle = @q,
                date_derniere_maj = GETDATE()
            WHERE produit_id=@p AND id_depot=@d;", cn, tr))
                {
                    cmd.Parameters.Add("@q", SqlDbType.Int).Value = nouvelleQuantite;
                    cmd.Parameters.Add("@p", SqlDbType.Int).Value = produitId;
                    cmd.Parameters.Add("@d", SqlDbType.Int).Value = depotId;

                    rows = await cmd.ExecuteNonQueryAsync();
                }

                // INSERT si pas trouvé
                if (rows == 0)
                {
                    using var cmdIns = new SqlCommand(@"
                INSERT INTO dbo.StockActuel
                    (produit_id, id_depot, quantite_reelle, date_derniere_maj)
                VALUES
                    (@p, @d, @q, GETDATE());", cn, tr);

                    cmdIns.Parameters.Add("@p", SqlDbType.Int).Value = produitId;
                    cmdIns.Parameters.Add("@d", SqlDbType.Int).Value = depotId;
                    cmdIns.Parameters.Add("@q", SqlDbType.Int).Value = nouvelleQuantite;

                    await cmdIns.ExecuteNonQueryAsync();
                }

                // log mouvement
                int delta = qteApres - qteAvant;
                await LogMouvementAsync(
                    cn, tr,
                    produitId,
                    depotId,
                    "Ajustement",
                    delta,
                    qteAvant,
                    qteApres,
                    "Inventaire",
                    null);

                tr.Commit();
                return true;
            }
            catch
            {
                tr.Rollback();
                throw;
            }
        }


        private async Task LogMouvementAsync(
            SqlConnection cn,
            SqlTransaction tr,
            int produitId,
            int depotId,
            string typeMouvement,
            int qte,
            int? qteAvant,
            int? qteApres,
            string? sourceDoc = null,
            int? idDoc = null)
                {
                    using var cmd = new SqlCommand(@"
                INSERT INTO dbo.MouvementsStock
                    (produit_id, id_depot, date_mouvement,
                     type_mouvement, qte, qte_avant, qte_apres,
                     source_doc, id_doc)
                VALUES
                    (@p, @d, GETDATE(),
                     @type, @q, @avant, @apres,
                     @src, @doc);", cn, tr);

                    cmd.Parameters.Add("@p", SqlDbType.Int).Value = produitId;
                    cmd.Parameters.Add("@d", SqlDbType.Int).Value = depotId;
                    cmd.Parameters.Add("@type", SqlDbType.NVarChar, 50).Value = typeMouvement;
                    cmd.Parameters.Add("@q", SqlDbType.Int).Value = qte;
                    cmd.Parameters.Add("@avant", SqlDbType.Int).Value = (object?)qteAvant ?? DBNull.Value;
                    cmd.Parameters.Add("@apres", SqlDbType.Int).Value = (object?)qteApres ?? DBNull.Value;
                    cmd.Parameters.Add("@src", SqlDbType.NVarChar, 50).Value = (object?)sourceDoc ?? DBNull.Value;
                    cmd.Parameters.Add("@doc", SqlDbType.Int).Value = (object?)idDoc ?? DBNull.Value;

                    await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<MouvementStock>> GetMouvementsAsync(
            DateTime? du = null,
            DateTime? au = null,
            int? produitId = null,
            int? depotId = null)
                {
                    var list = new List<MouvementStock>();

                    using var cn = CreateConnection();

                    var sql = @"
                SELECT m.mouvement_id,
                       m.date_mouvement,
                       m.type_mouvement,
                       m.qte,
                       m.qte_avant,
                       m.qte_apres,
                       m.source_doc,
                       m.id_doc,
                       p.produit_id,
                       p.nom_article,
                       d.id_depot,
                       d.nom_depot
                FROM dbo.MouvementsStock m
                JOIN dbo.produits p ON p.produit_id = m.produit_id
                JOIN dbo.Depot    d ON d.id_depot    = m.id_depot
                WHERE 1 = 1";

                    var cmd = new SqlCommand();
                    cmd.Connection = cn;

                    if (du.HasValue)
                    {
                        sql += " AND m.date_mouvement >= @du";
                        cmd.Parameters.Add("@du", SqlDbType.DateTime).Value = du.Value.Date;
                    }
                    if (au.HasValue)
                    {
                        sql += " AND m.date_mouvement < @au";
                        cmd.Parameters.Add("@au", SqlDbType.DateTime).Value = au.Value.Date.AddDays(1);
                    }
                    if (produitId.HasValue)
                    {
                        sql += " AND m.produit_id = @prod";
                        cmd.Parameters.Add("@prod", SqlDbType.Int).Value = produitId.Value;
                    }
                    if (depotId.HasValue)
                    {
                        sql += " AND m.id_depot = @dep";
                        cmd.Parameters.Add("@dep", SqlDbType.Int).Value = depotId.Value;
                    }

                    sql += " ORDER BY m.date_mouvement DESC, m.mouvement_id DESC;";

                    cmd.CommandText = sql;

                    await cn.OpenAsync();
                    using var rdr = await cmd.ExecuteReaderAsync();

                    while (await rdr.ReadAsync())
                    {
                        list.Add(new MouvementStock
                        {
                            MouvementId = rdr.GetInt32(0),
                            DateMouvement = rdr.GetDateTime(1),
                            TypeMouvement = rdr.GetString(2),
                            Qte = rdr.GetInt32(3),
                            QteAvant = rdr.IsDBNull(4) ? (int?)null : rdr.GetInt32(4),
                            QteApres = rdr.IsDBNull(5) ? (int?)null : rdr.GetInt32(5),
                            SourceDoc = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                            IdDoc = rdr.IsDBNull(7) ? (int?)null : rdr.GetInt32(7),
                            ProduitId = rdr.GetInt32(8),
                            NomArticle = rdr.GetString(9),
                            IdDepot = rdr.GetInt32(10),
                            NomDepot = rdr.GetString(11)
                        });
                    }

                    return list;
        }


    }
}
