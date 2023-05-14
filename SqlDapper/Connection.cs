using System.Data;
using System.Transactions;
using Dapper;

namespace SqlDapper;

public class DapperConnection<DBConnection> : IDapperConnection where DBConnection : IDbConnection, new() {
    private readonly Lazy<TransactionScope> tx = new(() => new TransactionScope());
    private readonly Lazy<IDbConnection> connection;

    public DapperConnection(string connectionString) {
        connection = new(() => new DBConnection { ConnectionString = connectionString });
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
}
