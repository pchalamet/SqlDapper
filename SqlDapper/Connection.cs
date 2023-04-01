using System.Data;
using System.Transactions;
using Dapper;

namespace SqlDapper;

public abstract class DapperConnection : IDapperConnection {
    private readonly Lazy<TransactionScope> tx = new(() => new TransactionScope());
    private readonly Lazy<IDbConnection> connection;

    protected DapperConnection(string connectionString) {
        connection = new(() => CreateConnection(connectionString));
    }

    public void Dispose() {
        if (connection.IsValueCreated) {
            connection.Value.Dispose();
        }

        if (tx.IsValueCreated) {
            tx.Value.Dispose();
        }
    }

    public int Execute(string sql, object prms) {
        return connection.Value.Execute(sql, prms);
    }

    public T ExecuteScalar<T>(string sql, object prms) {
        return connection.Value.ExecuteScalar<T>(sql, prms);
    }

    public IEnumerable<T> Query<T>(string sql, object prms) {
        return connection.Value.Query<T>(sql, prms);
    }

    public IDapperTransactionScope TransactionScope() {
        return new DapperTransactionScope(tx.Value);
    }

    protected abstract IDbConnection CreateConnection(string connectionString);
}
