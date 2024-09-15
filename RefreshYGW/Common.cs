using System;

namespace RefreshYGW
{
    class Common
    {
        // プログラムタイトル
        public static readonly string PROGRAM_TITLE = "[KMD004SC] EMマスタミラーリング";
        public static readonly string PROGRAM_NAME = "MirrorOra2MySQL";
        public static readonly string PROGRAM_VERSION = "230911.01";

        // 定義情報
        public static readonly string DB_CONFIG_FILE = "ConfigDB.xml";

        // エラーメッセージ定義
        public static readonly string ERR_NOT_NUMERIC = "数値を入力してください．";

        public static readonly string MSG_DATABESE_CONFIG_NOT_EXSIST = "データベース設定ファイルが存在しません\n設定ファイルを配置しアプリを再起動してください";
        public static readonly string MSG_FILE_CONFIG_NOT_EXSIST = "ファイル設定ファイルが存在しません\n設定ファイルを配置しアプリを再起動してください";
        public static readonly string MSG_DATABESE_CONNECTION_FAILURE = "データベースへの接続に失敗しました";
        public static readonly string MSG_DATABESE_CLOSE_FAILURE = "データベースへの切断に失敗しました";
        public static readonly string MSG_KM8420_REFRESH_FAILURE = "データベースの更新に失敗しました";

        public static readonly string MSG_PROGRAM_ERROR = "プログラムの想定エラーが発生しました";

    }
    /// <summary>
    /// データベース設定データ クラス
    /// </summary>
    public class DBConfigData
    {
        // コンストラクタ初期値
        DBConfigData() {
            User = "";
            EncPasswd = "";
            Protocol = "";
            Host = "";
            ServiceName = "";
            Schema = "";
            CharSet = "";
        }

        // プロパティ
        public string User { get; set; }        // ユーザー ID
        public string EncPasswd { get; set; }   // 暗号化パスワード ([KCM002SF] パスワード暗号化アプリ で暗号化した文字列)
        public string Protocol { get; set; }    // 通信プロトコル
        public string Host { get; set; }        // ホスト名または IPv4 アドレス
        public int Port { get; set; }           // ポート番号
        public string ServiceName { get; set; } // サービス名
        public string Schema { get; set; }      // スキーマ
        public string CharSet { get; set; }     // 文字セット
    }


    internal static class AssemblyState
    {
        public const bool IsDebug =
#if DEBUG
        true;
#else
        false;
#endif
    }

}
