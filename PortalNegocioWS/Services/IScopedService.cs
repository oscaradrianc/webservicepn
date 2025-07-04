using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PortalNegocioWS.Services
{
    public interface IScopedService
    {
        Task DoWork(CancellationToken cancellationToken);
    }

    public class ScopedService : IScopedService
    {
        private readonly ILogger<ScopedService> _logger;

        public ScopedService(ILogger<ScopedService> logger)
        {
            _logger = logger;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation("{now} MyScopedService is working.", DateTime.Now.ToString("T"));
            await Task.Delay(1000 * 20, cancellationToken);
        }
    }
}
