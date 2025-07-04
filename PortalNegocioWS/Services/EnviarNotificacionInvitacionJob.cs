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


        public EnviarNotificacionInvitacionJob(IScheduleConfig<EnviarNotificacionInvitacionJob> config, ILogger<EnviarNotificacionInvitacionJob> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(config.CronExpression, config.TimeZoneInfo, logger)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
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

                using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
                {
                    var q = from s in cx.PONESOLICITUDCOMPRAs
                            where s.SOCOESTADO == Configuracion.EstadoSolicitudPublicado && s.SOCOENVIOPROV == "N"
                            select s;

                    using (PORTALNEGOCIODataContext cx1 = new PORTALNEGOCIODataContext())
                    {
                        cx1.Connection.Open();
                        //using (var dbContextTransaction = cx1.Connection.BeginTransaction())
                        //{
                        try
                        {
                            foreach (var solicitud in q)
                            {
                                var soli = cx1.PONESOLICITUDCOMPRAs.Where(s => s.SOCOSOLICITUD == solicitud.SOCOSOLICITUD).SingleOrDefault();

                                if (soli != null)
                                {
                                    var infoSolicitud = new SolicitudCompra { CodigoSolicitud = (int)soli.SOCOSOLICITUD, Descripcion = soli.SOCODESCRIPCION };
                                    _notificacion.GenerarNotificacion(Configuracion.NotificacionPublicacionInvitacion, infoSolicitud);

                                    soli.SOCOENVIOPROV = Configuracion.ValorSI;
                                    cx1.SubmitChanges();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogInformation($"ERROR JOB ENVIAR INVITACION: { e.StackTrace }");
                        }


                        //  dbContextTransaction.Commit();
                        //}

                    }
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
