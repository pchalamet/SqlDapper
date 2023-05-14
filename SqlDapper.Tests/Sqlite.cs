namespace SqlDapper.Tests;
using SqlDapper;
using System.Data.SQLite;
using NUnit.Framework;

[Table("Articles")]
public record Article([property: Key] string Name, int Price);

public class SqliteTests {
    [Test]
    public void CreateAndSelect() {
        var rnd = new Random();
        var dbFile = Path.GetTempFileName();
        try {
            // connect using generic provider
            using var cnx = new DapperConnection<SQLiteConnection>($"Data Source={dbFile}");
            cnx.Execute("CREATE TABLE IF NOT EXISTS Articles (Name varchar(128), Price int)", new { });

            // INSERT
            var insertArticle = new Article(Guid.NewGuid().ToString(), rnd.Next(100));
            cnx.Insert<Article>(insertArticle);

            // SELECT
            var selectArticle = cnx.Select<Article>(new { Name = insertArticle.Name }).Single();
            Assert.That(selectArticle, Is.EqualTo(insertArticle));

            // UPDATE
            var updateArticle = insertArticle with { Price = 100 + rnd.Next(100) };
            cnx.Update<Article>(updateArticle);

            selectArticle = cnx.Select<Article>(new { Name = updateArticle.Name }).Single();
            Assert.That(selectArticle, Is.EqualTo(updateArticle));

            // DELETE
            cnx.Delete<Article>(new { Name = updateArticle.Name });
            var deleteArticle = cnx.Select<Article>(new { Name = updateArticle.Name }).SingleOrDefault();
            Assert.That(deleteArticle, Is.Null);
        } finally {
            File.Delete(dbFile);
        }
    }
}
