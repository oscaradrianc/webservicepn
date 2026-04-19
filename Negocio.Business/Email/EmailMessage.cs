using System.Collections.Generic;

namespace Negocio.Business.Email
{
    public record EmailMessage(
        List<string> Recipients,
        string Subject,
        string Body,
        bool Bcc = false
    );
}