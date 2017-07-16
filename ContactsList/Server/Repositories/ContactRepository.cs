using ContactsList.Server.Entities;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsList.Server.Repositories
{
    public class ContactRepository
    {
        private readonly string _tableName;
        private readonly string _connectionString;

        public ContactRepository(IConfigurationRoot configuration)
        {
            _connectionString = configuration["Data:SqlPostegresConnectionString"];
            _tableName = $"{nameof(Contact)}s";
        }

        internal IDbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(_connectionString);
            }
        }

        public async Task DeleteAllUserContact(int userId)
        {
            using (var connection = Connection)
            using (var tran = connection.BeginTransaction())
            {
                await connection.ExecuteAsync(
                    $@"delete from public.""{_tableName}"" where ""UserId""=@UserId",
                        new { UserId = userId });
                tran.Commit();
            }
        }

        public async Task<IEnumerable<Contact>> GetContactsById(int id)
        {
            using (var connection = Connection)
            {
                var result = await connection.QueryAsync<Contact>(
                    $@"select * from public.""{_tableName}"" where ""UserId""=@UserId",
                        new { UserId = id });
                return result;
            }
        }

        public async Task InsertMany(IEnumerable<Contact> items)
        {
            var query = $@"INSERT INTO public.""{_tableName}"" (""UserId"", ""Name"",
                             ""Phone"", ""Index"", ""Region"", ""City"", ""Address"") 
                        VALUES (@UserId, @Name, @Phone, @Index, @Region,
                                @City, @Address)";

            using (var connection = Connection)
            //using (var tran = connection.BeginTransaction())
            {
                await connection.ExecuteAsync("BEGIN;");
                foreach (var item in items)
                    await connection.ExecuteAsync(query, item);
                await connection.ExecuteAsync("COMMIT;");
                //tran.Commit();
            }
        }

        public async Task Update(IEnumerable<Contact> items)
        {
            using (var connection = Connection)
            //using (var tran = connection.BeginTransaction())
            {
                await connection.ExecuteAsync("BEGIN;");
                foreach (var item in items)
                {

                    await connection.ExecuteAsync(
                        $@"update public.""{_tableName}"" 
                        set ""Name""=@Name, 
                            ""Phone""=@Phone, 
                            ""Index""=@Index,
                            ""Region""=@Region,
                            ""City""=@City,
                            ""Address""=@Address                            
                        where ""Id""=@Id",
                        item);
                }
                await connection.ExecuteAsync("COMMIT;");

                //tran.Commit();
                //await connection.UpdateAsync(item);
            }
        }


    }
}
