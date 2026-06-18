using System.Data.Common;

namespace SerenissimaSql.ConnectionFactories;

public interface IDbConnectionFactory
{
    DbConnection Create();
}