using System.Data;
using Microsoft.Data.SqlClient;

namespace SqlDapper;

public class SqlServerConnection : DapperConnection {
    public SqlServerConnection(string connectionString) : base(connectionString) {
    }

    protected override IDbConnection CreateConnection(string connectionString) {
        return new SqlConnection(connectionString);
    }
}
