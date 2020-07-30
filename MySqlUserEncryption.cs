#if MYSQL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SecureUserAccess
{
    public class MySqlUserEncryption
    {
        bool userDeletedDate;
        readonly string SELECT_SQL;
        readonly string INSERT_SQL;
        public MySqlUserEncryption(string idField, string usernameField, string passwordField, string deletedDateField = null)
        {
            if (deletedDateField != null)
            {
                SELECT_SQL = string.Format(@"SELECT {0}, {1}, {2}
                        FROM users
                        WHERE {3} = @username", idField, passwordField, deletedDateField, usernameField);
                userDeletedDate = true;
            }
            else
            {
                SELECT_SQL = string.Format(@"SELECT {0}, {1}
                        FROM users
                        WHERE {2} = @username", idField, passwordField, usernameField);
            }
        }

        public class DBUser
        {
            public int Id { get; set; }
            public string Username { get; set; }
        }

        public DBUser ValidateUser(MySqlConnection connection, string userName, string password)
        {
            DBUser user;
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = SELECT_SQL;
                cmd.Parameters.AddWithValue("userName", userName);

                var reader = cmd.ExecuteReader();
                if (reader == null || !reader.HasRows || !reader.Read())
                    return null;
                int index = 0;
                user = new DBUser
                {
                    Id = reader.GetInt32(index++),
                    Username = userName
                };

                string hashPass = reader.GetString(index++);
                if (!PasswordHash.Validate(password, hashPass))
                    return null;

                if (userDeletedDate)
                {
                    DateTime? deletedDate = reader.GetDateTime(index++);
                    if (deletedDate != null)
                        return null;
                }

            }
            return user;
        }
    }
}

#endif