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
    }
}
