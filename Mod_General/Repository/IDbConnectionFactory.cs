using System.Data;

namespace Mod_General.Repository
{
    public interface IDbConnectionFactory
    {

        IDbConnection CreateConnection();

    }
}
