using System;
using Microsoft.Extensions.Configuration;

namespace Negocio.Data
{
    public class DataContextFactory : IDataContextFactory
    {
        private readonly string _connectionString;

        public DataContextFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PORTALNEGOCIODataContextConnectionString")
                ?? throw new InvalidOperationException(
                    "Connection string 'PORTALNEGOCIODataContextConnectionString' not found in configuration.");
        }

        public PORTALNEGOCIODataContext Create()
        {
            return new PORTALNEGOCIODataContext(_connectionString);
        }
    }
}
