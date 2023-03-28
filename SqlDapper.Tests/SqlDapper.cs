namespace SqlDapper.Tests;

using Moq;
using SqlDapper;

[Table("Status")]
public record DbStatus([property: Key] string Name, int Status);

[Table("StatusEx")]
public record DbStatusEx([property: Key] string Name, int Status, string? Comment);

[Table("Monitoring")]
public record DbMonitoring([property: Key] string Name, [property: Key] int Count, int Status);


public class SqlDapperTests {
    [Test]
    public void GenSQLSelect() {
        var prm = new { Name = "tagada" };
        var repository = new MockRepository(MockBehavior.Strict);
        var conn = repository.Create<IDapperConnection>();
        conn.Setup(dc => dc.Query<DbStatus>("SELECT [Name],[Status] FROM [Status] WHERE [Name]=@Name", prm))
                           .Returns(new[] { new DbStatus(Name: "tagada", Status: 42) });

        var res = conn.Object.Select<DbStatus>(prm).Single();
        Assert.That(res, Is.EqualTo(new DbStatus(Name: "tagada", Status: 42)));

        repository.VerifyAll();
    }

    [Test]
    public void GenSQLSelectMultiKey() {
        var prm = new { Name = "tagada", Count = 2, Status = 42 };
        var dbMonitoring = new DbMonitoring(Name: "tagada", Count: 42, Status: 42);
        var repository = new MockRepository(MockBehavior.Strict);
        var conn = repository.Create<IDapperConnection>();
        conn.Setup(dc => dc.Query<DbMonitoring>("SELECT [Name],[Count],[Status] FROM [Monitoring] WHERE [Name]=@Name AND [Count]=@Count AND [Status]=@Status", prm))
                           .Returns(new[] { dbMonitoring });

        var res = conn.Object.Select<DbMonitoring>(prm).Single();
        Assert.That(res, Is.EqualTo(dbMonitoring));

        repository.VerifyAll();
    }


    [Test]
    public void GenSQLSelectAll() {
        var repository = new MockRepository(MockBehavior.Strict);
        var conn = repository.Create<IDapperConnection>();
        conn.Setup(dc => dc.Query<DbStatus>("SELECT [Name],[Status] FROM [Status] WHERE 1=1", It.IsAny<object>()))
                           .Returns(new[] { new DbStatus(Name: "tagada", Status: 42) });

        var res = conn.Object.SelectAll<DbStatus>().Single();
        Assert.That(res, Is.EqualTo(new DbStatus(Name: "tagada", Status: 42)));

        repository.VerifyAll();
    }

    [Test]
    public void GenSQLInsert() {
        var prm = new { Name = "tagada", Status = 42 };
        var repository = new MockRepository(MockBehavior.Strict);
        var conn = repository.Create<IDapperConnection>();

        conn.Setup(dc => dc.Execute("INSERT INTO [Status] ([Name],[Status]) VALUES (@Name,@Status)", prm)).Returns(1);

        conn.Object.Insert<DbStatus>(prm);

        Mock.VerifyAll();
    }

    [Test]
    public void GenSQLUpdate() {
        var prm = new { Name = "tagada", Status = 42 };
        var repository = new MockRepository(MockBehavior.Strict);
        var conn = repository.Create<IDapperConnection>();

        conn.Setup(dc => dc.Execute("UPDATE [Status] SET [Status]=@Status WHERE [Name]=@Name", prm)).Returns(1);

        conn.Object.Update<DbStatus>(prm);

        Mock.VerifyAll();
    }

    [Test]
    public void GenSQLUpdateError() {
        var prm = new { Name = "tagada", Status = 42 };
        var repository = new MockRepository(MockBehavior.Strict);
        var conn = repository.Create<IDapperConnection>();

        conn.Setup(dc => dc.Execute("UPDATE [Status] SET [Status]=@Status WHERE [Name]=@Name", prm)).Returns(0);

        Assert.Throws<Exception>(() => conn.Object.Update<DbStatus>(prm));

        Mock.VerifyAll();
    }

    [Test]
    public void GenSQLDelete() {
        var prm = new { Name = "tagada", Status = 42 };
        var repository = new MockRepository(MockBehavior.Strict);
        var conn = repository.Create<IDapperConnection>();

        conn.Setup(dc => dc.Execute("DELETE [Status] WHERE [Name]=@Name AND [Status]=@Status", prm)).Returns(1);

        conn.Object.Delete<DbStatus>(prm);

        Mock.VerifyAll();
    }

    [Test]
    public void GenSQLUseTransaction() {
        var prm = new { Name = "tagada", Status = 42 };
        var repository = new MockRepository(MockBehavior.Strict);
        var seq = new MockSequence();
        var tx = repository.Create<IDapperTransactionScope>();
        var conn = repository.Create<IDapperConnection>();
        conn.InSequence(seq).Setup(dc => dc.TransactionScope()).Returns(tx.Object);
        conn.InSequence(seq).Setup(dc => dc.Execute("INSERT INTO [Status] ([Name],[Status]) VALUES (@Name,@Status)", prm)).Returns(1);
        tx.InSequence(seq).Setup(t => t.Complete());
        tx.InSequence(seq).Setup(t => t.Dispose());

        runInTx();

        repository.VerifyAll();

        void runInTx() {
            using var tx = conn.Object.TransactionScope();
            conn.Object.Insert<DbStatus>(prm);
            tx.Complete();
        }
    }
}
