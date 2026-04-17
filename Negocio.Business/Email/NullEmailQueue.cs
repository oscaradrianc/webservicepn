namespace Negocio.Business.Email
{
    /// <summary>
    /// No-op IEmailQueue for call sites that instantiate NotificacionBusiness
    /// directly instead of using DI. Prefer injecting INotificacion.
    /// </summary>
    public class NullEmailQueue : IEmailQueue
    {
        public void Queue(EmailMessage message)
        {
            // Intentionally no-op: direct NotificacionBusiness instantiations
            // outside DI should be refactored to inject INotificacion.
        }
    }
}
