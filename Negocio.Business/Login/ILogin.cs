using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.Business
{
    public interface ILogin
    {
        Response<Usuario> Authenticate(LoginRequest login);
        ResponseStatus ChangePassword(ChangePasswordRequest changePassword);
        ResponseStatus ResetPassword(ResetPassRequest req);

    }
}
