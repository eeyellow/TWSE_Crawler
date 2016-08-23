using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StockWebAPI.Models
{
    public class Stock
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime time { get; set; }
        /// <summary>
        /// 證券代碼
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 成交量（Trading Volume）
        /// </summary>
        public int tv { get; set; }
        /// <summary>
        /// 成交金額（TurnOver in value）
        /// </summary>
        public int tov { get; set; }
        /// <summary>
        /// 開盤價（Opening Price）
        /// </summary>
        public double op { get; set; }
        /// <summary>
        /// 最高價（High Price）
        /// </summary>
        public double hp { get; set; }
        /// <summary>
        /// 最低價（Low Price）
        /// </summary>
        public double lp { get; set; }
        /// <summary>
        /// 收盤價（Closing Price）
        /// </summary>
        public double cp { get; set; }
        /// <summary>
        /// 漲跌價差
        /// </summary>
        public double spread { get; set; }
        /// <summary>
        /// 成交筆數（Number of Transactions）
        /// </summary>
        public int nt { get; set; }
    }
}