using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Negocio.Business;
using Negocio.Data;
using PortalNegocioWS.Services;
using Xunit;

namespace PortalNegocioWS.Tests.Jobs;

public class CronJobTests
{
    private static Mock<IScheduleConfig<T>> BuildScheduleConfig<T>()
    {
        var mock = new Mock<IScheduleConfig<T>>();
        mock.Setup(c => c.CronExpression).Returns("0 0 * * *");
        mock.Setup(c => c.TimeZoneInfo).Returns(TimeZoneInfo.Utc);
        return mock;
    }

    [Fact]
    public async Task ActualizarEstadoSolicitudJob_DoWork_WhenFactoryThrows_DoesNotPropagate()
    {
        var mockFactory = new Mock<IDataContextFactory>();
        mockFactory.Setup(f => f.Create())
            .Throws(new InvalidOperationException("No Oracle in tests"));

        var job = new ActualizarEstadoSolicitudJob(
            BuildScheduleConfig<ActualizarEstadoSolicitudJob>().Object,
            new Mock<ILogger<ActualizarEstadoSolicitudJob>>().Object,
            mockFactory.Object);

        var exception = await Record.ExceptionAsync(() =>
            job.DoWork(CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task EnviarNotificacionInvitacionJob_DoWork_WhenFactoryThrows_DoesNotPropagate()
    {
        var mockFactory = new Mock<IDataContextFactory>();
        mockFactory.Setup(f => f.Create())
            .Throws(new InvalidOperationException("No Oracle in tests"));

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockScope = new Mock<IServiceScope>();
        var mockProvider = new Mock<IServiceProvider>();
        mockScope.Setup(s => s.ServiceProvider).Returns(mockProvider.Object);
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);
        mockProvider.Setup(p => p.GetService(typeof(INotificacion)))
            .Returns(new Mock<INotificacion>().Object);

        var job = new EnviarNotificacionInvitacionJob(
            BuildScheduleConfig<EnviarNotificacionInvitacionJob>().Object,
            new Mock<ILogger<EnviarNotificacionInvitacionJob>>().Object,
            mockScopeFactory.Object,
            mockFactory.Object);

        var exception = await Record.ExceptionAsync(() =>
            job.DoWork(CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task NotificacionActualizacionDatosJob_DoWork_WhenNotificacionThrows_DoesNotPropagate()
    {
        var mockScope = new Mock<IServiceScope>();
        var mockProvider = new Mock<IServiceProvider>();
        mockScope.Setup(s => s.ServiceProvider).Returns(mockProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

        var mockNotificacion = new Mock<INotificacion>();
        mockNotificacion.Setup(n => n.GenerarNotificacion(It.IsAny<string>(), It.IsAny<object>()))
            .Throws(new InvalidOperationException("Notification failed"));

        mockProvider.Setup(p => p.GetService(typeof(INotificacion)))
            .Returns(mockNotificacion.Object);

        var job = new NotificacionActualizacionDatosJob(
            BuildScheduleConfig<NotificacionActualizacionDatosJob>().Object,
            new Mock<ILogger<NotificacionActualizacionDatosJob>>().Object,
            mockScopeFactory.Object);

        var exception = await Record.ExceptionAsync(() =>
            job.DoWork(CancellationToken.None));

        Assert.Null(exception);
    }
}
