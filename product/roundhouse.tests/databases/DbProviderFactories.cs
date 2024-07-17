using System.Data.Common;
using Microsoft.Data.SqlClient;
using roundhouse.databases;
using roundhouse.infrastructure.logging.custom;
using Should;

namespace roundhouse.tests.databases
{
    using consoles;
    using roundhouse.databases.sqlserver;
    using roundhouse.infrastructure.app;

    public class DbProviderFactories
    {
        public interface ITestableDatabase
        {
            DbProviderFactory factory { get; }
        }

        public class TestableSqlServerDatabase : SqlServerDatabase, ITestableDatabase
        {
            public DbProviderFactory factory => get_db_provider_factory();
        }
    }

    public abstract class concern_for_Database<TDatabase> : TinySpec<AdoNetDatabase> where TDatabase : AdoNetDatabase, DbProviderFactories.ITestableDatabase, new()
    {
        protected static ConfigurationPropertyHolder configuration_property_holder;

        public concern_for_Database()
        {
            sut = new TDatabase();
        }

        protected override AdoNetDatabase sut { get; set; }
        protected DbProviderFactories.ITestableDatabase testable_sut() => (TDatabase)sut;

        public override void Context()
        {
            configuration_property_holder = new DefaultConfiguration
            {
                Logger = new Log4NetLogFactory().create_logger_bound_to(typeof(DbProviderFactories))
            };
        }

        public override void Because()
        {
            set_database_properties();
            sut.initialize_connections(configuration_property_holder);
        }

        protected virtual void set_database_properties()
        {
            sut.connection_string = "Data Source=|DataDirectory|Northwind.mdb";
            sut.database_name = "Bob";
            sut.server_name = "SQLEXPRESS";
        }
    }

    [Concern(typeof(DbProviderFactories.TestableSqlServerDatabase))]
    public class concern_for_SqlServerDatabase : concern_for_Database<DbProviderFactories.TestableSqlServerDatabase>
    {
        [Observation]
        public void has_sql_server_provider_factory()
        {
            DbProviderFactory fac = testable_sut().factory;
            fac.ShouldBeType<SqlClientFactory>();
        }
    }
}