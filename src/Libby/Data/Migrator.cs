using DbUp;

namespace Libby.Data;

public static class Migrator
{
    public static void Migrate(string connectionString)
    {
        var upgradeEngine = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithTransaction()
            .WithScriptsEmbeddedInAssembly(typeof(Migrator).Assembly)
            .WithVariablesDisabled()
            .LogToAutodetectedLog()
            .Build();

        var result = upgradeEngine.PerformUpgrade();

        if (!result.Successful)
        {
            throw new Exception("Failed to apply migrations", result.Error);
        }
    }
}
