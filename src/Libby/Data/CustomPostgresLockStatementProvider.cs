using MassTransit.EntityFrameworkCoreIntegration;

namespace Libby.Data;

public sealed class CustomPostgresLockStatementProvider : SqlLockStatementProvider
{
    public CustomPostgresLockStatementProvider(bool enableSchemaCaching = true)
        : base(new CustomPostgresLockStatementFormatter(), enableSchemaCaching)
    {
    }

    public CustomPostgresLockStatementProvider(string schemaName, bool enableSchemaCaching = true)
        : base(schemaName, new CustomPostgresLockStatementFormatter(), enableSchemaCaching)
    {
    }
}
