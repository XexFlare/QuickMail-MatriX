using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace QuickMail.Services.ContextImages;

/// <summary>
/// One stored choice: which picture a message gets and why. <see cref="Category"/> null means the
/// neutral row was chosen deliberately (no rule matched), which is remembered too.
/// </summary>
public sealed record ContextImageChoice(
    string MessageKey,
    string? Category,
    int Variant,
    string Source,          // "bundled" — the only source in v1; reserved for future providers
    string? Rule,           // the matched phrase, for transparency/debugging
    string Licence,         // licence of the chosen picture
    int SelectorVersion,
    long CreatedUtcTicks);

/// <summary>
/// MailBox MatriX contextual images, step 2: the choices, kept in their own small SQLite file
/// (context-images.db in the profile folder) so the feature stays separate from the mail store and
/// can be deleted without touching mail. Bounded: at most <see cref="MaxRows"/> rows; the oldest are
/// pruned. Never stores message bodies — only the key, the category and the matched phrase.
/// </summary>
public sealed class ContextImageStore
{
    public const int MaxRows = 50_000;
    private readonly string _connectionString;

    public ContextImageStore(string profileDir)
    {
        Directory.CreateDirectory(profileDir);
        DbPath = Path.Combine(profileDir, "context-images.db");
        _connectionString = new SqliteConnectionStringBuilder { DataSource = DbPath, Pooling = false }.ToString();
        using var db = Open();
        Exec(db, """
            CREATE TABLE IF NOT EXISTS ContextImage (
                message_key      TEXT PRIMARY KEY,
                category         TEXT NULL,
                variant          INTEGER NOT NULL,
                source           TEXT NOT NULL,
                rule             TEXT NULL,
                licence          TEXT NOT NULL,
                selector_version INTEGER NOT NULL,
                created_utc      INTEGER NOT NULL
            );
            """);
    }

    public string DbPath { get; }

    public Dictionary<string, ContextImageChoice> LoadAll()
    {
        var all = new Dictionary<string, ContextImageChoice>(StringComparer.Ordinal);
        using var db = Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT message_key, category, variant, source, rule, licence, selector_version, created_utc FROM ContextImage";
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var c = new ContextImageChoice(r.GetString(0), r.IsDBNull(1) ? null : r.GetString(1), r.GetInt32(2),
                r.GetString(3), r.IsDBNull(4) ? null : r.GetString(4), r.GetString(5), r.GetInt32(6), r.GetInt64(7));
            all[c.MessageKey] = c;
        }
        return all;
    }

    public void Save(IReadOnlyCollection<ContextImageChoice> choices)
    {
        if (choices.Count == 0) return;
        using var db = Open();
        using var tx = db.BeginTransaction();
        using var cmd = db.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT OR REPLACE INTO ContextImage (message_key, category, variant, source, rule, licence, selector_version, created_utc)
            VALUES ($k, $c, $v, $s, $r, $l, $sv, $t)
            """;
        var k = cmd.Parameters.Add("$k", SqliteType.Text);
        var c = cmd.Parameters.Add("$c", SqliteType.Text);
        var v = cmd.Parameters.Add("$v", SqliteType.Integer);
        var s = cmd.Parameters.Add("$s", SqliteType.Text);
        var rl = cmd.Parameters.Add("$r", SqliteType.Text);
        var l = cmd.Parameters.Add("$l", SqliteType.Text);
        var sv = cmd.Parameters.Add("$sv", SqliteType.Integer);
        var t = cmd.Parameters.Add("$t", SqliteType.Integer);
        foreach (var choice in choices)
        {
            k.Value = choice.MessageKey;
            c.Value = (object?)choice.Category ?? DBNull.Value;
            v.Value = choice.Variant;
            s.Value = choice.Source;
            rl.Value = (object?)choice.Rule ?? DBNull.Value;
            l.Value = choice.Licence;
            sv.Value = choice.SelectorVersion;
            t.Value = choice.CreatedUtcTicks;
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
        Prune(db);
    }

    /// <summary>Keeps the file bounded: beyond <see cref="MaxRows"/>, the oldest choices go.</summary>
    private static void Prune(SqliteConnection db)
    {
        Exec(db, $"""
            DELETE FROM ContextImage WHERE message_key IN (
                SELECT message_key FROM ContextImage ORDER BY created_utc DESC LIMIT -1 OFFSET {MaxRows});
            """);
    }

    private SqliteConnection Open()
    {
        var db = new SqliteConnection(_connectionString);
        db.Open();
        return db;
    }

    private static void Exec(SqliteConnection db, string sql)
    {
        using var cmd = db.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
