using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace ConsoleApp5
{
    class Program
    {
        static void Main(string[] args)
        {
            var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = ":memory:" };

            //ケースA：LINQ to SQL、InsertOnSubmitで1文ずつ
            using (var cn = new SQLiteConnection(sqlConnectionSb.ToString()))
            {
                cn.Open();

                using (var cmd = new SQLiteCommand(cn))
                using (var context = new DataContext(cn))
                {
                    //テーブル作成
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS denco(" +
                        "no INTEGER NOT NULL PRIMARY KEY," +
                        "name TEXT NOT NULL," +
                        "type TEXT NOT NULL," +
                        "attribute TEXT NOT NULL," +
                        "maxap INTEGER NOT NULL," +
                        "maxhp INTEGER NOT NULL," +
                        "skill TEXT)";
                    cmd.ExecuteNonQuery();

                    var table = context.GetTable<Denco>();

                    var sw = new Stopwatch();
                    sw.Start();

                    cmd.Transaction = cn.BeginTransaction();//ここのコメントアウト有無

                    for (var i = 0; i < 100000; i++)
                    {
                        cmd.InsertDenco(2 + i * 100, "為栗メロ", "アタッカー", "eco", 310, 300, "きゃのんぱんち");
                        cmd.InsertDenco(3 + i * 100, "新阪ルナ", "ディフェンダー", "cool", 220, 360, "ナイトライダー");
                        cmd.InsertDenco(4 + i * 100, "恋浜みろく", "トリックスター", "heat", 300, 360, "ダブルアクセス");
                        cmd.InsertDenco(8 + i * 100, "天下さや", "アタッカー", "cool", 400, 240);
                        cmd.InsertDenco(13 + i * 100, "新居浜いずな", "ディフェンダー", "heat", 290, 336, "重連壁");
                        cmd.InsertDenco(31 + i * 100, "新居浜ありす", "ディフェンダー", "heat", 270, 350, "ハッピーホリデイ");
                    }

                    cmd.Transaction.Commit();//ここのコメントアウト有無

                    sw.Stop();
                    Console.WriteLine(sw.Elapsed);
                    Console.ReadLine();

                    sw.Restart();

                    for (var i = 0; i < 1000; i++)
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM denco";
                        var c1 = cmd.ExecuteScalar();
                        cmd.CommandText = "SELECT COUNT(*) FROM denco WHERE attribute = 'eco'";
                        var c2 = cmd.ExecuteScalar();
                        //Console.Write($"a: {c1}, w: {c2}/");
                    }
                    Console.WriteLine();
                    Console.WriteLine(sw.Elapsed);

                    sw.Restart();

                    for (var i = 0; i < 1000; i++)
                    {
                        var c1 = table.Count();
                        var c2 = table.Where(denco => denco.Attribute == "eco").Count();
                        //Console.Write($"a: {c1}, w: {c2}/");
                    }
                    Console.WriteLine();
                    Console.WriteLine(sw.Elapsed);

                }
            }

            Console.ReadLine();
        }
    }

    public static class SQLiteExtension
    {
        public static int InsertDenco(this SQLiteCommand command, int no, string name, string type, string attr,
            int maxap, int maxhp, string skill = null)
        {
            var skillstr = skill == null ? "null" : $"'{skill}'";
            command.CommandText = "INSERT INTO denco(no, name, type, attribute, maxap, maxhp, skill) VALUES(" +
                $"{no}, '{name}', '{type}', '{attr}', {maxap}, {maxhp}, {skillstr})";
            return command.ExecuteNonQuery();
        }

        public static string DumpQuery(this SQLiteDataReader reader)
        {
            var i = 0;
            var sb = new StringBuilder();
            while (reader.Read())
            {
                if (i == 0)
                {
                    sb.AppendLine(string.Join("\t", reader.GetValues().AllKeys));
                    sb.AppendLine(new string('=', 8 * reader.FieldCount));
                }
                sb.AppendLine(string.Join("\t", Enumerable.Range(0, reader.FieldCount).Select(x => reader.GetValue(x))));
                i++;
            }

            return sb.ToString();
        }
    }

    //テーブル構造定義クラス
    [Table(Name = "denco")]
    public class Denco
    {
        [Column(Name = "no", CanBeNull = false, DbType = "INT", IsPrimaryKey = true)]
        public int No { get; set; }
        [Column(Name = "name", CanBeNull = false, DbType = "NVARCHAR")]
        public string Name { get; set; }
        [Column(Name = "type", CanBeNull = false, DbType = "NVARCHAR")]
        public string Type { get; set; }
        [Column(Name = "attribute", CanBeNull = false, DbType = "NVARCHAR")]
        public string Attribute { get; set; }
        [Column(Name = "maxap", CanBeNull = false, DbType = "INT")]
        public int MaxAp { get; set; }
        [Column(Name = "maxhp", CanBeNull = false, DbType = "INT")]
        public int MaxHp { get; set; }
        [Column(Name = "skill", CanBeNull = true, DbType = "NVARCHAR")]
        public string Skill { get; set; }

        public Denco() { }
        public Denco(int no, string name, string type, string attribute, int maxap, int maxhp, string skill = null)
        {
            No = no;
            Name = name; Type = type; Attribute = attribute;
            MaxAp = maxap; MaxHp = maxhp;
            Skill = skill;
        }

        public string Dump()
        {
            return $"{No}\t{Name}\t{Type}\t{Attribute}\t{MaxAp}\t{MaxHp}\t{Skill}";
        }
    }
}