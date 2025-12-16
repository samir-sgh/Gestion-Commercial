using GestionCommercial.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace GestionCommercial.Data
{
    public class ProduitRepository
    {
        private readonly IConfiguration _config;

        public ProduitRepository(IConfiguration config)
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

        // ===== LISTE =====
        public async Task<List<Produit>> GetProduitsAsync()
        {
            var list = new List<Produit>();

            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT  produit_id,
                        nom_article,
                        reference,
                        marque,
                        ISNULL(quantite, 0)      AS quantite,
                        ISNULL(prix_achat, 0)    AS prix_achat,
                        ISNULL(prix_vente, 0)    AS prix_vente,
                        ISNULL(prix_gros, 0)     AS prix_gros,
                        code_barre,
                        image_path,
                        date_creation,
                        id_marque,
                        ISNULL(stock_min, 0)     AS stock_min,
                        ISNULL(favoris, 0)       AS favoris,
                        id_depot
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
                    Reference = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                    Marque = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                    Quantite = rdr.GetDecimal(4),
                    PrixAchat = rdr.GetDecimal(5),
                    PrixVente = rdr.GetDecimal(6),
                    PrixGros = rdr.GetDecimal(7),
                    CodeBarre = rdr.IsDBNull(8) ? null : rdr.GetString(8),
                    ImageData = rdr.IsDBNull(9) ? null : (byte[])rdr[9],
                    DateCreation = rdr.IsDBNull(10) ? (DateTime?)null : rdr.GetDateTime(10),
                    IdMarque = rdr.IsDBNull(11) ? (int?)null : rdr.GetInt32(11),
                    StockMin = rdr.GetInt32(12),
                    Favoris = rdr.GetBoolean(13),
                    IdDepot = rdr.IsDBNull(14) ? (int?)null : rdr.GetInt32(14)
                });
            }

            return list;
        }

        // ===== GET BY ID =====
        public async Task<Produit?> GetProduitByIdAsync(int id)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT  produit_id,
                        nom_article,
                        reference,
                        marque,
                        ISNULL(quantite, 0)      AS quantite,
                        ISNULL(prix_achat, 0)    AS prix_achat,
                        ISNULL(prix_vente, 0)    AS prix_vente,
                        ISNULL(prix_gros, 0)     AS prix_gros,
                        code_barre,
                        image_path,
                        date_creation,
                        id_marque,
                        ISNULL(stock_min, 0)     AS stock_min,
                        ISNULL(favoris, 0)       AS favoris,
                        id_depot
                FROM dbo.produits
                WHERE produit_id = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
            {
                return new Produit
                {
                    ProduitId = rdr.GetInt32(0),
                    NomArticle = rdr.GetString(1),
                    Reference = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                    Marque = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                    Quantite = rdr.GetDecimal(4),
                    PrixAchat = rdr.GetDecimal(5),
                    PrixVente = rdr.GetDecimal(6),
                    PrixGros = rdr.GetDecimal(7),
                    CodeBarre = rdr.IsDBNull(8) ? null : rdr.GetString(8),
                    ImageData = rdr.IsDBNull(9) ? null : (byte[])rdr[9],
                    DateCreation = rdr.IsDBNull(10) ? (DateTime?)null : rdr.GetDateTime(10),
                    IdMarque = rdr.IsDBNull(11) ? (int?)null : rdr.GetInt32(11),
                    StockMin = rdr.GetInt32(12),
                    Favoris = rdr.GetBoolean(13),
                    IdDepot = rdr.IsDBNull(14) ? (int?)null : rdr.GetInt32(14)
                };
            }

            return null;
        }

        // ===== INSERT =====
        public async Task<int> CreateProduitAsync(Produit p)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO dbo.produits
                    (nom_article,
                     reference,
                     marque,
                     quantite,
                     prix_achat,
                     prix_vente,
                     prix_gros,
                     code_barre,
                     image_path,
                     stock_min,
                     favoris,
                     id_depot,
                     date_creation)
                VALUES
                    (@nom,
                     @ref,
                     @marque,
                     @qte,
                     @pa,
                     @pv,
                     @pg,
                     @code,
                     @img,
                     @stockMin,
                     @favoris,
                     @idDepot,
                     GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);", cn);

            cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 300).Value = p.NomArticle;
            cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 200).Value = (object?)p.Reference ?? DBNull.Value;
            cmd.Parameters.Add("@marque", SqlDbType.NVarChar, 200).Value = (object?)p.Marque ?? DBNull.Value;
            cmd.Parameters.Add("@qte", SqlDbType.Decimal).Value = p.Quantite;
            cmd.Parameters.Add("@pa", SqlDbType.Decimal).Value = p.PrixAchat;
            cmd.Parameters.Add("@pv", SqlDbType.Decimal).Value = p.PrixVente;
            cmd.Parameters.Add("@pg", SqlDbType.Decimal).Value = p.PrixGros;
            cmd.Parameters.Add("@code", SqlDbType.NVarChar, 100).Value = (object?)p.CodeBarre ?? DBNull.Value;
            cmd.Parameters.Add("@img", SqlDbType.VarBinary, -1).Value = (object?)p.ImageData ?? DBNull.Value;
            cmd.Parameters.Add("@stockMin", SqlDbType.Int).Value = p.StockMin;
            cmd.Parameters.Add("@favoris", SqlDbType.Bit).Value = p.Favoris;
            cmd.Parameters.Add("@idDepot", SqlDbType.Int).Value = (object?)p.IdDepot ?? DBNull.Value;

            await cn.OpenAsync();
            var newIdObj = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(newIdObj);
        }

        // ===== UPDATE =====
        public async Task UpdateProduitAsync(Produit p)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                UPDATE dbo.produits
                SET nom_article = @nom,
                    reference   = @ref,
                    marque      = @marque,
                    quantite    = @qte,
                    prix_achat  = @pa,
                    prix_vente  = @pv,
                    prix_gros   = @pg,
                    code_barre  = @code,
                    image_path  = @img,
                    stock_min   = @stockMin,
                    favoris     = @favoris,
                    id_depot    = @idDepot
                WHERE produit_id = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = p.ProduitId;
            cmd.Parameters.Add("@nom", SqlDbType.NVarChar, 300).Value = p.NomArticle;
            cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 200).Value = (object?)p.Reference ?? DBNull.Value;
            cmd.Parameters.Add("@marque", SqlDbType.NVarChar, 200).Value = (object?)p.Marque ?? DBNull.Value;
            cmd.Parameters.Add("@qte", SqlDbType.Decimal).Value = p.Quantite;
            cmd.Parameters.Add("@pa", SqlDbType.Decimal).Value = p.PrixAchat;
            cmd.Parameters.Add("@pv", SqlDbType.Decimal).Value = p.PrixVente;
            cmd.Parameters.Add("@pg", SqlDbType.Decimal).Value = p.PrixGros;
            cmd.Parameters.Add("@code", SqlDbType.NVarChar, 100).Value = (object?)p.CodeBarre ?? DBNull.Value;
            cmd.Parameters.Add("@img", SqlDbType.VarBinary, -1).Value = (object?)p.ImageData ?? DBNull.Value;
            cmd.Parameters.Add("@stockMin", SqlDbType.Int).Value = p.StockMin;
            cmd.Parameters.Add("@favoris", SqlDbType.Bit).Value = p.Favoris;
            cmd.Parameters.Add("@idDepot", SqlDbType.Int).Value = (object?)p.IdDepot ?? DBNull.Value;

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ===== DELETE =====
        public async Task DeleteProduitAsync(int id)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                DELETE FROM dbo.produits
                WHERE produit_id = @id;", cn);

            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<string>> GetMarquesAsync()
        {
            var list = new List<string>();

            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
        SELECT DISTINCT marque
        FROM dbo.produits
        WHERE marque IS NOT NULL AND marque <> ''
        ORDER BY marque;", cn);

            await cn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();

            while (await rdr.ReadAsync())
            {
                list.Add(rdr.GetString(0));
            }

            return list;
        }

        public async Task InsertMarqueAsync(string marque)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
        INSERT INTO marques (nom_marque)
        VALUES (@m);
    ", cn);

            cmd.Parameters.AddWithValue("@m", marque);

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }


    }
}
