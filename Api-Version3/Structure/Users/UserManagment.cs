using Api_Version3.Data;
using Api_Version3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

namespace Api_Version3.Structure.Users
{
    //
    //
    //
    //Creating a new user
    public class UserManagment
    {
        public static string UserCreate(CreateUser CU)
        {
            const int keySize = 64;
            const int iterations = 350000;
            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

            User user = PasswordHashing(CU.Password, out var salt, keySize, iterations, hashAlgorithm);
            if(user.Name == "Failed")
            {
                return "Password Failed to Encrypt: Please Try Again Soon.";
            }
            else
            {
                user.Username = CU.Username;
                user.Email = CU.Email;
                user.Name = CU.Name;

                try
                {
                    var conn = UserDatabaseConnection.UserConnection();
                    conn.Open();

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = @"INSERT INTO users (name, username, email, hashedPassword, salt) VALUES ($name, $username, $email, $hp, $salt)"; ;

                    cmd.Parameters.AddWithValue("$name", user.Name);
                    cmd.Parameters.AddWithValue("$username", user.Username);
                    cmd.Parameters.AddWithValue("$email", user.Email);
                    cmd.Parameters.AddWithValue("$hp", user.HashedPassword);
                    cmd.Parameters.AddWithValue("$salt", user.Salt);

                    var status = cmd.ExecuteNonQuery();
                    conn.Close();

                    if (status > 0) { return "Success: Account Created."; }
                    else { return "Error Adding Account To Database: Please Try Again Soon."; }
                }
                catch(Exception ex)
                {
                    return "Error Loading Database: Please Try Again Soon.";
                }
            }

        }

        private static User PasswordHashing(string providedPassword, out byte[] salt, int keySize, int iterations, HashAlgorithmName hashAlgorithm)
        {
            User user = new User();

            salt = RandomNumberGenerator.GetBytes(1);
            try
            {
                salt = RandomNumberGenerator.GetBytes(keySize);
                var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(providedPassword), salt, iterations, hashAlgorithm, keySize);
                string finalHash = Convert.ToHexString(hash);

                user.HashedPassword = finalHash;
                user.Salt = salt;

                return user;
            }
            catch { user.Name = "Failed"; return user; }
        }

        //
        //
        //
        //Logging in the user
        public static LoggedInUser UserLogin(LoginUser LU)
        {
            const int keySize = 64;
            const int iterations = 350000;
            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
            LoggedInUser LIU = new LoggedInUser();
            try
            {
                var conn = UserDatabaseConnection.UserConnection();
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT id FROM users WHERE username = $username LIMIT 1";
                cmd.Parameters.AddWithValue("username", LU.Username);
                using var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    var connTwo = UserDatabaseConnection.UserConnection();
                    connTwo.Open();

                    var cmdTwo = connTwo.CreateCommand();
                    cmdTwo.CommandText = @"SELECT hashedPassword, salt, id, username, name FROM users WHERE username = $username LIMIT 1";
                    cmdTwo.Parameters.AddWithValue("username", LU.Username);

                    bool resp = false;
                    using (var readerTwo = cmdTwo.ExecuteReader())
                    {
                        while (readerTwo.Read())
                        {
                            string hashedPassword = readerTwo.GetString(0);
                            byte[] salt = (byte[])readerTwo.GetValue(1);
                            resp = checkPassword(LU.Password, keySize, iterations, hashAlgorithm, salt, hashedPassword);
                            if (resp)
                            {
                                LIU.ID = readerTwo.GetInt32(2);
                                LIU.Username = readerTwo.GetString(3);
                                LIU.Name = readerTwo.GetString(4);
                            }
                        }
                        conn.Close();
                        return LIU;
                    }
                }
                else { conn.Close(); LIU.ID = -1; return LIU; }
            }
            catch { LIU.ID = -2;  return LIU; }
        }

        public static bool checkPassword(string password, int keySize, int iterations, HashAlgorithmName hashAlgorithm, byte[] salt, string hashedPassword)
        {
            try
            {
                var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize);
                return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(hashedPassword));
            }
            catch { return false; }
        }

        //
        //
        //
        //Delete the user
        public static int UserDelete(LoggedInUser LIU)
        {
            try
            {
                var conn = UserDatabaseConnection.UserConnection();
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"DELETE FROM users WHERE id = $id";
                cmd.Parameters.AddWithValue("$id", LIU.ID);
                var status = cmd.ExecuteNonQuery();

                if (status > 0) { return 0; }
                else { return 2; }
            }
            catch { return 1; }
        }

        //
        //
        //
        //Update details of the user
        public static int UserUpdate(UpdateDetails UD)
        {
            const int keySize = 64;
            const int iterations = 350000;
            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

            int counter = UD.UpdateCounter();

            string sqlStart = @"UPDATE users SET";
            string sqlEnd = " WHERE id = $id";

            string hash = "";
            byte[] Salt = [];

            if (UD.NewPassword != "")
            {
                --counter;
                var user = PasswordHashing(UD.NewPassword, out byte[] salt, keySize, iterations, hashAlgorithm);
                hash = user.HashedPassword;
                Salt = user.Salt;
                sqlStart += " password = $password";
                if (counter != 0) { sqlEnd += ","; }
            }
            if (UD.NewEmail != "")
            {
                --counter;
                sqlStart += " email = $email";
                if(counter != 0) {  sqlEnd += ","; }
            }
            if (UD.NewName != "")
            {
                --counter;
                sqlStart += " name = $name";
                if (counter != 0) { sqlEnd += ","; }
            }

            try
            {
                string finalSQLString = sqlStart + sqlEnd;

                var conn = UserDatabaseConnection.UserConnection();
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = finalSQLString;
                cmd.Parameters.AddWithValue("$name", UD.NewName);
                cmd.Parameters.AddWithValue("$email", UD.NewEmail);
                cmd.Parameters.AddWithValue("$password", hash);
                cmd.Parameters.AddWithValue("$salt", Salt);
                cmd.Parameters.AddWithValue("$id", UD.ID);
                var insertSuccess = cmd.ExecuteNonQuery();

                if (insertSuccess > 0) { return 0; }
                else { return 2; }
            }
            catch { return 1;  }
        }
    }
}
