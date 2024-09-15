using System;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using MySql.Data.MySqlClient;
using System.Data;
using DecryptPassword;
using System.Threading;

namespace RefreshYGW
{
    internal class Program
    {
        private static DBConfigData[] dbconfig = new DBConfigData[3];
        private static OracleConnection connOracle = null;
        private static MySqlConnection connMySQL = null;
        private static int day = 0;
        private static bool isUpdate = false;
        private static bool isDisp = false;

        static void Main(string[] args)
        {
            // 定義ファイル読み取り
            try
            {
                dbconfig = FileAccess.ReserializeDBConfigFile();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                if (AssemblyState.IsDebug) Console.ReadKey();
                Environment.Exit(1);
            }

            // 2*データベースオープン
            DBOpen();

            // メイン処理
            D0340();

            // データベースクローズ
            if (connOracle != null) connOracle.Close();
            if (connMySQL != null) connMySQL.Close();

            // 終了処理
            if (AssemblyState.IsDebug)
            {
                Console.WriteLine("なにかキーを押してください");
                Console.ReadKey();
            }
            Environment.Exit(0);
        }

        // Oracle 接続文字列
        private static string getOracleConnectionString()
        {
            var dpc = new DecryptPasswordClass();
            dpc.DecryptPassword(dbconfig[0].EncPasswd, out string decPasswd);
            var host = dbconfig[0].Host;
            var userid = dbconfig[0].User;
            var sid = dbconfig[0].ServiceName;
            var password = decPasswd;
            var datasource = $"(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT=1521))(CONNECT_DATA=(SERVICE_NAME={sid})))";
            if (AssemblyState.IsDebug)
                Console.WriteLine($"Oracle[HOST:{host}/UserID:{userid}]");
            return $"User Id={userid};Password={password};Data Source={datasource}";
        }

        // MySQL 接続文字列作成
        private static string getMySQLConnectionString()
        {
            var dpc = new DecryptPasswordClass();
            dpc.DecryptPassword(dbconfig[2].EncPasswd, out string decPasswd);
            var host = dbconfig[2].Host;
            var userid = dbconfig[2].User;
            var password = decPasswd;
            var database = dbconfig[2].Schema;
            var port = dbconfig[2].Port;
            if (AssemblyState.IsDebug)
                Console.WriteLine($"MySQL [HOST:{host}/UserID:{userid}]");
            return $"Server={host};User ID={userid};Password={password};Database={database};Port={port};";
        }

        private static void DBOpen()
        {
            var ocn = getOracleConnectionString();
            connOracle = new OracleConnection(ocn);
            try
            {
                connOracle.Open();
                //Console.WriteLine("Oracleデータベース接続確認");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Oracleデータベースへ接続できませんでした．\r\n" + ex.Message.ToString());
                Environment.Exit(1);
            }
            var mcn = getMySQLConnectionString();
            connMySQL = new MySqlConnection(mcn);
            try
            {
                connMySQL.Open();
                //Console.WriteLine("MySQLデータベース接続確認");
            }
            catch (Exception ex)
            {
                Console.WriteLine("MySQLデータベースへ接続できませんでした．\r\n" + ex.Message.ToString());
                Environment.Exit(1);
            }
        }

