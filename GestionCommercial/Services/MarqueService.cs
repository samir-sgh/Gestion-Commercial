using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;   // مهم بزاف
using Dapper;
using GestionCommercial.Models;

namespace GestionCommercial.Services
{
    public class MarqueService
    {
        private readonly string _conString;

        public MarqueService(IConfiguration config)
        {
            // اسم الكونّكشن ستـرِينغ من appsettings.json
            _conString = config.GetConnectionString("DefaultConnection");
        }

        // دالة صغيرة ترجع لينا Conn جديدة كل مرة
        private IDbConnection CreateConnection()
            => new SqlConnection(_conString);

        // ====== SELECT كل الماركات ======
        public async Task<List<Marque>> GetMarques()
        {
            const string sql = @"
                SELECT id_marque AS IdMarque,
                       nom_marque AS NomMarque
                FROM dbo.Marque
                ORDER BY nom_marque;
            ";

            using var cn = CreateConnection();
            var data = await cn.QueryAsync<Marque>(sql);
            return data.ToList();
        }

        // ====== INSERT ماركة جديدة وترجع Id ديالها ======
        public async Task<int> CreateMarque(string nom)
        {
            const string sql = @"
                INSERT INTO dbo.Marque(nom_marque)
                VALUES (@nom);
                SELECT CAST(SCOPE_IDENTITY() as int);
            ";

            using var cn = CreateConnection();
            var id = await cn.ExecuteScalarAsync<int>(sql, new { nom });
            return id;
        }
    }
}
