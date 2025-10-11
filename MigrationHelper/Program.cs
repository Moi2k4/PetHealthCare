using Npgsql;

var connectionString = Environment.GetEnvironmentVariable("MARK_MIGRATION_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
{
	Console.Error.WriteLine("Missing MARK_MIGRATION_CONNECTION_STRING environment variable.");
	return 1;
}

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

const string ensureHistoryTableSql = """
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
	"MigrationId" character varying(150) NOT NULL,
	"ProductVersion" character varying(32) NOT NULL,
	CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);
""";
await using (var ensureCmd = new NpgsqlCommand(ensureHistoryTableSql, connection))
{
	await ensureCmd.ExecuteNonQueryAsync();
}

const string insertSql = """
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251011093048_InitialCreate', '8.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;
""";

await using (var insertCmd = new NpgsqlCommand(insertSql, connection))
{
	var rows = await insertCmd.ExecuteNonQueryAsync();
	Console.WriteLine(rows > 0
		? "Migration history entry inserted."
		: "Migration history entry already existed.");
}

return 0;
