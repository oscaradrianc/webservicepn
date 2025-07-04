using Microsoft.Extensions.Configuration;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.Business
{
    public interface ILogin
    {
        Response<Usuario> Authenticate(LoginRequest login, IConfiguration configuration);
        ResponseStatus ChangePassword(ChangePasswordRequest changePassword, IConfiguration configuration);
        ResponseStatus ResetPassword(ResetPassRequest req, IConfiguration configuration);

    }
}
