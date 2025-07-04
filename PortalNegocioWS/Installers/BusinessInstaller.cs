using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Negocio.Business;
using Negocio.Business.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortalNegocioWS.Installers
{
    public class BusinessInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISolicitudCompra, SolicitudBusiness>();
            services.AddScoped<ICatalogo, CatalogoBusiness>();
            services.AddScoped<IOpcion, OpcionBusiness>();
            services.AddScoped<INotificacion, NotificacionBusiness>();
            services.AddScoped<INoticias, NoticiasBusiness>();
            services.AddScoped<IRol, RolBusiness>();
            services.AddScoped<ICotizacion, CotizacionBusiness>();
            services.AddScoped<IPreguntas, PreguntasBusiness>();
            services.AddScoped<IProveedor, ProveedorBusiness>();
            services.AddScoped<IUtilidades, UtilidadesBusiness>();
            services.AddScoped<ILogin, LoginBusiness>();
            services.AddScoped<IUsuario, UsuarioBusiness>();
            services.AddScoped<IConsultas, ConsultasBusiness>();
            services.AddScoped<IArchivoExcel, ArchivoExcelBusiness>();
            services.AddScoped<INotificacionUsuario, NotificacionUsuarioBusiness>();
            services.AddScoped<IParametroGeneral, ParametroGeneral>();
            services.AddScoped<IAutorizadorGerencia, AutorizadorGerenciaBusiness>();
            services.AddScoped<IConstante, ConstanteBusiness>();   
        }
    }
}
