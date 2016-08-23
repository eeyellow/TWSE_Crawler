using System.Collections.Generic;
using System.Web.Http;
using Npgsql;
using System.Configuration;
using System.Data;

namespace StockWebAPI.Controllers
{
    public class StockController : ApiController
    {
        // GET: api/Stock/5
        public DataTable Get(string code)
        {
            DataTable dt = new DataTable();            
            using (NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["TWSE_ConnStr"].ToString()))
            {
                using (NpgsqlCommand comm = new NpgsqlCommand())
                {
                    comm.Connection = conn;
                    conn.Open();
                    comm.CommandText = @"
                        SELECT 
                            to_char(time, 'YYYY-MM-DD') as time,
                            tv,
                            tov,
                            op,
                            hp,
                            lp,
                            cp,
                            spread,
                            nt
                        FROM 
                            stock_day
                        WHERE
                            code = @code
                    ";
                    comm.Parameters.AddWithValue("@code", code);
                    
                    using (NpgsqlDataAdapter sda = new NpgsqlDataAdapter(comm))
                    {
                        sda.Fill(dt);
                    }
                }
            }
            return dt;
        }

        /*
        // POST: api/Stock
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Stock/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Stock/5
        public void Delete(int id)
        {
        }
        */
    }
}
