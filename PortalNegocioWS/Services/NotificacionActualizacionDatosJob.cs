using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Negocio.Business;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Services
{   
    public class NotificacionActualizacionDatosJob : CronJobService
    {
        private readonly ILogger<NotificacionActualizacionDatosJob> _logger;
        //private readonly INotificacion _notificacion;
        private readonly IServiceScopeFactory _serviceScopeFactory;


        public NotificacionActualizacionDatosJob(IScheduleConfig<NotificacionActualizacionDatosJob> config, ILogger<NotificacionActualizacionDatosJob> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(config.CronExpression, config.TimeZoneInfo, logger)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("JOB ENVIAR ACTUALIZACION DE DATOS PN INICIADO.");
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            DateTime fechaEjecucion = DateTime.Now;
            _logger.LogInformation($"{fechaEjecucion:hh:mm:ss} JOB ENVIAR ACTUALIZACION DE DATOS PN ESTA FUNCIONADO.");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _notificacion = scope.ServiceProvider.GetRequiredService<INotificacion>();

                try
                {
                    _notificacion.GenerarNotificacion(Configuracion.NotificacionActualizacionDatos, null);
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"ERROR JOB ENVIAR ACTUALIZACION DE DATOS: { e.StackTrace }");
                }    
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("JOB ENVIAR ACTUALIZACION DE DATOS PN DETENIDO.");
            return base.StopAsync(cancellationToken);
        }

    }
}
