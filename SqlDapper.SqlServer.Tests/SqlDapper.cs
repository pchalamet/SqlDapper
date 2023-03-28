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
    public void GenSQLUpsert() {
        var prm = new { Name = "tagada", Status = 42 };
        var repository = new MockRepository(MockBehavior.Strict);
        var conn = repository.Create<IDapperConnection>();

        conn.Setup(dc => dc.Execute("MERGE INTO [Status] as TARGET USING (VALUES(@Name,@Status)) AS SOURCE ([Name],[Status]) ON SOURCE.[Name]=TARGET.[Name] WHEN MATCHED THEN UPDATE SET [Status]=SOURCE.[Status] WHEN NOT MATCHED THEN INSERT ([Name],[Status]) VALUES (SOURCE.[Name],SOURCE.[Status]);", prm)).Returns(1);

        conn.Object.Upsert<DbStatus>(prm);

        Mock.VerifyAll();
    }

    [Test]
    public void GenSQLUpsertWithPartialTable() {
        var prm = new { Name = "tagada", Status = 42 };
        var repository = new MockRepository(MockBehavior.Strict);
        var conn = repository.Create<IDapperConnection>();

        conn.Setup(dc => dc.Execute("MERGE INTO [StatusEx] as TARGET USING (VALUES(@Name,@Status)) AS SOURCE ([Name],[Status]) ON SOURCE.[Name]=TARGET.[Name] WHEN MATCHED THEN UPDATE SET [Status]=SOURCE.[Status] WHEN NOT MATCHED THEN INSERT ([Name],[Status]) VALUES (SOURCE.[Name],SOURCE.[Status]);", prm)).Returns(1);

        conn.Object.Upsert<DbStatusEx>(prm);

        Mock.VerifyAll();
    }

    [Test]
    public void GenSQLUpsertKO() {
        var prm = new { Name = "tagada", Status = 42 };
        var repository = new MockRepository(MockBehavior.Strict);
        var conn = repository.Create<IDapperConnection>();

        conn.Setup(dc => dc.Execute("MERGE INTO [Status] as TARGET USING (VALUES(@Name,@Status)) AS SOURCE ([Name],[Status]) ON SOURCE.[Name]=TARGET.[Name] WHEN MATCHED THEN UPDATE SET [Status]=SOURCE.[Status] WHEN NOT MATCHED THEN INSERT ([Name],[Status]) VALUES (SOURCE.[Name],SOURCE.[Status]);", prm)).Returns(0);

        Assert.Throws<Exception>(() => conn.Object.Upsert<DbStatus>(prm));

        Mock.VerifyAll();
    }

    [Test]
    public void GenSQLUpsertMultiKey() {
        var prm = new { Name = "tagada", Count = 2, Status = 42 };
        var repository = new MockRepository(MockBehavior.Strict);
        var conn = repository.Create<IDapperConnection>();

        conn.Setup(dc => dc.Execute("MERGE INTO [Monitoring] as TARGET USING (VALUES(@Name,@Count,@Status)) AS SOURCE ([Name],[Count],[Status]) ON SOURCE.[Name]=TARGET.[Name] AND SOURCE.[Count]=TARGET.[Count] WHEN MATCHED THEN UPDATE SET [Status]=SOURCE.[Status] WHEN NOT MATCHED THEN INSERT ([Name],[Count],[Status]) VALUES (SOURCE.[Name],SOURCE.[Count],SOURCE.[Status]);", prm)).Returns(1);

        conn.Object.Upsert<DbMonitoring>(prm);

        Mock.VerifyAll();
    }
}
