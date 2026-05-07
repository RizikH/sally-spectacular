using Microsoft.Data.Sqlite;
using RepairTracker.Logic;
using RepairTracker.Models;

namespace RepairTracker.Database;

public static class DbContext
{
    private static string _dbPath = "";

    public static void Initialize()
    {
        _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "repair_tracker.db");
        using var conn = Connect();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS app_state (
                key   TEXT PRIMARY KEY,
                value TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS seasons (
                id         INTEGER PRIMARY KEY AUTOINCREMENT,
                name       TEXT NOT NULL UNIQUE,
                created_at TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS episodes (
                id                INTEGER PRIMARY KEY AUTOINCREMENT,
                season_id         INTEGER NOT NULL REFERENCES seasons(id) ON DELETE CASCADE,
                episode_number    INTEGER NOT NULL,
                item_description  TEXT NOT NULL DEFAULT '',
                cost              REAL NOT NULL DEFAULT 0,
                parts             REAL NOT NULL DEFAULT 0,
                est_sell_price    REAL,
                actual_sell_price REAL,
                postage           REAL NOT NULL DEFAULT 0,
                created_at        TEXT NOT NULL,
                updated_at        TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS hours_log (
                id           INTEGER PRIMARY KEY AUTOINCREMENT,
                episode_id   INTEGER NOT NULL UNIQUE REFERENCES episodes(id) ON DELETE CASCADE,
                hours_worked REAL NOT NULL DEFAULT 0,
                notes        TEXT
            );
        ";
        cmd.ExecuteNonQuery();
    }

    private static SqliteConnection Connect()
    {
        var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();
        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return conn;
    }

    // ---- App State ----

    public static string? GetAppState(string key)
    {
        using var conn = Connect();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT value FROM app_state WHERE key = $key";
        cmd.Parameters.AddWithValue("$key", key);
        return cmd.ExecuteScalar() as string;
    }

    public static void SetAppState(string key, string value)
    {
        using var conn = Connect();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT OR REPLACE INTO app_state (key, value) VALUES ($key, $value)";
        cmd.Parameters.AddWithValue("$key", key);
        cmd.Parameters.AddWithValue("$value", value);
        cmd.ExecuteNonQuery();
    }

    // ---- Seasons ----

    public static List<Season> GetAllSeasons()
    {
        using var conn = Connect();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT s.id, s.name, s.created_at, COUNT(e.id) AS cnt
            FROM seasons s
            LEFT JOIN episodes e ON e.season_id = s.id
            GROUP BY s.id
            ORDER BY s.id
        ";
        var list = new List<Season>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Season { Id = r.GetInt32(0), Name = r.GetString(1), CreatedAt = r.GetString(2), EpisodeCount = r.GetInt32(3) });
        return list;
    }

    public static Season CreateSeason(string name)
    {
        using var conn = Connect();
        string now = DateTime.UtcNow.ToString("o");
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO seasons (name, created_at) VALUES ($name, $now); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$now", now);
        int id = (int)(long)cmd.ExecuteScalar()!;
        return new Season { Id = id, Name = name, CreatedAt = now };
    }

    // ---- Episodes ----

    public static List<Episode> GetEpisodesForSeason(int seasonId)
    {
        using var conn = Connect();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, season_id, episode_number, item_description, cost, parts,
                   est_sell_price, actual_sell_price, postage, created_at, updated_at
            FROM episodes WHERE season_id = $sid ORDER BY episode_number
        ";
        cmd.Parameters.AddWithValue("$sid", seasonId);
        var list = new List<Episode>();
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(ReadEpisode(r));
        return list;
    }

    public static int GetNextEpisodeNumber(int seasonId)
    {
        using var conn = Connect();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COALESCE(MAX(episode_number), 0) + 1 FROM episodes WHERE season_id = $sid";
        cmd.Parameters.AddWithValue("$sid", seasonId);
        return (int)(long)cmd.ExecuteScalar()!;
    }

    public static Episode CreateEpisode(Episode ep)
    {
        using var conn = Connect();
        string now = DateTime.UtcNow.ToString("o");
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO episodes (season_id, episode_number, item_description, cost, parts,
                est_sell_price, actual_sell_price, postage, created_at, updated_at)
            VALUES ($sid, $num, $desc, $cost, $parts, $est, $act, $post, $now, $now);
            SELECT last_insert_rowid();
        ";
        cmd.Parameters.AddWithValue("$sid", ep.SeasonId);
        cmd.Parameters.AddWithValue("$num", ep.EpisodeNumber);
        cmd.Parameters.AddWithValue("$desc", ep.ItemDescription);
        cmd.Parameters.AddWithValue("$cost", ep.Cost);
        cmd.Parameters.AddWithValue("$parts", ep.Parts);
        cmd.Parameters.AddWithValue("$est", ep.EstSellPrice.HasValue ? (object)ep.EstSellPrice.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("$act", ep.ActualSellPrice.HasValue ? (object)ep.ActualSellPrice.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("$post", ep.Postage);
        cmd.Parameters.AddWithValue("$now", now);
        ep.Id = (int)(long)cmd.ExecuteScalar()!;
        ep.CreatedAt = ep.UpdatedAt = now;
        return ep;
    }

    public static void UpdateEpisode(Episode ep)
    {
        using var conn = Connect();
        string now = DateTime.UtcNow.ToString("o");
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE episodes SET
                item_description  = $desc,
                cost              = $cost,
                parts             = $parts,
                est_sell_price    = $est,
                actual_sell_price = $act,
                postage           = $post,
                updated_at        = $now
            WHERE id = $id
        ";
        cmd.Parameters.AddWithValue("$desc", ep.ItemDescription);
        cmd.Parameters.AddWithValue("$cost", ep.Cost);
        cmd.Parameters.AddWithValue("$parts", ep.Parts);
        cmd.Parameters.AddWithValue("$est", ep.EstSellPrice.HasValue ? (object)ep.EstSellPrice.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("$act", ep.ActualSellPrice.HasValue ? (object)ep.ActualSellPrice.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("$post", ep.Postage);
        cmd.Parameters.AddWithValue("$now", now);
        cmd.Parameters.AddWithValue("$id", ep.Id);
        cmd.ExecuteNonQuery();
        ep.UpdatedAt = now;
    }

    // ---- Hours Log ----

    public static HoursLog? GetHoursLog(int episodeId)
    {
        using var conn = Connect();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, episode_id, hours_worked, notes FROM hours_log WHERE episode_id = $eid";
        cmd.Parameters.AddWithValue("$eid", episodeId);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new HoursLog { Id = r.GetInt32(0), EpisodeId = r.GetInt32(1), HoursWorked = r.GetDouble(2), Notes = r.IsDBNull(3) ? null : r.GetString(3) };
    }

    public static List<HoursLog> GetHoursLogsForSeason(int seasonId)
    {
        using var conn = Connect();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT h.id, h.episode_id, h.hours_worked, h.notes
            FROM hours_log h
            JOIN episodes e ON e.id = h.episode_id
            WHERE e.season_id = $sid
            ORDER BY e.episode_number
        ";
        cmd.Parameters.AddWithValue("$sid", seasonId);
        var list = new List<HoursLog>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new HoursLog { Id = r.GetInt32(0), EpisodeId = r.GetInt32(1), HoursWorked = r.GetDouble(2), Notes = r.IsDBNull(3) ? null : r.GetString(3) });
        return list;
    }

    public static void UpsertHoursLog(HoursLog log)
    {
        using var conn = Connect();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT OR REPLACE INTO hours_log (episode_id, hours_worked, notes)
            VALUES ($eid, $hours, $notes)
        ";
        cmd.Parameters.AddWithValue("$eid", log.EpisodeId);
        cmd.Parameters.AddWithValue("$hours", log.HoursWorked);
        cmd.Parameters.AddWithValue("$notes", (object?)log.Notes ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    // ---- Profit Summary ----

    public static List<(Season season, double totalCost, double totalParts, double totalPostage, double? estProfit, double? netProfit)>
        GetProfitSummary(int? filterSeasonId = null)
    {
        var seasons = GetAllSeasons();
        if (filterSeasonId.HasValue)
            seasons = seasons.Where(s => s.Id == filterSeasonId.Value).ToList();

        var result = new List<(Season, double, double, double, double?, double?)>();
        foreach (var season in seasons)
        {
            var eps = GetEpisodesForSeason(season.Id);
            double cost = eps.Sum(e => e.Cost);
            double parts = eps.Sum(e => e.Parts);
            double postage = eps.Sum(e => e.Postage);

            var withEst = eps.Where(e => e.EstSellPrice.HasValue).ToList();
            double? estP = withEst.Count > 0
                ? withEst.Sum(e => Calculations.EstimatedProfit(e.Cost, e.Parts, e.EstSellPrice!.Value))
                : null;

            var withAct = eps.Where(e => e.ActualSellPrice.HasValue).ToList();
            double? netP = withAct.Count > 0
                ? withAct.Sum(e => Calculations.NetProfit(e.Cost, e.Parts, e.ActualSellPrice!.Value, e.Postage))
                : null;

            result.Add((season, cost, parts, postage, estP, netP));
        }
        return result;
    }

    private static Episode ReadEpisode(SqliteDataReader r) => new Episode
    {
        Id = r.GetInt32(0),
        SeasonId = r.GetInt32(1),
        EpisodeNumber = r.GetInt32(2),
        ItemDescription = r.GetString(3),
        Cost = r.GetDouble(4),
        Parts = r.GetDouble(5),
        EstSellPrice = r.IsDBNull(6) ? null : r.GetDouble(6),
        ActualSellPrice = r.IsDBNull(7) ? null : r.GetDouble(7),
        Postage = r.GetDouble(8),
        CreatedAt = r.GetString(9),
        UpdatedAt = r.GetString(10)
    };
}
