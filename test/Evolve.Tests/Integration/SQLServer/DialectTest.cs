using EvolveDb.Connection;
using EvolveDb.Dialect;
using EvolveDb.Dialect.SQLServer;
using EvolveDb.Tests.Infrastructure;
using System.Data.SqlClient;
using System.Xml.Linq;
using Xunit;
using static EvolveDb.Tests.TestContext;

namespace EvolveDb.Tests.Integration.SQLServer
{
    public class DialectTest : DbContainerFixture<SQLServerContainer>
    {
        public const string DbName = "my_database_1";

        public override bool FromScratch => Local;

        [Fact]
        [Category(Test.SQLServer)]
        public void Run_all_SQLServer_integration_tests_work()
        {
            // Arrange
            TestUtil.CreateSqlServerDatabase(DbName, CnxStr);
            var cnn = new SqlConnection(CnxStr.Replace("master", DbName));
            var wcnn = new WrappedConnection(cnn).AssertDatabaseServerType(DBMS.SQLServer);
            var db = DatabaseHelperFactory.GetDatabaseHelper(DBMS.SQLServer, wcnn);
            string schemaName = "dbo";
            var schema = new SQLServerSchema(schemaName, wcnn);

            // Assert
            db.AssertDefaultSchemaName(schemaName)
              .AssertApplicationLock(new SqlConnection(CnxStr.Replace("master", DbName)))
              .AssertMetadataTableCreation(schemaName, "changelog")
              .AssertMetadataTableLock()
              .AssertSchemaIsErasableWhenEmptySchemaFound(schemaName) // id:1
              .AssertVersionedMigrationSave() // id:2
              .AssertVersionedMigrationChecksumUpdate()
              .AssertRepeatableMigrationSave(); // id:3

            schema.AssertIsNotEmpty();
            schema.Erase();
            schema.AssertIsEmpty();

            db.AssertCloseConnection();
        }
    }
}
