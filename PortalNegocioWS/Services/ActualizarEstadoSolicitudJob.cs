using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Negocio.Data;
using Negocio.Model;

namespace PortalNegocioWS.Services
{
    public class ActualizarEstadoSolicitudJob : CronJobService
    {
        private readonly ILogger<ActualizarEstadoSolicitudJob> _logger;
        private readonly IDataContextFactory _factory;


        public ActualizarEstadoSolicitudJob(IScheduleConfig<ActualizarEstadoSolicitudJob> config, ILogger<ActualizarEstadoSolicitudJob> logger, IDataContextFactory factory)
            : base(config.CronExpression, config.TimeZoneInfo, logger)
        {
            _logger = logger;
            _factory = factory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("JOB PN INICIADO.");
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            DateTime fechaEjecucion = DateTime.Now;
            _logger.LogInformation($"{fechaEjecucion:hh:mm:ss} JOB PN ESTA FUNCIONADO.");

            try
            {
                using (var cx = _factory.Create())
                {
                    cx.Connection.Open();
                    using (var dbContextTransaction = cx.Connection.BeginTransaction())
                    {
                        var solicitudes = (from s in cx.PONESOLICITUDCOMPRAs
                                           where s.SOCOESTADO == Configuracion.EstadoSolicitudPublicado
                                             && Convert.ToDateTime(s.SOCOFECHACIERRE).Date < fechaEjecucion.Date
                                           select s).ToList();

                        foreach (var soli in solicitudes)
                        {
                            soli.SOCOESTADO = Configuracion.EstadoSolicitudCerrado;
                            soli.USUAUSUARIO = -1;
                            cx.SubmitChanges();
                        }

                        dbContextTransaction.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ERROR JOB ACTUALIZAR ESTADO SOLICITUD: {Message}", e.Message);
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("JOB PN DETENIDO.");
            return base.StopAsync(cancellationToken);
        }
    }
}
