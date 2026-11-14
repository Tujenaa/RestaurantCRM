using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace RestaurantCRM.API.Helpers
{
    public static class DbErrorHelper
    {
        /// <summary>
        /// Lỗi do vi phạm khóa chính / ràng buộc UNIQUE của SQL Server
        /// (2627 = PRIMARY KEY / UNIQUE constraint, 2601 = unique index).
        /// </summary>
        public static bool IsUniqueViolation(DbUpdateException ex)
            => ex.InnerException is SqlException sql && (sql.Number == 2627 || sql.Number == 2601);
    }
}