        // 出荷指示書のYデータ取得
        private static void D0340()
        {
            // １．ＹＧＷ対象ファイルの最小の登録日付と件数を取得
            var ss = $"select COUNT(*), TO_CHAR(MIN(INSTDT),'YYYY-MM-DD') AS OPDATE, "
                   + "TO_CHAR(MIN(INSTDT),'HH24MI') AS OPTIME, MIN(INSTDT) AS INSTDT from D0340 ";
//                   + "where INSTDT>TRUNC(SYSDATE)";
            var oc = new OracleCommand(ss);
            oc.Connection = connOracle;
            OracleDataReader or = oc.ExecuteReader();
            or.Read();
            if (or.GetInt32(0) == 0)
            {
                Console.WriteLine("当日のＹＧＷデータは存在しませんでした．");
                Environment.Exit(1);
            }
            var opdate = or.GetString(1);
            var optime = or.GetString(2);
            Console.WriteLine($"対象日時：[{opdate}-{optime}]");

            // ２．出荷指示書に載せるY情報を取得 (Oracle)
            var dtOra = new DataTable();
            var sqlOra = @"
select
    TO_CHAR(MIN(d34.INSTDT),'YYYY-MM-DD') as OPDATE, TO_CHAR(MIN(d34.INSTDT),'HH24MI') AS OPTIME
  , d311.HMCD as HMCD
  , m512.ODCD as PODCD, m302.ODRNM as PODRNM, sum(nvl(d512.JIQTY, 0)) as PJIQTY
  , m511.ODCD as ODCD, m301.ODRNM as ODRNM
  , sum(d33.PLNQTY) as PLNQTY, sum(d33.ZAIQTY) as ZAIQTY, sum(nvl(d511.JIQTY, 0)) as JIQTY
from
    D0340 d34
        left join M0510 m512 on m512.HMCD = d34.HMCD
            and m512.VALDTF =
                (select max(tmp3.VALDTF) from M0510 tmp3 where tmp3.HMCD = d34.HMCD)
            and m512.KTSEQ =
                (select max(tmp4.KTSEQ)  from M0510 tmp4 where tmp4.HMCD = d34.HMCD and tmp4.JIKBN = '1' 
                    and tmp4.VALDTF = (select max(tmp6.VALDTF) from M0510 tmp6 where tmp6.HMCD = d34.HMCD)
                    and tmp4.KTSEQ  < (select max(tmp5.KTSEQ)  from M0510 tmp5 where tmp5.HMCD = d34.HMCD and tmp5.JIKBN = '1')
                )
        left join M0300 m302 on m302.ODCD = m512.ODCD
        left join D0310 d312 on d312.KJUNO = d34.KJUNO and d312.KTCD = m512.KTCD and d312.ODCD = m512.ODCD
        left join D0510 d512 on d512.HMCD = d312.HMCD and d512.KTCD = d312.KTCD and d512.ODCD = d312.ODCD and d512.JIDT > d312.INSTDT
  , M0510 m511
  , D0310 d311
        left join D0510 d511 on d511.HMCD = d311.HMCD and d511.KTCD = d311.KTCD and d511.ODCD = d311.ODCD and d511.JIDT > d311.INSTDT
  , D0330 d33
  , M0300 m301
where
    d34.HMCD = m511.HMCD
    and m511.VALDTF = (select max(tmp1.valdtf) from M0510 tmp1 where tmp1.HMCD = m511.HMCD)
    and m511.KTSEQ = (select max(tmp2.KTSEQ)  from M0510 tmp2 where tmp2.HMCD = m511.HMCD and tmp2.JIKBN = '1')

    and d34.KJUNO = d311.KJUNO
    and m511.KTCD = d311.KTCD
    and m511.ODCD = d311.ODCD

    and d311.KJUNO = d33.KJUNO
    and d311.SEQ = d33.SEQ

    and d311.ODCD = m301.ODCD

    and d33.PLNQTY > 0
    and d33.YGW1 = '1'
group by d311.HMCD, m511.ODCD, m301.ODRNM, m512.ODCD, m302.ODRNM
";
//    and d33.INSTDT > trunc(SYSDATE) // 本来は入れたい！！！テスト中は外す

            var oracleCommand = new OracleCommand(sqlOra);
            oracleCommand.Connection = connOracle;
            OracleDataReader oracleReader = oracleCommand.ExecuteReader();
            dtOra.Load(oracleReader);

            // トランザクション開始
            var transaction = connMySQL.BeginTransaction();

            // DataAdapterの生成
            var sqlMySQL = $"select * from KD8010 where OPDATE='{opdate}' and OPTIME='{optime}'";
            var myDa = new MySqlDataAdapter(sqlMySQL, connMySQL);
            // CommandBuiderの生成
            var myCb = new MySqlCommandBuilder(myDa);
            // DataTableに格納
            var dtMySQL = new DataTable();
            myDa.Fill(dtMySQL);

            // OracleRowsを一件ずつループしデータテーブルに対して追加更新を行う
            var countInsert = 0;
            var countUpdate = 0;
            var countDelete = 0;
            foreach (DataRow row in dtOra.Rows)
            {
                var hmcd = row["HMCD"].ToString();

                if (dtMySQL.Select($"HMCD='{hmcd}'").Count() == 0)
                {
                    DataRow r = dtMySQL.NewRow();
                    
                    r["OPDATE"] = opdate;
                    r["OPTIME"] = optime;
                    r["HMCD"] = row["HMCD"];
                    r["PODCD"] = row["PODCD"];
                    r["PODRNM"] = row["PODRNM"];
                    r["PJIQTY"] = row["PJIQTY"];
                    r["ODCD"] = row["ODCD"];
                    r["ODRNM"] = row["ODRNM"];
                    r["PLNQTY"] = row["PLNQTY"];
                    r["ZAIQTY"] = row["ZAIQTY"];
                    r["JIQTY"] = row["JIQTY"];
                    dtMySQL.Rows.Add(r);
                    //dtMySQL.ImportRow(r);
                    //dtMySQL.Rows[dtMySQL.Rows.Count - 1].SetAdded();
                    countInsert++;
                }
                else
                {
                    DataRow dr = dtMySQL.Select($"OPDATE='{opdate}' and OPTIME='{optime}' and HMCD='{hmcd}'")[0];
                    var idx = dtMySQL.Rows.IndexOf(dr);
                    if (
                        int.Parse(row["PJIQTY"].ToString()) != int.Parse(dtMySQL.Rows[idx]["PJIQTY"].ToString())||
                        int.Parse(row["JIQTY"].ToString()) != int.Parse(dtMySQL.Rows[idx]["JIQTY"].ToString())
                        )
                    {
                        dtMySQL.Rows[idx]["PJIQTY"] = row["PJIQTY"];
                        dtMySQL.Rows[idx]["JIQTY"] = row["JIQTY"];
                        countUpdate++;
                    }
                }
            }
            // 逆にMySQLRowを一件ずつループしデータテーブルより削除
            foreach (DataRow row in dtMySQL.Rows)
            {
                var hmcd = row["HMCD"].ToString();

                if (dtOra.Select($"OPDATE='{opdate}' and OPTIME='{optime}' and HMCD='{hmcd}'").Count() == 0)
                {
                    DataRow dr = dtMySQL.Select($"OPDATE='{opdate}' and OPTIME='{optime}' and HMCD='{hmcd}'")[0];
                    var idx = dtMySQL.Rows.IndexOf(dr);
                    dtMySQL.Rows[idx].Delete();
                    countDelete++;
                }
            }

            // データテーブルに対して行った追加更新削除をデータベースに反映
            myDa.Update(dtMySQL);

            // 結果
            var tab = "      ";
            if (countInsert + countUpdate + countDelete > 0)
            {
                if (isDisp) Console.WriteLine();
                Console.WriteLine(tab + "検査対象件数：" + String.Format("{0:#,0}", dtOra.Rows.Count) + " 件");
                Console.WriteLine(tab + "新規登録件数：" + String.Format("{0:#,0}", countInsert) + " 件");
                Console.WriteLine(tab + "　　更新件数：" + String.Format("{0:#,0}", countUpdate) + " 件");
                Console.WriteLine(tab + "　　削除件数：" + String.Format("{0:#,0}", countDelete) + " 件");
                Console.WriteLine();

                // トランザクション終了
                transaction.Commit();
            }
            else
            {
                Console.WriteLine(tab + "更新はありませんでした．"); //.PadLeft(18));
                Console.WriteLine();

                // トランザクション終了
                transaction.Rollback();
            }
        }
    }
}
