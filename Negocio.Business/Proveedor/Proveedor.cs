using System;
using Negocio.Data;
using Negocio.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Devart.Data.Linq;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Negocio.Business.Utilidades;
using System.IO;

namespace Negocio.Business
{
    public class ProveedorBusiness : IProveedor
    {
        private readonly IUtilidades _utilidades;
        private readonly IConfiguration _configuration;
        private readonly IUsuario _usuario;
        private readonly IParametroGeneral _parametroGeneral;
        private readonly IStorageService _storageService;

        public ProveedorBusiness(IUtilidades utilidades, IConfiguration configuration, IUsuario usuario, IParametroGeneral parametroGeneral, IStorageService storageService)
        {
            _utilidades = utilidades;
            _configuration = configuration;
            _usuario = usuario;
            _parametroGeneral = parametroGeneral;
            _storageService = storageService;
        }

        #region Metodos Publicos

        /// <summary>
        /// Metodo para crear el proveedor en el sistema
        /// </summary>
        /// <param name="request">Objeto complejo proveedor</param>
        /// <returns>OK si todo esta bien o el mensaje de error</returns>
        public async Task<ResponseStatus> RegistrarProveedor(Proveedor request)
        {
            ResponseStatus resp = new();

            using (PORTALNEGOCIODataContext cx = new())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        if (!_storageService.ExistsDirectory())
                        {
                            resp.Status = Configuracion.StatusError;
                            resp.Message = "No existe Directorio para almacenar archivos, por favor validar con el área de compras de EEP.";
                            return resp;
                        }

                        bool personaNatural = request.TipoPersona == 1;

                        var existeProveedor = cx.PONEPROVEEDORs.Where(p => p.PROVIDENTIFICACION == (personaNatural ? request.Documento : request.Nit)).SingleOrDefault();

                        if(existeProveedor != null)
                        {
                            resp.Status = Configuracion.StatusError;
                            resp.Message = $"Ya existe un proveedor con el numero de identificación: { (personaNatural ? request.Documento : request.Nit) }";
                            await Task.Run(() => dbContextTransaction.Rollback());
                        }
                        else
                        {
                            //Proveedor
                            var tblProveedor = new PONEPROVEEDOR();
                            int codigoProveedor = _utilidades.GetSecuencia("SECU_PONEPROVEEDOR", cx);
                            tblProveedor.PROVPROVEEDOR = codigoProveedor;
                            tblProveedor.CLASTIPOPERSONAL1 = request.TipoPersona;
                            tblProveedor.CLASTIPOIDENTIFICACION2 = personaNatural ? request.TipoDocumento : 4; //TODO: Revisar para que no quede quemado
                            tblProveedor.PROVIDENTIFICACION = personaNatural ? request.Documento : request.Nit;
                            tblProveedor.PROVLUGAREXPEDICION = request.LugarExpedicion;
                            tblProveedor.PROVRAZONSOCIAL = personaNatural ? request.Nombre : request.NombreJuridica;
                            tblProveedor.CLASTIPOIDENTREPRES2 = request.TipoDocumentoRep;
                            tblProveedor.PROVIDENTREPRESENTANTE = request.DocumentoRep;
                            tblProveedor.PROVREPRESENTANTELEGAL = request.NombreRep;
                            tblProveedor.PROVFECEXPIDENTREPRESENTANTE = request.FechaExpedicionRep;
                            tblProveedor.PROVLUGAREXPIDENTREPRESENT = request.LugarExpedicionRep;
                            tblProveedor.PROVFECHANACIMIENTOREP = request.FechaNacimientoRep;
                            tblProveedor.PROVLUGARNACIMIENTOREP = request.LugarNacimientoRep;
                            tblProveedor.PROVFECHANACIMIENTO = request.FechaNacimiento;
                            tblProveedor.PROVLUGARNACIMIENTO = request.LugarNacimiento;
                            //tblProveedor.PROV_REGIMENTRIBUTARIO = ""; 
                            tblProveedor.PROVDIRECCIONPRINCIAL = personaNatural ? request.DireccionResidencia : request.DireccionJuridica;
                            tblProveedor.PROVDIRECCIONCOMERCIAL = request.DireccionComercial;
                            tblProveedor.PROVTELEFONO = personaNatural ? request.Telefono : request.TelefonoPrincipal;
                            tblProveedor.PROVEMAIL = request.Email;
                            tblProveedor.PROVFECHAINSCRIPCION = DateTime.Now;
                            tblProveedor.PROVESTADO = "P"; //Pendiente por Autorizar
                            tblProveedor.ACECCODIGOACTIVIDAD = request.ActividadEconomica;
                            tblProveedor.PROVPROFESION = request.Profesion;
                            tblProveedor.PROVACTIVIDAD = request.Actividad;
                            tblProveedor.PROVEMPRESATRABAJA = request.Empresa;
                            tblProveedor.PROVCARGO = request.Cargo;
                            tblProveedor.PROVFAX = request.Fax;
                            tblProveedor.PROVCIUDAD = personaNatural ? request.Ciudad : request.CiudadJuridica;
                            tblProveedor.PROVTELEFONOEMPRESA = request.TelefonoEmpresa;
                            tblProveedor.PROVCIUDADEMPRESA = request.CiudadEmpresa;
                            tblProveedor.PROVDIRECCIONCOMERCIAL = request.DireccionComercial;
                            tblProveedor.PROVMANEJARECURSOPUBLICOS = ((request.ManejaRecursos != null) && (bool)request.ManejaRecursos) ? "S" : "N";
                            tblProveedor.PROVRECONOCIMIENTOPUBLICO = ((request.ReconocimientoPublico != null) && (bool)request.ReconocimientoPublico) ? "S" : "N";
                            tblProveedor.PROVPODERPUBLICO = ((request.PoderPublico != null) && (bool)request.PoderPublico) ? "S" : "N";
                            tblProveedor.PROVOBSERVACION = request.RespuestaAfirmativa;
                            //tblProveedor.ACECCODIGOACTIVIDAD = request.ActividadEconomica;
                            tblProveedor.CLASTIPOEMPRESA13 = request.TipoEmpresa;
                            tblProveedor.CLASSECTORECONOMICO14 = request.SectorEconomia;
                            tblProveedor.PROVNACIONALIDADREPRESENTANTE = request.NacionalidadRep;
                            tblProveedor.PROVINGRESOSMENSUALES = request.IngresosMensuales;
                            tblProveedor.PROVEGRESOSMENSUALES = request.EgresosMensuales;
                            tblProveedor.PROVACTIVOS = request.Activos;
                            tblProveedor.PROVPASIVOS = request.Pasivos;
                            tblProveedor.PROVPATRIMONIO = request.Patrimonio;
                            tblProveedor.PROVOTROSINGRESOS = request.OtrosIngresos;
                            tblProveedor.PROVCONCEPTOOTROSINGRESOS = request.ConceptoOtrosIngresos;
                            tblProveedor.PROVTRANSMONEDAEXT = request.MonedaExtranjera ? "S" : "N";
                            tblProveedor.CLASTIPOMONEDA5 = request.TipoMonedaExtranjera;
                            tblProveedor.PROVPRODFINANEXT = request.CuentasMonedaExtranjera ? "S" : "N";
                            tblProveedor.PROVENTIDADESTATAL = request.EntidadEstatal != null ? request.EntidadEstatal == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO;
                            tblProveedor.PROVENTIDADSINANILUCRO = request.EntidadSinLucro != null ? request.EntidadSinLucro == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO; 
                            tblProveedor.PROVRESOLENTSINANILUCRO = request.ResolEntidadSinLucro;
                            tblProveedor.PROVGRANCONTRIBUYENTE = request.GranContribuyente != null ? request.GranContribuyente == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO;
                            tblProveedor.PROVRESOLGRANCONTRIBUYENTE = request.ResolGranContribuyente;
                            tblProveedor.PROVRESPONSABLEIVA = request.ResponsableIVA != null ? request.ResponsableIVA == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO;
                            tblProveedor.PROVAUTORRETENEDOR = request.Autorretenedor != null ? request.Autorretenedor == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO; 
                            tblProveedor.PROVRESOLAUTORRETENEDOR = request.ResolAutorretenedor;                            
                            tblProveedor.PROVCONTRIBUYENTERENTA = request.ContribuyenteRenta != null ? request.ContribuyenteRenta == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO; 
                            tblProveedor.PROVAGENTERETENEDORIVA = request.AgenteRetenedorIVA != null ? request.AgenteRetenedorIVA == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO;
                            tblProveedor.PROVRESOLAGENTERETENEDORIVA = request.ResolAgenteRetenedorIVA;
                            tblProveedor.PROVINDUSTRIAYCOMERCIO = request.ResponsableIndyComer != null ? request.ResponsableIndyComer == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO;
                            //tblProveedor.PROVACTICOMERCIALOTROSMUNI = request.ResponsableOtros ? "S" : "N";
                            tblProveedor.CLASENTIDADBANCARIA6 = request.EntidadBancaria;
                            tblProveedor.PROVSUCURSALBANCO = request.Sucursal;
                            tblProveedor.PROVCIUDADSUCURSAL = request.CiudadSucursal;
                            tblProveedor.PROVTELEFONOSUCURSAL = request.TelefonoSucursal;
                            tblProveedor.PROVDIRECCIONSUCURSAL = request.DireccionSucursal;
                            tblProveedor.PROVNUMEROCUENTA = request.Cuenta;
                            tblProveedor.CLASTIPOCUENTA7 = request.TipoCuenta;
                            tblProveedor.PROVTITULARCUENTA = request.TitularCuenta;
                            tblProveedor.PROVIDENTITULARCUENTA = request.IdentificacionCuenta;
                            tblProveedor.CLASACTPEREIRA14 = request.ActEcoPereira;
                            //tblProveedor.CLASACTOTROS14 = request.ActEcoOtros;
                            tblProveedor.PROVCLASTAMANO = request.ClasificacionTamano;
                            tblProveedor.PROVCLASSECTOR = request.ClasificacionSector;
                            tblProveedor.PROVOPERMERCANTILES = request.OperacionesMercantiles ? "S" : "N";
                            tblProveedor.LOGSUSUARIO = request.LogsUsuario;

                            var tblUsuario = new POGEUSUARIO
                            {
                                USUAUSUARIO = _utilidades.GetSecuencia("SECU_POGEUSUARIO", cx),
                                USUAIDENTIFICACION = personaNatural ? request.Documento : request.Nit,
                                USUAIDENTIFICADOR = personaNatural ? request.Documento : request.Nit,
                                USUANOMBRE = personaNatural ? request.Nombre : request.NombreJuridica,
                                USUACORREO = request.Email,
                                USUACLAVE = ".",
                                LOGSFECHA = DateTime.Now,
                                PROVPROVEEDOR = codigoProveedor,
                                USUAESTADO = "I" //Estado Inactivo
                            };


                            cx.PONEPROVEEDORs.InsertOnSubmit(tblProveedor);
                            cx.SubmitChanges();

                            //Si no es persona natural y tiene accionistas, almacena los accionistas
                            if (!personaNatural && request.Accionistas.Count > 0)
                                CargarAccionista(request.Accionistas, cx, codigoProveedor, request.LogsUsuario);

                            //almacena las especialidades de la empresa
                            CargarEspecialidades(request.Especialidades, cx, codigoProveedor, request.LogsUsuario);
                            //Almacena los documentos anexos
                            await CargarAnexos(request.Certificaciones, cx, codigoProveedor, (int)tblUsuario.USUAUSUARIO);
                            //Crea el usuario en el sistema
                            //UsuarioBusiness usuarioBusiness = new UsuarioBusiness(_utilidades, _configuration);
                            //var resp = usuarioBusiness.CrearUsuarioProveedor(tblUsuario, cx);
                            var resp1 = _usuario.CrearUsuarioProveedor(tblUsuario, cx);

                            //Se crea para la plantilla del correo
                            Proveedor prov = new()
                            {
                                Nombre = personaNatural ? request.Nombre : request.NombreJuridica,
                                Nit = personaNatural ? request.Documento : request.Nit
                            };


                            //////////////////////////////////////////////////////////////////////////////////////////
                            if (resp1.Status == "ERROR")
                            {
                                await Task.Run(() => dbContextTransaction.Rollback()); 
                                resp.Status = resp1.Status;
                                resp.Message = "Error al crear el usuario, no se puedo generar el registro de proveedor";
                            }
                            else
                            {
                                //////////////////Envia Correo indicando nuevo registro de proveedor//////////////////////////
                                Thread t = new(() =>
                                    (new NotificacionBusiness(_utilidades)).GenerarNotificacion("registroproveedor", prov)
                                );
                                t.Start();
                                t.IsBackground = true;

                                Thread t1 = new(() =>
                                    (new NotificacionBusiness(_utilidades)).GenerarNotificacion("confregistroprov", request)
                                );
                                t1.Start();
                                t1.IsBackground = true;
                            }

                            await Task.Run(() => dbContextTransaction.Commit()); 

                            resp.Status = Configuracion.StatusOk;                            
                        }
                    }
                    catch (Exception ex)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback()); 
                        resp.Status = Configuracion.StatusError;
                        resp.Message = ex.Message;
                    }
                }
            }

            return resp;
        }


        /// <summary>
        /// Metodo para actualizar el proveedor en el sistema
        /// </summary>
        /// <param name="request">Objeto complejo proveedor</param>
        /// <returns>OK si todo esta bien o el mensaje de error</returns>
        public string ActualizarProveedor(Proveedor request)
        {
            using (PORTALNEGOCIODataContext cx = new())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        bool personaNatural = request.TipoPersona == 1;

                        //Proveedor
                        var tblProveedor = (from p in cx.PONEPROVEEDORs
                                            where p.PROVPROVEEDOR == request.CodigoProveedor
                                            select p).FirstOrDefault();

                        if(tblProveedor != null)
                        {
                            tblProveedor.CLASTIPOPERSONAL1 = request.TipoPersona;
                            tblProveedor.CLASTIPOIDENTIFICACION2 = personaNatural ? request.TipoDocumento : 4; //TODO: Revisar para que no quede quemado
                            tblProveedor.PROVIDENTIFICACION = personaNatural ? request.Documento : request.Nit;
                            tblProveedor.PROVLUGAREXPEDICION = request.LugarExpedicion;
                            tblProveedor.PROVRAZONSOCIAL = personaNatural ? request.Nombre : request.NombreJuridica;
                            tblProveedor.CLASTIPOIDENTREPRES2 = request.TipoDocumentoRep;
                            tblProveedor.PROVIDENTREPRESENTANTE = request.DocumentoRep;
                            tblProveedor.PROVREPRESENTANTELEGAL = request.NombreRep;
                            tblProveedor.PROVFECEXPIDENTREPRESENTANTE = request.FechaExpedicionRep;
                            tblProveedor.PROVLUGAREXPIDENTREPRESENT = request.LugarExpedicionRep;
                            tblProveedor.PROVFECHANACIMIENTOREP = request.FechaNacimientoRep;
                            tblProveedor.PROVLUGARNACIMIENTOREP = request.LugarNacimientoRep;
                            tblProveedor.PROVFECHANACIMIENTO = request.FechaNacimiento;
                            tblProveedor.PROVLUGARNACIMIENTO = request.LugarNacimiento;
                            tblProveedor.PROVDIRECCIONPRINCIAL = personaNatural ? request.DireccionResidencia : request.DireccionJuridica;
                            tblProveedor.PROVDIRECCIONCOMERCIAL = request.DireccionComercial;
                            tblProveedor.PROVTELEFONO = request.Telefono;
                            tblProveedor.PROVEMAIL = request.Email;
                            //tblProveedor.ACECCODIGOACTIVIDAD = request.ActividadEconomica;
                            tblProveedor.PROVPROFESION = request.Profesion;
                            tblProveedor.PROVACTIVIDAD = request.Actividad;
                            tblProveedor.PROVEMPRESATRABAJA = request.Empresa;
                            tblProveedor.PROVCARGO = request.Cargo;
                            tblProveedor.PROVFAX = request.Fax;
                            tblProveedor.PROVCIUDAD = personaNatural ? request.Ciudad : request.CiudadJuridica;
                            tblProveedor.PROVTELEFONOEMPRESA = request.TelefonoEmpresa;
                            tblProveedor.PROVCIUDADEMPRESA = request.CiudadEmpresa;
                            tblProveedor.PROVDIRECCIONCOMERCIAL = request.DireccionComercial;
                            tblProveedor.PROVMANEJARECURSOPUBLICOS = ((request.ManejaRecursos != null) && (bool)request.ManejaRecursos) ? "S" : "N";
                            tblProveedor.PROVRECONOCIMIENTOPUBLICO = ((request.ReconocimientoPublico != null) && (bool)request.ReconocimientoPublico) ? "S" : "N";
                            tblProveedor.PROVPODERPUBLICO = ((request.PoderPublico != null) && (bool)request.PoderPublico) ? "S" : "N";
                            tblProveedor.PROVOBSERVACION = request.RespuestaAfirmativa;
                            tblProveedor.ACECCODIGOACTIVIDAD = request.ActividadEconomica;
                            tblProveedor.CLASTIPOEMPRESA13 = request.TipoEmpresa;
                            tblProveedor.CLASSECTORECONOMICO14 = request.SectorEconomia;
                            tblProveedor.PROVNACIONALIDADREPRESENTANTE = request.NacionalidadRep;
                            tblProveedor.PROVINGRESOSMENSUALES = request.IngresosMensuales;
                            tblProveedor.PROVEGRESOSMENSUALES = request.EgresosMensuales;
                            tblProveedor.PROVACTIVOS = request.Activos;
                            tblProveedor.PROVPASIVOS = request.Pasivos;
                            tblProveedor.PROVPATRIMONIO = request.Patrimonio;
                            tblProveedor.PROVOTROSINGRESOS = request.OtrosIngresos;
                            tblProveedor.PROVCONCEPTOOTROSINGRESOS = request.ConceptoOtrosIngresos;
                            tblProveedor.PROVTRANSMONEDAEXT = request.MonedaExtranjera ? "S" : "N";
                            tblProveedor.CLASTIPOMONEDA5 = request.TipoMonedaExtranjera;
                            tblProveedor.PROVPRODFINANEXT = request.CuentasMonedaExtranjera ? "S" : "N";
                            tblProveedor.PROVENTIDADESTATAL = request.EntidadEstatal != null ? request.EntidadEstatal == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO;
                            tblProveedor.PROVENTIDADSINANILUCRO = request.EntidadSinLucro != null ? request.EntidadSinLucro == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO; 
                            tblProveedor.PROVRESOLENTSINANILUCRO = request.ResolEntidadSinLucro;
                            tblProveedor.PROVGRANCONTRIBUYENTE = request.GranContribuyente != null ? request.GranContribuyente == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO; 
                            tblProveedor.PROVRESOLGRANCONTRIBUYENTE = request.ResolGranContribuyente;
                            tblProveedor.PROVRESPONSABLEIVA = request.ResponsableIVA != null ? request.ResponsableIVA == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO;
                            tblProveedor.PROVAUTORRETENEDOR = request.Autorretenedor != null ? request.Autorretenedor == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO;
                            tblProveedor.PROVRESOLAUTORRETENEDOR = request.ResolAutorretenedor;
                            tblProveedor.PROVCONTRIBUYENTERENTA = request.ContribuyenteRenta != null ? request.ContribuyenteRenta == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO;
                            tblProveedor.PROVAGENTERETENEDORIVA = request.AgenteRetenedorIVA != null ? request.AgenteRetenedorIVA == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO;
                            tblProveedor.PROVRESOLAGENTERETENEDORIVA = request.ResolAgenteRetenedorIVA;
                            tblProveedor.PROVINDUSTRIAYCOMERCIO = request.ResponsableIndyComer != null ? request.ResponsableIndyComer == true ? Configuracion.ValorSI : Configuracion.ValorNO : Configuracion.ValorNO;
                            //tblProveedor.PROVACTICOMERCIALOTROSMUNI = request.ResponsableOtros ? "S" : "N";
                            tblProveedor.CLASENTIDADBANCARIA6 = request.EntidadBancaria;
                            tblProveedor.PROVSUCURSALBANCO = request.Sucursal;
                            tblProveedor.PROVCIUDADSUCURSAL = request.CiudadSucursal;
                            tblProveedor.PROVTELEFONOSUCURSAL = request.TelefonoSucursal;
                            tblProveedor.PROVDIRECCIONSUCURSAL = request.DireccionSucursal;
                            tblProveedor.PROVNUMEROCUENTA = request.Cuenta;
                            tblProveedor.CLASTIPOCUENTA7 = request.TipoCuenta;
                            tblProveedor.PROVTITULARCUENTA = request.TitularCuenta;
                            tblProveedor.PROVIDENTITULARCUENTA = request.IdentificacionCuenta;
                            tblProveedor.CLASACTPEREIRA14 = request.ActEcoPereira;
                            //tblProveedor.CLASACTOTROS14 = request.ActEcoOtros;
                            tblProveedor.PROVCLASTAMANO = request.ClasificacionTamano;
                            tblProveedor.PROVCLASSECTOR = request.ClasificacionSector;
                            tblProveedor.PROVOPERMERCANTILES = request.OperacionesMercantiles ? "S" : "N";
                            tblProveedor.PROVFECHAULTACT = DateTime.Now;
                            tblProveedor.LOGSUSUARIO = request.LogsUsuario;
                            cx.SubmitChanges();

                            //Si no es persona natural y tiene accionistas, almacena los accionistas
                            if (!personaNatural && request.Accionistas.Count > 0)
                                CargarAccionista(request.Accionistas, cx, request.CodigoProveedor, request.LogsUsuario);

                            //almacena las especialidades de la empresa
                            ActualizarEspecialidades(request.Especialidades, cx, request.CodigoProveedor, request.LogsUsuario);

                            dbContextTransaction.Commit();
                        }

                        return "OK";
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return ex.Message;
                    }
                }
            }
        }

        public string ActualizarDocsProveedor(Proveedor proveedor)
        {
            using (PORTALNEGOCIODataContext cx = new())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        //Proveedor
                        var tblProveedor = (from p in cx.PONEPROVEEDORs
                                            where p.PROVPROVEEDOR == proveedor.CodigoProveedor
                                            select p).FirstOrDefault();

                        if (tblProveedor != null)
                        {
                            ActualizarDocumentosProveedor(proveedor.Certificaciones, cx, proveedor.CodigoProveedor, proveedor.LogsUsuario);
                            cx.SubmitChanges();

                            dbContextTransaction.Commit();
                        }

                        return "OK";
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return ex.Message;
                    }

                }

            }
        }


        /// <summary>
        /// Obtiene la lista de proveedores por estado
        /// </summary>
        /// <param name="estado">estado actual del proveedor</param>
        /// <returns>Lista de Proveedores</returns>
        public async Task<List<Proveedor>> ObtenerProveedorXEstado(string estado)
        {

            List<Proveedor> proveedores = new();

            using (PORTALNEGOCIODataContext cx = new())
            {
                await Task.Run(() =>
                {
                    var lta = (from p in cx.PONEPROVEEDORs
                               where p.PROVESTADO == estado
                               select p).ToList();

                    foreach (var proveedor in lta)
                    {
                        bool personaNatural = proveedor.CLASTIPOPERSONAL1 == 1;
                        Proveedor lobj_proveedor = new();
                        List<Accionista> accionistas = new();
                        List<Especialidad> especialidades = new();
                        List<Documento> documentos = new();

                        //Accionistas
                        /* var ltaAccionistas = (from q in cx.PONEDETALLEACCIONISTAs
                                               where q.PROVPROVEEDOR == proveedor.PROVPROVEEDOR
                                               select q).ToList();*/
                        foreach (var item in proveedor.PONEDETALLEACCIONISTAs)
                        {
                            Accionista lobj_accionista = new()
                            {
                                IdentificacionAccionista = item.DEACIDENTIFICACION,
                                NombreAccionista = item.DEACNOMBRE,
                                TipoDocumentoAccionista = Convert.ToInt32(item.CLASTIPOIDENTIFICACION),
                                ParticipacionAccionista = Convert.ToInt32(item.DEACPORCENTAJE)
                            };
                            accionistas.Add(lobj_accionista);
                        }

                        //Especialidades
                        /*var ltaEspecialidades = (from q in cx.PONEESPECIALIDADPROVEEDORs
                                              where q.PROVPROVEEDOR == proveedor.PROVPROVEEDOR
                                              select q).ToList();
                        */
                        foreach (var item in proveedor.PONEESPECIALIDADPROVEEDORs)
                        {
                            Especialidad lobj_especialidad = new()
                            {
                                BienesServiciosEspecialidad = item.LIESBIENESOSERVICIOS,
                                ComercioEspecialidad = item.LIESCOMERCIO == "S",
                                ServiciosEspecialidad = item.LIESSERVICIOS == "S",
                                ManufacturaEspecialidad = item.LIESMANUFACTURA == "S",
                                GravadaEspecialidad = item.LIESGRAVADA == "S"
                            };
                            especialidades.Add(lobj_especialidad);
                        }

                        //Certificaciones
                        foreach (var item in proveedor.PONEDOCUMENTOs)
                        {
                            Documento lobj_documento = new()
                            {
                                CodigoDocumento = Convert.ToInt32(item.DOCUDOCUMENTO),
                                Tipo = Convert.ToInt32(item.CLASTIPODOCUMENTO8),
                                Nombre = item.DOCUNOMBRE
                            };
                            lobj_documento.Tipo = Convert.ToInt32(item.CLASTIPODOCUMENTO8);
                            //byte[] buffer = (from p in cx.PONEBLOBs
                             //                where p.BLOBBLOB == item.BLOBBLOB
                              //               select p.BLOBDATO).FirstOrDefault();
                            //lobj_documento.DataB64 = Convert.ToBase64String(buffer);
                            //lobj_documento.CodigoBlob = Convert.ToInt32(item.BLOBBLOB);
                            documentos.Add(lobj_documento);
                        }

                        lobj_proveedor.Accionistas = accionistas;
                        lobj_proveedor.Especialidades = especialidades;
                        lobj_proveedor.Certificaciones = documentos;
                        lobj_proveedor.Actividad = Convert.ToInt32(proveedor.PROVACTIVIDAD);
                        lobj_proveedor.ActividadEconomica = proveedor.ACECCODIGOACTIVIDAD;
                        lobj_proveedor.Activos = proveedor.PROVACTIVOS;
                        lobj_proveedor.Autorretenedor = proveedor.PROVAUTORRETENEDOR == Configuracion.ValorSI;
                        lobj_proveedor.Cargo = proveedor.PROVCARGO;
                        lobj_proveedor.Ciudad = Convert.ToInt32(proveedor.PROVCIUDAD);
                        lobj_proveedor.CiudadEmpresa = Convert.ToInt32(proveedor.PROVCIUDADEMPRESA);
                        lobj_proveedor.CiudadJuridica = Convert.ToInt32(proveedor.PROVCIUDAD);
                        lobj_proveedor.CiudadSucursal = Convert.ToInt32(proveedor.PROVCIUDADSUCURSAL);
                        lobj_proveedor.CodigoCIIU = proveedor.ACECCODIGOACTIVIDAD;
                        lobj_proveedor.CodigoProveedor = Convert.ToInt32(proveedor.PROVPROVEEDOR);
                        lobj_proveedor.ConceptoOtrosIngresos = proveedor.PROVCONCEPTOOTROSINGRESOS;
                        lobj_proveedor.ContribuyenteRenta = proveedor.PROVCONTRIBUYENTERENTA == Configuracion.ValorSI;
                        lobj_proveedor.Cuenta = proveedor.PROVNUMEROCUENTA;
                        lobj_proveedor.CuentasMonedaExtranjera = proveedor.PROVPRODFINANEXT == "S";
                        lobj_proveedor.DireccionComercial = proveedor.PROVDIRECCIONCOMERCIAL;
                        lobj_proveedor.DireccionJuridica = personaNatural ? null : proveedor.PROVDIRECCIONPRINCIAL;
                        lobj_proveedor.DireccionResidencia = personaNatural ? proveedor.PROVDIRECCIONPRINCIAL : proveedor.PROVDIRECCIONPRINCIAL;
                        lobj_proveedor.DireccionSucursal = proveedor.PROVDIRECCIONSUCURSAL;
                        lobj_proveedor.Documento = proveedor.PROVIDENTIFICACION;
                        lobj_proveedor.DocumentoRep = proveedor.PROVIDENTREPRESENTANTE;
                        lobj_proveedor.EgresosMensuales = proveedor.PROVEGRESOSMENSUALES;
                        lobj_proveedor.Email = proveedor.PROVEMAIL;
                        lobj_proveedor.Empresa = proveedor.PROVEMPRESATRABAJA;
                        lobj_proveedor.EntidadBancaria = Convert.ToInt32(proveedor.CLASENTIDADBANCARIA6);
                        lobj_proveedor.EntidadEstatal = proveedor.PROVENTIDADESTATAL == Configuracion.ValorSI;
                        lobj_proveedor.Fax = proveedor.PROVFAX;
                        lobj_proveedor.FechaExpedicionRep = Convert.ToDateTime(proveedor.PROVFECEXPIDENTREPRESENTANTE);
                        lobj_proveedor.FechaNacimiento = Convert.ToDateTime(proveedor.PROVFECHANACIMIENTO);
                        lobj_proveedor.FechaNacimientoRep = Convert.ToDateTime(proveedor.PROVFECHANACIMIENTOREP);
                        lobj_proveedor.GranContribuyente = proveedor.PROVGRANCONTRIBUYENTE == Configuracion.ValorSI;
                        lobj_proveedor.IdentificacionCuenta = proveedor.PROVIDENTITULARCUENTA;
                        lobj_proveedor.IngresosMensuales = proveedor.PROVINGRESOSMENSUALES;
                        lobj_proveedor.LugarExpedicionRep = Convert.ToInt32(proveedor.PROVLUGAREXPIDENTREPRESENT);
                        lobj_proveedor.LugarNacimiento = Convert.ToInt32(proveedor.PROVLUGARNACIMIENTO);
                        lobj_proveedor.LugarNacimientoRep = Convert.ToInt32(proveedor.PROVLUGARNACIMIENTOREP);
                        lobj_proveedor.ManejaRecursos = proveedor.PROVMANEJARECURSOPUBLICOS == "S";
                        lobj_proveedor.MonedaExtranjera = proveedor.PROVTRANSMONEDAEXT == "S";
                        lobj_proveedor.NacionalidadRep = Convert.ToInt32(proveedor.PROVNACIONALIDADREPRESENTANTE);
                        lobj_proveedor.Nit = personaNatural ? null : proveedor.PROVIDENTIFICACION;
                        lobj_proveedor.Nombre = proveedor.PROVRAZONSOCIAL;
                        lobj_proveedor.NombreJuridica = proveedor.PROVRAZONSOCIAL;
                        lobj_proveedor.NombreRep = proveedor.PROVREPRESENTANTELEGAL;
                        //lobj_proveedor.NoResponsableIVA = proveedor.PROVNORESPONSABLEIVA;
                        lobj_proveedor.OtrosIngresos = proveedor.PROVOTROSINGRESOS;
                        lobj_proveedor.Pasivos = proveedor.PROVPASIVOS;
                        lobj_proveedor.Patrimonio = proveedor.PROVPATRIMONIO;
                        lobj_proveedor.PoderPublico = proveedor.PROVPODERPUBLICO == "S";
                        lobj_proveedor.Profesion = proveedor.PROVPROFESION;
                        lobj_proveedor.ReconocimientoPublico = proveedor.PROVRECONOCIMIENTOPUBLICO == "S";
                        lobj_proveedor.ResponsableIVA = proveedor.PROVRESPONSABLEIVA == Configuracion.ValorSI;
                        //lobj_proveedor.ResponsableOtros = proveedor.PROVACTICOMERCIALOTROSMUNI == "S" ? true : false;
                        //lobj_proveedor.ActEcoOtros = proveedor.CLASACTOTROS14;
                        //lobj_proveedor.CodigoCIIUOtros = proveedor.CLASACTOTROS14;
                        lobj_proveedor.ResponsableIndyComer = proveedor.PROVINDUSTRIAYCOMERCIO == Configuracion.ValorSI;
                        lobj_proveedor.ActEcoPereira = proveedor.CLASACTPEREIRA14;
                        lobj_proveedor.CodigoCIIUIndustriaPereira = proveedor.CLASACTPEREIRA14;
                        lobj_proveedor.RespuestaAfirmativa = proveedor.PROVOBSERVACION;
                        lobj_proveedor.AgenteRetenedorIVA = proveedor.PROVAGENTERETENEDORIVA == Configuracion.ValorSI;
                        lobj_proveedor.SectorEconomia = Convert.ToInt32(proveedor.CLASSECTORECONOMICO14);
                        lobj_proveedor.Sucursal = proveedor.PROVSUCURSALBANCO;
                        lobj_proveedor.Telefono = Convert.ToInt64(proveedor.PROVTELEFONO);
                        lobj_proveedor.TelefonoEmpresa = proveedor.PROVTELEFONOEMPRESA;
                        lobj_proveedor.TelefonoPrincipal = !personaNatural ? proveedor.PROVTELEFONO : (long?)null;
                        lobj_proveedor.TelefonoSucursal = !personaNatural ? proveedor.PROVTELEFONOSUCURSAL : null;
                        lobj_proveedor.TipoCuenta = Convert.ToInt32(proveedor.CLASTIPOCUENTA7);
                        lobj_proveedor.TipoDocumento = Convert.ToInt32(proveedor.CLASTIPOIDENTIFICACION2);
                        lobj_proveedor.TipoDocumentoRep = Convert.ToInt32(proveedor.CLASTIPOIDENTREPRES2);
                        lobj_proveedor.TipoEmpresa = Convert.ToInt32(proveedor.CLASTIPOEMPRESA13);
                        lobj_proveedor.TipoMonedaExtranjera = Convert.ToInt32(proveedor.CLASTIPOMONEDA5);
                        lobj_proveedor.TipoPersona = Convert.ToInt32(proveedor.CLASTIPOPERSONAL1);
                        lobj_proveedor.TitularCuenta = proveedor.PROVTITULARCUENTA;
                        lobj_proveedor.LugarExpedicion = Convert.ToInt32(proveedor.PROVLUGAREXPEDICION);
                        lobj_proveedor.ClasificacionTamano = proveedor.PROVCLASTAMANO;
                        lobj_proveedor.ClasificacionSector = proveedor.PROVCLASSECTOR;
                        lobj_proveedor.OperacionesMercantiles = proveedor.PROVOPERMERCANTILES == "S";

                        proveedores.Add(lobj_proveedor);
                    }
                });

                return proveedores;
            }
        }

        /// <summary>
        /// Actualiza el estado del proveedor y del usuario
        /// </summary>
        /// <param name="estadoProveedor">estado actual del proveedor</param>
        /// <returns>OK si todo esta bien o el mensaje de error</returns>
        public async Task<string> ActualizarEstado(ActualizarEstadoProveedor estadoProveedor)
        {

            using (PORTALNEGOCIODataContext cx = new())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var tblProveedor = (from p in cx.PONEPROVEEDORs
                                            where p.PROVPROVEEDOR == estadoProveedor.CodigoProveedor
                                            select p).FirstOrDefault();

                        var tblUsuario = (from u in cx.POGEUSUARIOs
                                            where u.USUAIDENTIFICADOR == tblProveedor.PROVIDENTIFICACION
                                            select u).FirstOrDefault();

                        //string claveGenerada = cx.GENERARCLAVEALEATORIA();
                        //string claveEncriptada = cx.FENCRIPTAR(claveGenerada);
                        string claveGenerada = _utilidades.GetRandomKey();
                        string claveEncriptada = _utilidades.Encriptar(claveGenerada, _configuration.GetSection("EncryptedKey").Value);

                        tblUsuario.USUAESTADO = estadoProveedor.Estado;
                        tblUsuario.USUACLAVE = claveEncriptada;
                        tblUsuario.USUACAMBIARCLAVE = Configuracion.ValorSI;                        
                        tblProveedor.PROVESTADO = estadoProveedor.Estado;
                        tblProveedor.PROVUSUARIOAUTORIZO = estadoProveedor.UsuarioAutoriza;
                        tblProveedor.PROVFECHAAUTORIZACION = DateTime.Now;

                        await InsertarAnexo(estadoProveedor.DocumentoRevision, cx, (int)tblProveedor.PROVPROVEEDOR, estadoProveedor.UsuarioAutoriza);

                        cx.SubmitChanges();

                        dbContextTransaction.Commit();

                        //////Envia correo notificando al usuario el registro
                        ///
                        Usuario usr = new()
                        {
                            Identificador = tblUsuario.USUAIDENTIFICADOR,
                            Nombres = tblUsuario.USUANOMBRE,
                            IdProveedor = (int)tblProveedor.PROVPROVEEDOR,
                            Clave = claveGenerada
                        };
                        if (estadoProveedor.Estado == Configuracion.EstadoActivo)
                        {
                            (new NotificacionBusiness(_utilidades)).GenerarNotificacion(Configuracion.NotificacionProvAutorizado, usr);
                        }
                        else
                        {
                            (new NotificacionBusiness(_utilidades)).GenerarNotificacion(Configuracion.NotificacionProvRechazado, usr);
                        }                       

                        return "OK";
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return ex.Message;
                    }
                }
                
            }
        }

        /// <summary>
        /// Consulta los datos basicos del proveedor
        /// </summary>
        /// <returns></returns>
        public List<ProveedorDatosBasicos> ConsultarDatosBasicosProveedor()
        {
            List<ProveedorDatosBasicos> prov = new();

            using (PORTALNEGOCIODataContext cx = new())
            {
                /*prov = (from p in cx.PONEPROVEEDORs
                        select new ProveedorDatosBasicos { CodigoProveedor = (int)p.PROVPROVEEDOR, Documento = p.PROVIDENTIFICACION, TipoPersona = p.CLASTIPOPERSONAL1, Nombre = p.PROVRAZONSOCIAL }).ToList();*/

                prov = [.. (from p in cx.PONEPROVEEDORs
                        join t in cx.PONEVTIPOPERSONAs on p.CLASTIPOPERSONAL1 equals t.CLVACLASEVALOR                        
                        //join c in cx.POGEMUNICIPIOs on p.PROVCIUDAD equals c.MUNICODIGO
                        select new ProveedorDatosBasicos
                        {
                            CodigoProveedor = (int)p.PROVPROVEEDOR,
                            Nombre = p.PROVRAZONSOCIAL,
                            TipoPersona = t.CLVAVALOR,
                            TipoIdentificacion = (from ti in cx.PONEVTIPOIDENTIFICACIONs
                                                     where ti.CLVACLASEVALOR == p.CLASTIPOIDENTIFICACION2
                                                     select ti.CLVAVALOR).FirstOrDefault(),
                            Documento = p.PROVIDENTIFICACION,
                            Ciudad = cx.POGEMUNICIPIOs.Where(m => m.MUNICODIGO == p.PROVCIUDAD).Select(m => m.MUNINOMBRE).SingleOrDefault(),  //c.MUNINOMBRE,
                            Direccion = p.PROVDIRECCIONPRINCIAL,
                            Telefono = Convert.ToString(p.PROVTELEFONO),
                            Correo = p.PROVEMAIL,
                            Estado =
                            (
                                p.PROVESTADO == Configuracion.EstadoActivo ? "Activo" :
                                p.PROVESTADO == Configuracion.EstadoInactivo ? "Inactivo" :
                                p.PROVESTADO == Configuracion.EstadoPendiente ? "Pendiente" :
                                p.PROVESTADO == Configuracion.EstadoRechazado ? "Rechazado" : "Indeterminado"
                            )
                        })];

            }

            return prov;
        }

        public static string ObtenerEmailxProveedor(int idProveedor)
        {
            using (PORTALNEGOCIODataContext cx = new())
            {
                string correo = (from p in cx.PONEPROVEEDORs
                                 where p.PROVPROVEEDOR == idProveedor
                                 select p.PROVEMAIL).SingleOrDefault();
                return correo;
            }
        }

        public Proveedor ObtenerProveedor(int idProveedor)
        {
            using (PORTALNEGOCIODataContext cx = new())
            {
                DataLoadOptions dlo = new();
                dlo.LoadWith<PONEPROVEEDOR>(a => a.PONEDETALLEACCIONISTAs);
                dlo.LoadWith<PONEPROVEEDOR>(e => e.PONEESPECIALIDADPROVEEDORs);
                dlo.LoadWith<PONEPROVEEDOR>(d => d.PONEDOCUMENTOs);
                dlo.LoadWith<PONEPROVEEDOR>(a => a.POGEACTIVIDADECONOMICA);

                cx.LoadOptions = dlo;

                var prov = cx.PONEPROVEEDORs.Where(p => p.PROVPROVEEDOR == idProveedor).SingleOrDefault();

                var prov1 = Proveedor.CrearProveedor(prov);
                prov1.Accionistas = Accionista.CrearAccionista([.. prov.PONEDETALLEACCIONISTAs]);
                prov1.Certificaciones = Documento.CrearDocumentos([.. prov.PONEDOCUMENTOs]);

                /*if (prov1.Certificaciones.Count > 0)
                {
                    List<Documento> lta = new();
                    foreach (Documento item in prov1.Certificaciones)
                    {
                        item.DataB64 = _utilidades.ObtenerBlob(item.CodigoBlob,cx);
                        lta.Add(item);
                    }

                    prov1.Certificaciones = lta;
                }*/

                prov1.Especialidades = Especialidad.CrearEspecialidad([.. prov.PONEESPECIALIDADPROVEEDORs]);

                return prov1;

            }
        }

        public async Task<ProveedorFormato> ObtenerProveedorFormato(int idProveedor)
        {
            using (PORTALNEGOCIODataContext cx = new())
            {
                return await
                    Task.Run(() =>
                {
                    ProveedorFormato provFormato = new();

                    /*DataLoadOptions dlo = new DataLoadOptions();
                    dlo.LoadWith<PONEPROVEEDOR>(a => a.PONEDETALLEACCIONISTAs);
                    dlo.LoadWith<PONEPROVEEDOR>(e => e.PONEESPECIALIDADPROVEEDORs);
                    dlo.LoadWith<PONEPROVEEDOR>(d => d.PONEDOCUMENTOs);

                    cx.LoadOptions = dlo;*/

                    //var prov = cx.PONEPROVEEDORs.Where(p => p.PROVPROVEEDOR == idProveedor).SingleOrDefault();
                    var resProv = cx.FOBTENERPROVEEDOR(idProveedor);

                    foreach (var item in resProv)
                    {
                        provFormato = ProveedorFormato.CrearProveedorFormato(item);
                    }

                    var ltaEspecialidad = (from e in cx.PONEESPECIALIDADPROVEEDORs
                                           where e.PROVPROVEEDOR == idProveedor
                                           select new EspecialidadFormato
                                           {
                                               ComercioEspecialidad = e.LIESCOMERCIO,
                                               ServiciosEspecialidad = e.LIESSERVICIOS,
                                               ManufacturaEspecialidad = e.LIESMANUFACTURA,
                                               GravadaEspecialidad = e.LIESGRAVADA,
                                               BienesOServicios = e.LIESBIENESOSERVICIOS

                                           }).ToList();

                    provFormato.Especialidades = ltaEspecialidad;

                    var ltaAccionistas = (from a in cx.PONEDETALLEACCIONISTAs
                                          join t in cx.POGECLASEVALORs on a.CLASTIPOIDENTIFICACION equals t.CLVACLASEVALOR
                                          where a.PROVPROVEEDOR == idProveedor
                                          select new AccionistaFormato
                                          {
                                              NombreAccionista = a.DEACNOMBRE,
                                              TipoDocumentoAccionista = t.CLVAVALOR,
                                              IdentificacionAccionista = a.DEACIDENTIFICACION,
                                              ParticipacionAccionista = a.DEACPORCENTAJE
                                          }).ToList();

                    provFormato.Accionistas = ltaAccionistas;

                    return provFormato;
                });
            }
        }

        /// <summary>
        /// Crea objecto a partir del json y completa con los datos de lista para exportar al momento de registro de proveedor
        /// </summary>
        /// <param name="proveedor"></param>
        /// <returns></returns>
        public async Task<ProveedorFormato> ObtenerProveedorFormatoJson(Proveedor proveedor)
        {
            using (PORTALNEGOCIODataContext cx = new())
            {
                return await Task.Run(async () =>
                {
                    ProveedorFormato provFormato = new();

                    bool personaNatural = proveedor.TipoPersona == 1;

                    Municipio lugarNacimiento = await _utilidades.ObtenerMunicipio(proveedor.LugarNacimiento != null ? (int)proveedor.LugarNacimiento : -1);
                    Municipio lugarExpedicion = await _utilidades.ObtenerMunicipio(proveedor.LugarExpedicion != null ? (int)proveedor.LugarExpedicion : -1);
                    Municipio ciudadEmpresa = await _utilidades.ObtenerMunicipio(proveedor.CiudadEmpresa != null ? (int)proveedor.CiudadEmpresa : -1);
                    Municipio ciudad = await _utilidades.ObtenerMunicipio(proveedor.Ciudad != null ? (int)proveedor.Ciudad : -1);

                    Municipio ciudadJuridica = new();
                    Municipio ciudadSucursal = new();
                    Municipio lugarExpRep = new();
                    Municipio lugarNacRep = new();

                    if (!personaNatural)
                    {
                        ciudadJuridica = await _utilidades.ObtenerMunicipio(proveedor.CiudadJuridica != null ? (int)proveedor.CiudadJuridica : -1);
                        ciudadSucursal = await _utilidades.ObtenerMunicipio(proveedor.CiudadSucursal != null ? (int)proveedor.CiudadSucursal : -1);
                        lugarExpRep = await _utilidades.ObtenerMunicipio(proveedor.LugarExpedicionRep != null ? (int)proveedor.LugarExpedicionRep : -1);
                        lugarNacRep = await _utilidades.ObtenerMunicipio(proveedor.LugarNacimientoRep != null ? (int)proveedor.LugarNacimientoRep : -1);
                    }

                    ActividadEconomica actividadEconomica = await _utilidades.ObtenerActividadEconomica(proveedor.CodigoCIIU);
                    ActividadEconomica actividadIndComPereira = await _utilidades.ObtenerActividadEconomica(proveedor.ActEcoPereira != null ? proveedor.ActEcoPereira.ToString() : "null");
                    ActividadEconomica actividadIndComOtros = await _utilidades.ObtenerActividadEconomica(proveedor.ActEcoOtros != null ? proveedor.ActEcoOtros.ToString() : "null");

                    provFormato.TipoPersona = proveedor.TipoPersona;
                    provFormato.Nombre = personaNatural ? proveedor.Nombre : proveedor.NombreJuridica;
                    provFormato.TipoDocumento = proveedor.TipoDocumento;
                    provFormato.EntidadBancaria = _utilidades.ObtenerValorClaseValor(Convert.ToInt32(proveedor.EntidadBancaria));
                    provFormato.TelefonoEmpresa = proveedor.TelefonoEmpresa;
                    provFormato.ManejaRecursos = proveedor.ManejaRecursos;
                    provFormato.ReconocimientoPublico = proveedor.ReconocimientoPublico;
                    provFormato.PoderPublico = proveedor.PoderPublico;
                    provFormato.RespuestaAfirmativa = proveedor.RespuestaAfirmativa;
                    //provFormato.Nombre = proveedor.Nombre;
                    provFormato.TipoDocumento = proveedor.TipoDocumento;
                    provFormato.Documento = personaNatural ? proveedor.Documento : proveedor.Nit;
                    provFormato.LugarExpedicion = lugarExpedicion?.Nombre;
                    provFormato.FechaNacimiento = proveedor.FechaNacimiento;
                    provFormato.LugarNacimiento = lugarNacimiento?.Nombre;
                    provFormato.DireccionResidencia = proveedor.DireccionResidencia;
                    provFormato.Telefono = proveedor.Telefono;
                    provFormato.Ciudad = ciudad?.Nombre;
                    provFormato.Email = proveedor.Email;
                    provFormato.Actividad = proveedor.Actividad;
                    provFormato.Profesion = proveedor.Profesion;
                    provFormato.Empresa = proveedor.Empresa;
                    provFormato.Cargo = proveedor.Cargo;
                    provFormato.DireccionComercial = proveedor.DireccionComercial;
                    provFormato.CiudadEmpresa = ciudadEmpresa?.Nombre;
                    provFormato.Fax = proveedor.Fax;
                    provFormato.ActividadEconomica = actividadEconomica?.Nombre;
                    provFormato.CodigoCIIU = proveedor.CodigoCIIU;
                    provFormato.IngresosMensuales = proveedor.IngresosMensuales;
                    provFormato.EgresosMensuales = proveedor.EgresosMensuales;
                    provFormato.Activos = proveedor.Activos;
                    provFormato.Pasivos = proveedor.Pasivos;
                    provFormato.OtrosIngresos = proveedor.OtrosIngresos;
                    provFormato.Patrimonio = proveedor.Patrimonio;

                    //Persona juridica
                    provFormato.DireccionJuridica = proveedor.DireccionJuridica;
                    provFormato.CiudadJuridica = ciudadJuridica?.Nombre;
                    provFormato.TelefonoPrincipal = proveedor.TelefonoPrincipal?.ToString();
                    provFormato.DireccionSucursal = proveedor.DireccionSucursal;
                    provFormato.CiudadSucursal = ciudadSucursal?.Nombre;
                    provFormato.TelefonoSucursal = proveedor.TelefonoSucursal;
                    provFormato.TipoEmpresa = proveedor.TipoEmpresa != null ? _utilidades.ObtenerValorClaseValor(Convert.ToInt32(proveedor.TipoEmpresa)) : " ";
                    provFormato.SectorEconomia = proveedor.SectorEconomia != null ? _utilidades.ObtenerValorClaseValor(Convert.ToInt32(proveedor.SectorEconomia)) : " ";
                    provFormato.NombreRep = proveedor.NombreRep;
                    provFormato.TipoDocumentoRep = _utilidades.ObtenerValorClaseValor(Convert.ToInt32(proveedor.TipoDocumentoRep));
                    provFormato.DocumentoRep = proveedor.DocumentoRep;
                    provFormato.FechaExpedicionRep = proveedor.FechaExpedicionRep;
                    provFormato.LugarExpedicionRep = lugarExpRep?.Nombre;
                    provFormato.FechaNacimientoRep = proveedor.FechaNacimientoRep;
                    provFormato.LugarNacimientoRep = lugarNacRep?.Nombre;
                    provFormato.NacionalidadRep = _utilidades.ObtenerValorClaseValor(Convert.ToInt32(proveedor.NacionalidadRep));

                    //provFormato.ResponsablePereira = proveedor.ActEcoPereira?.ToString();
                    //provFormato.ResponsableOtros = proveedor.ActEcoOtros?.ToString();
                    provFormato.ConceptoOtrosIngresos = proveedor.ConceptoOtrosIngresos;
                    provFormato.MonedaExtranjera = proveedor.MonedaExtranjera;
                    provFormato.TipoMoneda = proveedor.TipoMonedaExtranjera != null ? _utilidades.ObtenerValorClaseValor((int)proveedor.TipoMonedaExtranjera) : "";
                    provFormato.CuentasMonedaExtranjera = proveedor.CuentasMonedaExtranjera;

                    //provFormato.EntidadEstatalB          = proveedor.  EntidadEstatalB;
                    //provFormato.GranContribuyenteB       = proveedor.GranContribuyenteB;
                    //provFormato.ResponsableIVAB          = proveedor.ResponsableIVAB;
                    //provFormato.NoResponsableIVAB        = proveedor.NoResponsableIVAB;
                    // provFormato.AutorretenedorB          = proveedor.AutorretenedorB;
                    //provFormato.ContribuyenteB           = proveedor.ContribuyenteB;
                    //provFormato.RetenedorIVAB            = proveedor.RetenedorIVAB;
                    provFormato.EntidadEstatal = proveedor.EntidadEstatal;
                    provFormato.EntidadSinLucro = proveedor.EntidadSinLucro;
                    provFormato.ResolEntidadSinLucro = proveedor.ResolEntidadSinLucro;
                    provFormato.GranContribuyente = proveedor.GranContribuyente;
                    provFormato.ResolGranContribuyente = proveedor.ResolGranContribuyente;
                    provFormato.ResponsableIVA = proveedor.ResponsableIVA;                    
                    provFormato.Autorretenedor = proveedor.Autorretenedor;
                    provFormato.ResolAutorretenedor = proveedor.ResolAutorretenedor;
                    provFormato.ContribuyenteRenta = proveedor.ContribuyenteRenta;
                    provFormato.AgenteRetenedorIVA = proveedor.AgenteRetenedorIVA;
                    provFormato.ResolAgenteRetenedorIVA = proveedor.ResolAgenteRetenedorIVA;
                    provFormato.ResponsableIndyComer = proveedor.ResponsableIndyComer;                    
                    provFormato.EntidadBancaria = _utilidades.ObtenerValorClaseValor(proveedor.EntidadBancaria != null ? Convert.ToInt32(proveedor.EntidadBancaria) : -1);
                    provFormato.Sucursal = proveedor.Sucursal;
                    provFormato.Cuenta = proveedor.Cuenta;
                    provFormato.TipoCuenta = _utilidades.ObtenerValorClaseValor(proveedor.TipoCuenta != null ? Convert.ToInt32(proveedor.TipoCuenta) : -1);
                    provFormato.TitularCuenta = proveedor.TitularCuenta;
                    provFormato.IdentificacionCuenta = proveedor.IdentificacionCuenta;
                    provFormato.ClasificacionSector = proveedor.ClasificacionSector;
                    provFormato.ClasificacionTamano = proveedor.ClasificacionTamano;
                    provFormato.OperacionesMercantiles = proveedor.OperacionesMercantiles;
                    
                    //provFormato.accionistas              = proveedor.accionistas;
                    //provFormato.especialidades = proveedor.especialidades;

                    List<EspecialidadFormato> ltaEspecialidades = new();

                    if (proveedor.Especialidades != null && proveedor.Especialidades.Count > 0)
                    {
                        proveedor.Especialidades.ForEach(i =>
                        {
                            ltaEspecialidades.Add(new EspecialidadFormato
                            {
                                BienesOServicios = i.BienesServiciosEspecialidad,
                                ComercioEspecialidad = i.ComercioEspecialidad ? "S" : "N",
                                ServiciosEspecialidad = i.ServiciosEspecialidad ? "S" : "N",
                                ManufacturaEspecialidad = i.ManufacturaEspecialidad ? "S" : "N",
                                GravadaEspecialidad = i.GravadaEspecialidad ? "S" : "N",                                
                                ItemEspecialidad = i.ItemEspecialidad                                
                            });
                        });
                    }

                    provFormato.Especialidades = ltaEspecialidades;
                    

                    List<AccionistaFormato> ltaAccionistas = new();

                    if(proveedor.Accionistas != null && proveedor.Accionistas.Count > 0)
                    {
                        proveedor.Accionistas.ForEach(i =>
                        {
                            ltaAccionistas.Add(new AccionistaFormato
                            {
                                NombreAccionista = i.NombreAccionista,
                                IdentificacionAccionista = i.IdentificacionAccionista,
                                TipoDocumentoAccionista = _utilidades.ObtenerValorClaseValor((int)i.TipoDocumentoAccionista),
                                ParticipacionAccionista = i.ParticipacionAccionista
                            });
                        });
                    }

                    provFormato.Accionistas = ltaAccionistas;

                    return provFormato;
                });
            }
        }

        public async Task<List<ProveedorEstado>> ObtenerCantidadProveedorPorEstado()
        {
            using (PORTALNEGOCIODataContext cx = new())
            {
                var query = from p in cx.PONEPROVEEDORs
                            group p by p.PROVESTADO into g
                            select new ProveedorEstado
                            {
                                Cantidad = g.Count(),
                                Estado = (g.Key == "A") ? "Autorizado" : (g.Key == "P") ? "Pendiente Autorización" : "Inactivo"
                            };

                return await Task.FromResult(query.ToList());
            }
        }

        public async Task<List<FPROVEEDORESREGISTRADOSMEResult>> ObtenerNroProveedoresRegistradoPorMes(int vigencia)
        {
            using (PORTALNEGOCIODataContext cx = new())
            {
                var query = from p in cx.FPROVEEDORESREGISTRADOSME(vigencia) select p;

                return await Task.FromResult(query.ToList());                            
            }
        }

        

        #endregion

        #region Metodos Privados
        /// <summary>/// Almacena los accionistas de la empresa cuando es persona juridica
        /// </summary>
        /// <param name="Accionistas">Lista de Accionista</param>
        /// <param name="cx">Contexto de BD</param>
        /// <param name="codigoProveedor">Codigo del proveedor</param>
        private void CargarAccionista(List<Accionista> Accionistas, PORTALNEGOCIODataContext cx, int codigoProveedor, int? logsUsuario)
        {
            foreach (var item in Accionistas)
            {
                if(item.IdentificacionAccionista != null)
                {
                    var tblAccionistas = new PONEDETALLEACCIONISTA
                    {
                        DEACSECUENCIA = _utilidades.GetSecuencia("SECU_PONEACCIONISTA", cx),
                        DEACNOMBRE = item.NombreAccionista,
                        DEACIDENTIFICACION = item.IdentificacionAccionista,
                        CLASTIPOIDENTIFICACION = item.TipoDocumentoAccionista,
                        DEACPORCENTAJE = item.ParticipacionAccionista,
                        PROVPROVEEDOR = codigoProveedor,
                        LOGSUSUARIO = logsUsuario
                    };
                    cx.PONEDETALLEACCIONISTAs.InsertOnSubmit(tblAccionistas);
                }                
            }

            cx.SubmitChanges();
        }

        /// <summary>
        /// Almacena las especialidades del proveedor
        /// </summary>
        /// <param name="Especialidades">Lista de especialidades</param>
        /// <param name="cx">Contexto de BD</param>
        /// <param name="codigoProveedor">Codigo del proveedor</param>
        /// <returns></returns>
        private void CargarEspecialidades(List<Especialidad> Especialidades, PORTALNEGOCIODataContext cx, int codigoProveedor, int? logsUsuario)
        {
            foreach (var item in Especialidades)
            {
                if(item.BienesServiciosEspecialidad != null)
                {
                    var tblEspecialidades = new PONEESPECIALIDADPROVEEDOR
                    {
                        LIESSECUENCIA = _utilidades.GetSecuencia("SECU_PONEESPECIALIDAD", cx),
                        PROVPROVEEDOR = codigoProveedor,
                        LIESCOMERCIO = item.ComercioEspecialidad ? "S" : "N",
                        LIESSERVICIOS = item.ServiciosEspecialidad ? "S" : "N",
                        LIESMANUFACTURA = item.ManufacturaEspecialidad ? "S" : "N",
                        LIESGRAVADA = item.GravadaEspecialidad ? "S" : "N",
                        LIESBIENESOSERVICIOS = item.BienesServiciosEspecialidad,
                        LOGSUSUARIO = logsUsuario
                    };

                    cx.PONEESPECIALIDADPROVEEDORs.InsertOnSubmit(tblEspecialidades);
                }                
            }

            cx.SubmitChanges();
        }

        private void ActualizarEspecialidades(List<Especialidad> Especialidades, PORTALNEGOCIODataContext cx, int codigoProveedor, int? logsUsuario)
        {
            foreach (var item in Especialidades)
            {
                if (item.BienesServiciosEspecialidad != null)
                {
                    var itemEspecialidad = cx.PONEESPECIALIDADPROVEEDORs.Where(e => e.LIESSECUENCIA == item.SecuenciaEspecialidad).SingleOrDefault();

                    if(itemEspecialidad != null)
                    {
                        if (item.EstadoRegistro == "D") //Indica que se debe eliminar el registro para esto actualizamos el usuario para auditoria
                        {
                            itemEspecialidad.LOGSUSUARIO = logsUsuario;
                            cx.SubmitChanges();

                            cx.PONEESPECIALIDADPROVEEDORs.DeleteOnSubmit(itemEspecialidad);
                            cx.SubmitChanges();
                        }
                        else //Existe el registro y debe actualizar.
                        {
                            itemEspecialidad.LIESCOMERCIO = item.ComercioEspecialidad ? "S" : "N";
                            itemEspecialidad.LIESSERVICIOS = item.ServiciosEspecialidad ? "S" : "N";
                            itemEspecialidad.LIESMANUFACTURA = item.ManufacturaEspecialidad ? "S" : "N";
                            itemEspecialidad.LIESGRAVADA = item.GravadaEspecialidad ? "S" : "N";
                            itemEspecialidad.LIESBIENESOSERVICIOS = item.BienesServiciosEspecialidad;
                            itemEspecialidad.LOGSUSUARIO = logsUsuario;
                            cx.SubmitChanges();
                        }
                    }
                    else     //  Si no existe es porque se agrego               
                    {
                        var tblEspecialidades = new PONEESPECIALIDADPROVEEDOR
                        {
                            LIESSECUENCIA = _utilidades.GetSecuencia("SECU_PONEESPECIALIDAD", cx),
                            PROVPROVEEDOR = codigoProveedor,
                            LIESCOMERCIO = item.ComercioEspecialidad ? "S" : "N",
                            LIESSERVICIOS = item.ServiciosEspecialidad ? "S" : "N",
                            LIESMANUFACTURA = item.ManufacturaEspecialidad ? "S" : "N",
                            LIESGRAVADA = item.GravadaEspecialidad ? "S" : "N",
                            LIESBIENESOSERVICIOS = item.BienesServiciosEspecialidad,
                            LOGSUSUARIO = logsUsuario
                        };
                        cx.PONEESPECIALIDADPROVEEDORs.InsertOnSubmit(tblEspecialidades);
                        cx.SubmitChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Carga los documentos anexos del proveedor
        /// </summary>
        /// <param name="Certificaciones">Lista de Documentos</param>
        /// <param name="cx">Contexto de BD</param>
        /// <param name="codigoProveedor">Codigo del Proveedor</param>
        private async Task CargarAnexos(List<Documento> Certificaciones, PORTALNEGOCIODataContext cx, int codigoProveedor, int? idUsuario)
        {
            foreach (var item in Certificaciones)
            {
                await InsertarAnexo(item, cx, codigoProveedor, idUsuario);
            }

            cx.SubmitChanges();
        }
        
        private async Task InsertarAnexo(Documento certificacion, PORTALNEGOCIODataContext cx, int codigoProveedor, int? idUsuario, bool historico = false)
        {
            // Inserta el blob
            /*var tblBlobs = new PONEBLOB
            {
                BLOBDATO = _utilidades.DecodificarArchivo(certificacion.DataB64)
            };
            int codigoBlob = _utilidades.GetSecuencia("SECU_PONEBLOB", cx);
            tblBlobs.BLOBBLOB = codigoBlob;
            cx.PONEBLOBs.InsertOnSubmit(tblBlobs);
            cx.SubmitChanges();*/
            // _utilidades.DecodificarArchivo(certificacion.DataB64)
            // Inserta el documento

            var folder = $"proveedores\\{codigoProveedor.ToString()}\\{DateTime.Now.Year}";

            var (contentType, data) = _utilidades.DecodificarArchivoContentType(certificacion.DataB64);
            using var fileStream = new MemoryStream(data);
            var rutaCompleta = await _storageService.SaveFileAsync(folder, certificacion.Nombre, fileStream, contentType);

            var tblDocumento = new PONEDOCUMENTO
            {
                DOCUDOCUMENTO = _utilidades.GetSecuencia("SECU_PONEDOCUMENTO", cx),          
                DOCUFECHACREACION = DateTime.Now,
                DOCUNOMBRE = certificacion.Nombre,
                LOGSFECHA = DateTime.Now,
                LOGSUSUARIO = (decimal)idUsuario,
                CLASTIPODOCUMENTO8 = certificacion.Tipo,
                DOCURUTA = rutaCompleta,
                DOCUCONTENTTYPE = contentType
            };
            cx.PONEDOCUMENTOs.InsertOnSubmit(tblDocumento);
            cx.SubmitChanges();

            if(!historico)
                cx.PONEPROVEEDORs.FirstOrDefault(X => X.PROVPROVEEDOR == codigoProveedor).PONEDOCUMENTOs.Add(tblDocumento);
            
        }

        /// <summary>
        /// Actualiza los documentos anexos del proveedor
        /// </summary>
        /// <param name="Certificaciones">Lista de Documentos</param>
        /// <param name="cx">Contexto de BD</param>
        /// <param name="codigoProveedor">Codigo del Proveedor</param>
        private void ActualizarDocumentosProveedor(List<Documento> certificaciones, PORTALNEGOCIODataContext cx, int codigoProveedor, int? IdUsuario)
        {
            // Por cada documento que llega valida si ya existe, si no existe pone en estado I al del mismo tipo y e inserta el nuevo documento
            foreach (var item in certificaciones)
            {
                //Si no tiene codigo de Blob inserta un nuevo registro, elimina el anterior y lo copia a la tabla de historicos
                //if(item.CodigoBlob == 0)
                {                    
                    var query = cx.PONEPROVEEDORs.Where(x => x.PROVPROVEEDOR == codigoProveedor).FirstOrDefault();
                    var documentoEliminar = query.PONEDOCUMENTOs.Where(x => x.CLASTIPODOCUMENTO8 == item.Tipo).FirstOrDefault();

                    if(documentoEliminar != null)
                    {
                        PONEHISTDOCPROV tblhistorico = new()
                        {
                            DOCUDOCUMENTO = documentoEliminar.DOCUDOCUMENTO,
                            PROVPROVEEDOR = codigoProveedor,
                            HIDOFECHA = DateTime.Now,
                            LOGSUSUARIO = IdUsuario,
                            HIDOHIDO = _utilidades.GetSecuencia("SECU_PONEHISTDOCPROV", cx)
                        };

                        cx.PONEHISTDOCPROVs.InsertOnSubmit(tblhistorico);
                        //Elimina el documento existente
                        cx.PONEPROVEEDORs.FirstOrDefault(X => X.PROVPROVEEDOR == codigoProveedor).PONEDOCUMENTOs.Remove(documentoEliminar);
                        // Inserta el antiguo documento
                        Documento docAntiguo = new()
                        {
                            //CodigoBlob = (int)documentoEliminar.BLOBBLOB,
                            CodigoDocumento = (int)documentoEliminar.DOCUDOCUMENTO,
                            //DataB64 = _utilidades.ObtenerBlob((int)documentoEliminar.BLOBBLOB, cx),
                            FechaCreacion = documentoEliminar.DOCUFECHACREACION,
                            Nombre = documentoEliminar.DOCUNOMBRE,
                            Tipo = (int)documentoEliminar.CLASTIPODOCUMENTO8,
                            IdUsuario = (int)IdUsuario
                        };

                        InsertarAnexo(docAntiguo,
                            cx,
                            codigoProveedor,
                            IdUsuario,
                            true);
                    }
                    
                    // Inserta el nuevo documento
                    InsertarAnexo(item, cx, codigoProveedor, IdUsuario);

                }
                
            }
        }

        #endregion
    }

}
