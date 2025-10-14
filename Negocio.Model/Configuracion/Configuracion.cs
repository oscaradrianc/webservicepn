namespace Negocio.Model
{
    public static class Configuracion
    {
        public const string StatusOk = "OK";
        public const string StatusError = "ERROR";
        public const string MsjNotFound = "NotFound";
        public const string ValorSI = "S";
        public const string ValorNO = "N";
        public const string MsjModeloInvalido = "Parámetros enviados no son validos";

        public const string EstadoGerencia = "3";
        public const string EstadoSolicitudPublicado = "5";
        public const string EstadoSolicitudCerrado = "7";

        public const string EstadoActivo = "A";
        public const string EstadoInactivo = "I";
        public const string EstadoPendiente = "P";
        public const string EstadoRechazado = "R";

        public const int TipoIdentificacionCedula = 1;
        public const int TipoIdentificacionNIT = 4;

        public const int DocumentoFormatoProveedor = 1;

        public const int TipoPersonaNatural = 1;
        public const int TipoPersonaJuridica = 2;

        public const string TipoPresupuestoSaia = "S";
        public const string TipoPresupuestoFac = "F";

        public const string TipoAutorizacionGerencia = "G";
        public const string TipoAutorizacionCompras = "C";

        public const string TipoEstadoAdjudicado = "A";
        public const string TipoEstadoDesierto = "D";


        #region referencianotificaciones
        public const string NotificacionProvAutorizado = "autorizacionproveedo";
        public const string NotificacionProvRechazado = "rechazoproveedor";
        public const string NotificacionRegProveedor = "registroproveedor";
        public const string NotificacionAutoGerencia = "autorizagerencia";
        public const string NotificacionAutoCompras = "autorizacompras";
        public const string NotificacionResetPassword = "resetpassword";
        public const string NotificacionPublicacionInvitacion = "invitacionacotizar";
        public const string NotificacionActualizacionDatos = "actualizaciondatos";
        public const string NotificacionConfirmacionCotizacion = "confirmacioncotizaci";
        public const string NotificacionConfirmacionRegistroProveedor = "confregistroprov";
        public const string NotificacionRegistroCotizacion = "registrocotizacion";
        public const string NotificacionNuevoUsuario = "nuevousuario";
        public const string NotificacionAjudicado = "adjudicacion";
        public const string NotificacionDesierto = "desierta";

        #endregion

        #region Id Clases

        public const int ClaseUnidadMedida = 4;
        public const int ClaveValorDocSarlaft = 374;

        #endregion

        #region Tipo Usuario
        public const string TipoUsuarioInterno = "I";
        public const string TipoUsuarioProveedor = "P";
        #endregion

    }
}