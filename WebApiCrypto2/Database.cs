using Npgsql;
using WebApiCrypto2.Models;
using System.Diagnostics;

namespace WebApiCrypto2
{
    public class Database
    {
        NpgsqlConnection con = new NpgsqlConnection(Constants.Connect);

        public async Task InsertCryptoCurrencyAsync(CryptoCurrency cryptoCurrency)
        {
            try
            {
                Debug.WriteLine($"Connection string: {Constants.Connect}");
                await con.OpenAsync();
                foreach (var kvp in cryptoCurrency.cryptocurrencies)
                {
                    var sql = "INSERT INTO public.\"CryptoPriceLog\"(\"Coin\", \"Price\", \"Time\") " +
                              "VALUES (@Coin, @Price, @Time)";
                    NpgsqlCommand comm = new NpgsqlCommand(sql, con);
                    comm.Parameters.AddWithValue("Coin", kvp.Key);
                    comm.Parameters.AddWithValue("Price", kvp.Value.usd);
                    comm.Parameters.AddWithValue("Time", DateTime.Now);

                    var result = await comm.ExecuteNonQueryAsync();
                    Debug.WriteLine($"Inserted {result} row(s) for {kvp.Key}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inserting data: {ex.Message}");
                throw;
            }
            finally
            {
                await con.CloseAsync();
            }
        }

        public async Task<List<CryptoTerm>> GetCryptoTermsAsync()
        {
            var terms = new List<CryptoTerm>();

            try
            {
                await con.OpenAsync();
                var sql = "SELECT \"Id\", \"Term\", \"Definition\" FROM public.\"CryptoTerms\"";
                var comm = new NpgsqlCommand(sql, con);

                using (var reader = await comm.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        terms.Add(new CryptoTerm
                        {
                            Id = reader.GetInt32(0),
                            Term = reader.GetString(1),
                            Definition = reader.GetString(2)
                        });
                    }
                }
            }
            finally
            {
                await con.CloseAsync();
            }

            return terms;
        }

        public async Task<CryptoTerm> GetCryptoTermByIdAsync(int id)
        {
            CryptoTerm term = null;

            try
            {
                await con.OpenAsync();
                var sql = "SELECT \"Id\", \"Term\", \"Definition\" FROM public.\"CryptoTerms\" WHERE \"Id\" = @Id";
                var comm = new NpgsqlCommand(sql, con);
                comm.Parameters.AddWithValue("Id", id);

                using (var reader = await comm.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        term = new CryptoTerm
                        {
                            Id = reader.GetInt32(0),
                            Term = reader.GetString(1),
                            Definition = reader.GetString(2)
                        };
                    }
                }
            }
            finally
            {
                await con.CloseAsync();
            }

            return term;
        }

        public async Task InsertCryptoTermAsync(CryptoTerm term)
        {
            try
            {
                await con.OpenAsync();
                var sql = "INSERT INTO public.\"CryptoTerms\"(\"Term\", \"Definition\") VALUES (@Term, @Definition)";
                var comm = new NpgsqlCommand(sql, con);
                comm.Parameters.AddWithValue("Term", term.Term);
                comm.Parameters.AddWithValue("Definition", term.Definition);

                await comm.ExecuteNonQueryAsync();
            }
            finally
            {
                await con.CloseAsync();
            }
        }

        public async Task UpdateCryptoTermAsync(CryptoTerm term)
        {
            try
            {
                await con.OpenAsync();
                var sql = "UPDATE public.\"CryptoTerms\" SET \"Term\" = @Term, \"Definition\" = @Definition WHERE \"Id\" = @Id";
                var comm = new NpgsqlCommand(sql, con);
                comm.Parameters.AddWithValue("Id", term.Id);
                comm.Parameters.AddWithValue("Term", term.Term);
                comm.Parameters.AddWithValue("Definition", term.Definition);

                await comm.ExecuteNonQueryAsync();
            }
            finally
            {
                await con.CloseAsync();
            }
        }

        public async Task DeleteCryptoTermAsync(int id)
        {
            try
            {
                await con.OpenAsync();
                var sql = "DELETE FROM public.\"CryptoTerms\" WHERE \"Id\" = @Id";
                var comm = new NpgsqlCommand(sql, con);
                comm.Parameters.AddWithValue("Id", id);

                await comm.ExecuteNonQueryAsync();
            }
            finally
            {
                await con.CloseAsync();
            }
        }

        public async Task<CryptoTerm> GetCryptoTermByNameAsync(string termName)
        {
            CryptoTerm term = null;

            try
            {
                await con.OpenAsync();
                var sql = "SELECT \"Id\", \"Term\", \"Definition\" FROM public.\"CryptoTerms\" WHERE \"Term\" = @Term";
                var comm = new NpgsqlCommand(sql, con);
                comm.Parameters.AddWithValue("Term", termName);

                using (var reader = await comm.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        term = new CryptoTerm
                        {
                            Id = reader.GetInt32(0),
                            Term = reader.GetString(1),
                            Definition = reader.GetString(2)
                        };
                    }
                }
            }
            finally
            {
                await con.CloseAsync();
            }

            return term;
        }
    }
}
