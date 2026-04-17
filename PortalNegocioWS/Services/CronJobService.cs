using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Negocio.Business;
using Microsoft.Extensions.Logging;

namespace PortalNegocioWS.Services
{
    public abstract class CronJobService : BackgroundService
    {
        private readonly CronExpression _expression;
        private readonly TimeZoneInfo _timeZoneInfo;
        private readonly ILogger _logger;

        protected CronJobService(string cronExpression, TimeZoneInfo timeZoneInfo, ILogger logger)
        {
            _expression = CronExpression.Parse(cronExpression);
            _timeZoneInfo = timeZoneInfo;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var next = _expression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
                    if (!next.HasValue)
                        return;

                    var delay = next.Value - DateTimeOffset.Now;
                    if (delay.TotalMilliseconds <= 0)
                        continue;

                    await LongDelay(delay, stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                        return;

                    await DoWork(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CronJob error in {Job}", GetType().Name);
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }
        }

        private static async Task LongDelay(TimeSpan delay, CancellationToken cancellationToken)
        {
            var remaining = delay;
            while (remaining > TimeSpan.Zero)
            {
                var chunk = remaining > TimeSpan.FromMilliseconds(int.MaxValue - 1)
                    ? TimeSpan.FromMilliseconds(int.MaxValue - 1)
                    : remaining;
                await Task.Delay(chunk, cancellationToken);
                remaining -= chunk;
            }
        }

        public virtual async Task DoWork(CancellationToken cancellationToken)
        {
            await Task.Delay(5000, cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }

    public interface IScheduleConfig<T>
    {
        string CronExpression { get; set; }
        TimeZoneInfo TimeZoneInfo { get; set; }
    }

    public class ScheduleConfig<T> : IScheduleConfig<T>
    {
        public string CronExpression { get; set; }
        public TimeZoneInfo TimeZoneInfo { get; set; }
    }

    public static class ScheduledServiceExtensions
    {
        public static IServiceCollection AddCronJob<T>(this IServiceCollection services, Action<IScheduleConfig<T>> options) where T : CronJobService
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), @"Please provide Schedule Configurations.");
            }
            var config = new ScheduleConfig<T>();
            options.Invoke(config);
            if (string.IsNullOrWhiteSpace(config.CronExpression))
            {
                throw new ArgumentNullException(nameof(ScheduleConfig<T>.CronExpression), @"Empty Cron Expression is not allowed.");
            }

            services.AddSingleton<IScheduleConfig<T>>(config);
            services.AddHostedService<T>();
            return services;
        }
    }
}
