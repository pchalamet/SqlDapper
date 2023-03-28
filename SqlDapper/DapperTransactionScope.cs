using System.Transactions;

namespace SqlDapper;

public class DapperTransactionScope : IDapperTransactionScope {
    private readonly TransactionScope ts;

    public DapperTransactionScope(TransactionScope ts) {
        this.ts = ts;
    }

    public void Complete() {
        ts.Complete();
    }

    public void Dispose() {
        ts.Dispose();
    }
}
