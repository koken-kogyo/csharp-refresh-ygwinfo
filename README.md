# [KMD012SC] ＹＧＷ累積ファイル作成  
- RefreshYGW  

## 概要  
- EMのＹＧＷ対象ファイルとMySQL側のＹＧＷ累積ファイルを比較し登録更新を行う  

## 開発環境  
- C# .NET Framework v4.8  コンソールアプリケーション  

## 参照設定  
- DecryptPassword.dll  
- Oracle.ManagedDataAccess.dll  
- MySql.Data.dll  
  (\packages\MySql.Data.8.0.32.1\lib\net452)  

## メンバー  
- y.watanabe  

## プロジェクト構成  
~~~  
./  
│  .gitignore                                  # ソース管理除外対象  
│  RefreshYGW.sln                              # Visual Studio Solution ファイル  
│  README.md                                   # このファイル  
│  
├─ ActualProductCollation  
│  │  App.config                              # アプリケーション設定ファイル  
│  │  Common.cs                               # 共通設定ファイル  
│  │  FileAccess.cs                           # ファイルアクセス  
│  └  Program.cs                              # メイン関数  
│      
├─ packages  
│  │  DecryptPassword.dll                     #   
│  │  Oracle.ManagedDataAccess.dll            #   
│  └  MySql.Data.8.0.32.1  
│      
├─ settingfiles  
│      ConfigDB.xml                            # データベース定義ファイル  
│      
└─ specification  
        [KXXxxxXX] xxx 機能仕様書_Ver.1.0.0.0.xlsx  
        
~~~  

## 変更履歴  
- 2024.09.14  新規作成  y.watanabe  
