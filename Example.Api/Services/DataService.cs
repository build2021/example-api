namespace Example.Api.Services
{
    using System.Data.Common;
    using System.Threading.Tasks;

    using Example.Api.Models.Entity;

    using Smart.Data;
    using Smart.Data.Mapper;
    using Smart.Data.Mapper.Builders;

    public class DataService
    {
        private IDbProvider DbProvider { get; }

        private IDialect Dialect { get; }

        public DataService(
            IDbProvider dbProvider,
            IDialect dialect)
        {
            DbProvider = dbProvider;
            Dialect = dialect;
        }

        public ValueTask<DataEntity?> QueryDataAsync(int id) =>
            DbProvider.UsingAsync(con =>
                con.QueryFirstOrDefaultAsync<DataEntity?>(
                    SqlSelect<DataEntity>.ByKey(),
                    new { Id = id }));

        public ValueTask<bool> InsertDataAsync(DataEntity entity) =>
            DbProvider.UsingAsync(async con =>
            {
                try
                {
                    await con.ExecuteAsync(SqlInsert<DataEntity>.Values(), entity);

                    return true;
                }
                catch (DbException e)
                {
                    if (Dialect.IsDuplicate(e))
                    {
                        return false;
                    }

                    throw;
                }
            });
    }
}
