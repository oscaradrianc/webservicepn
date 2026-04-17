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
    public class EnviarNotificacionInvitacionJob : CronJobService
    {
        private readonly ILogger<EnviarNotificacionInvitacionJob> _logger;
        //private readonly INotificacion _notificacion;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IDataContextFactory _factory;


        public EnviarNotificacionInvitacionJob(IScheduleConfig<EnviarNotificacionInvitacionJob> config, ILogger<EnviarNotificacionInvitacionJob> logger,
            IServiceScopeFactory serviceScopeFactory, IDataContextFactory factory)
            : base(config.CronExpression, config.TimeZoneInfo, logger)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _factory = factory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("JOB ENVIAR INVITACION PN INICIADO.");
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            DateTime fechaEjecucion = DateTime.Now;
            _logger.LogInformation($"{fechaEjecucion:hh:mm:ss} JOB ENVIAR INVITACION PN ESTA FUNCIONADO.");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _notificacion = scope.ServiceProvider.GetRequiredService<INotificacion>();

                try
                {
                    using (var cx = _factory.Create())
                    {
                        cx.Connection.Open();
                        // Transaction remains disabled (same as original)
                        var solicitudes = (from s in cx.PONESOLICITUDCOMPRAs
                                           where s.SOCOESTADO == Configuracion.EstadoSolicitudPublicado && s.SOCOENVIOPROV == "N"
                                           select s).ToList();

                        foreach (var soli in solicitudes)
                        {
                            var infoSolicitud = new SolicitudCompra { CodigoSolicitud = (int)soli.SOCOSOLICITUD, Descripcion = soli.SOCODESCRIPCION };
                            _notificacion.GenerarNotificacion(Configuracion.NotificacionPublicacionInvitacion, infoSolicitud);

                            soli.SOCOENVIOPROV = Configuracion.ValorSI;
                            cx.SubmitChanges();
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "ERROR JOB ENVIAR INVITACION: {Message}", e.Message);
                }
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("JOB ENVIAR INVITACION PN DETENIDO.");
            return base.StopAsync(cancellationToken);
        }

    }
}
