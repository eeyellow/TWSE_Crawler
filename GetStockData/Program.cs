using System;
using Npgsql;
using System.Data;
using System.Configuration;
using System.Net;
using System.IO;
using CsQuery;
using System.Globalization;

namespace GetStockData
{
    class Program
    {
        static void Main(string[] args)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["TWSE_ConnStr"].ToString()))
            {
                using (NpgsqlCommand comm = new NpgsqlCommand())
                {
                    comm.Connection = conn;
                    conn.Open();
                    DataTable dt = new DataTable();

                    #region 取出追蹤中的證券代碼
                    comm.CommandText = @"
                        SELECT 
                            code 
                        FROM 
                            stock
                        WHERE 
                            track = true
                    ";
                    using (NpgsqlDataAdapter sda = new NpgsqlDataAdapter(comm))
                    {
                        sda.Fill(dt);
                    }
                    #endregion


                    #region 對每個追蹤中的證券，用爬蟲把資料抓回來
                    foreach (DataRow dr in dt.Rows)
                    {
                        //組出證券資訊的網址
                        string stock_code = dr["code"].ToString();
                        //往前抓兩年的資料
                        for (int i = 0; i < 24; i++)
                        {
                            string period = DateTime.Now.AddMonths(-1 * i).ToString("yyyyMM");                            
                            using (var client = new WebClient())
                            {
                                //先發出request，讓伺服器生成php頁面
                                var values = new System.Collections.Specialized.NameValueCollection();
                                values["STK_NO"] = stock_code;
                                values["mmon"] = period.Substring(4, 2).Trim('0');
                                values["myear"] = period.Substring(0, 4);
                                var response = client.UploadValues(ConfigurationManager.AppSettings["TWSE_Gen"], values);
                                var responseString = System.Text.Encoding.Default.GetString(response);
                                var start = responseString.IndexOf("(\"");
                                var end = responseString.IndexOf("\")");
                                //再去抓生成後的php頁面
                                var crawler_url = ConfigurationManager.AppSettings["TWSE_Stock"] + responseString.Substring(start + 2, end - start - 2);

                                WebRequest myRequest = WebRequest.Create(crawler_url);
                                myRequest.Method = "GET";
                                WebResponse myResponse;

                                myResponse = myRequest.GetResponse();

                                using (StreamReader sr = new StreamReader(myResponse.GetResponseStream()))
                                {
                                    string result = sr.ReadToEnd();
                                    CQ dom = CQ.Create(result);
                                    comm.CommandText = @"
                                        INSERT INTO 
                                            stock_day(time, code, tv, tov, op, hp, lp, cp, spread, nt)    
                                        VALUES
                                            (@time, @code, @tv, @tov, @op, @hp, @lp, @cp, @spread, @nt)
                                        ON CONFLICT (time, code)
                                        DO 
                                            NOTHING                      
                                    ";
                                    for (var j = 2; j < dom[".board_trad tr"].Length; j++)
                                    {
                                        comm.Parameters.Clear();
                                        CultureInfo tc = new CultureInfo("zh-TW");
                                        tc.DateTimeFormat.Calendar = new TaiwanCalendar();

                                        comm.Parameters.AddWithValue("@time", DateTime.ParseExact(dom[".board_trad tr:eq(" + j + ") td:eq(0) div"].Html(), "yyyy/MM/dd", tc));
                                        comm.Parameters.AddWithValue("@code", stock_code);
                                        comm.Parameters.AddWithValue("@tv", Int32.Parse(dom[".board_trad tr:eq(" + j + ") td:eq(1)"].Html().Replace(",", "")));
                                        comm.Parameters.AddWithValue("@tov", Int64.Parse(dom[".board_trad tr:eq(" + j + ") td:eq(2)"].Html().Replace(",", "")));
                                        comm.Parameters.AddWithValue("@op", float.Parse(dom[".board_trad tr:eq(" + j + ") td:eq(3)"].Html()));
                                        comm.Parameters.AddWithValue("@hp", float.Parse(dom[".board_trad tr:eq(" + j + ") td:eq(4)"].Html()));
                                        comm.Parameters.AddWithValue("@lp", float.Parse(dom[".board_trad tr:eq(" + j + ") td:eq(5)"].Html()));
                                        comm.Parameters.AddWithValue("@cp", float.Parse(dom[".board_trad tr:eq(" + j + ") td:eq(6)"].Html()));
                                        comm.Parameters.AddWithValue("@spread", float.Parse(dom[".board_trad tr:eq(" + j + ") td:eq(7)"].Html().Replace("X", "")));
                                        comm.Parameters.AddWithValue("@nt", Int32.Parse(dom[".board_trad tr:eq(" + j + ") td:eq(8)"].Html().Replace(",", "")));
                                        comm.ExecuteNonQuery();
                                    }
                                }
                                myResponse.Close();
                            }                            
                        }      
                    }
                    #endregion
                }
            }
        }
    }
}
