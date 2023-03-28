using System.Transactions;
using Microsoft.Data.SqlClient;
using Dapper;

namespace SqlDapper;

public record DapperConnection(string ConnectionString) : IDapperConnection {
    private readonly Lazy<TransactionScope> tx = new(() => new TransactionScope());
    private readonly Lazy<SqlConnection> connection = new(() => new SqlConnection(ConnectionString));

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

