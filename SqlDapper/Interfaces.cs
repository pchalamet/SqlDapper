namespace SqlDapper;

public interface IDapperTransactionScope : IDisposable {
    void Complete();
}

public interface IDapperConnection : IDisposable {
    T ExecuteScalar<T>(string sql, object prms);
    IEnumerable<T> Query<T>(string sql, object prms);
    int Execute(string sql, object prms);
    IDapperTransactionScope TransactionScope();
}
