using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace Kudo_Scraper
{
    class database
    {
        string dns = "Server='192.168.2.3';port=3306;user id='root';password='';database='refill_mlm'";
        public DataTable SQLCari(string query)
        {
            using (MySqlConnection koneksi = new MySqlConnection(dns))
            {
                using (MySqlDataAdapter da = new MySqlDataAdapter(query, koneksi))
                {
                    DataTable Dtb = new DataTable();
                    koneksi.Open();
                    da.Fill(Dtb);
                    return Dtb;
                }
            }
        }

        public string SQLQuery(string query)
        {
            using (MySqlConnection koneksi = new MySqlConnection(dns))
            {
                using (MySqlCommand sqlcmd = new MySqlCommand(query, koneksi))
                {
                    koneksi.Open();
                    sqlcmd.ExecuteNonQuery();
                    return "OK";
                }
            }
        }
    }
}
