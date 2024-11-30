using Microsoft.Data.Sqlite;

namespace Api_Version3.Data
{
    public class UserDatabaseConnection
    {
        public static SqliteConnection UserConnection()
        {
            using var conn = new SqliteConnection("Data Source=Data/Databases/Users.db");
            return conn;
        }
    }
}
