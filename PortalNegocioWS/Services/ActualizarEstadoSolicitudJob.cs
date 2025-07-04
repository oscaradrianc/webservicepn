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
        

        public ActualizarEstadoSolicitudJob(IScheduleConfig<ActualizarEstadoSolicitudJob> config, ILogger<ActualizarEstadoSolicitudJob> logger)
            : base(config.CronExpression, config.TimeZoneInfo, logger)
        {
            _logger = logger;        
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

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                var q = from s in cx.PONESOLICITUDCOMPRAs
                        where s.SOCOESTADO == Configuracion.EstadoSolicitudPublicado
                          && Convert.ToDateTime(s.SOCOFECHACIERRE).Date < fechaEjecucion.Date
                        select s;

                using (PORTALNEGOCIODataContext cx1 = new PORTALNEGOCIODataContext())
                {
                    cx1.Connection.Open();
                    using (var dbContextTransaction = cx1.Connection.BeginTransaction())
                    {

                        foreach (var solicitud in q)
                        {
                            var soli = cx1.PONESOLICITUDCOMPRAs.Where(s => s.SOCOSOLICITUD == solicitud.SOCOSOLICITUD).SingleOrDefault();

                            if (soli != null)
                            {
                                soli.SOCOESTADO = Configuracion.EstadoSolicitudCerrado;
                                soli.USUAUSUARIO = -1;

                                cx1.SubmitChanges();

                            }
                        }

                        dbContextTransaction.Commit();
                    }
                }
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
