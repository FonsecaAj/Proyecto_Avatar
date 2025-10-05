using System.Data;

namespace EjemploMicroServicio.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

}
