using Negocio.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Negocio.Model
{
    public class Proveedor
    {
        public static Expression<Func<PONEPROVEEDOR, Proveedor>> FromProveedor
        {
            get
            {
                return p => new Proveedor
                {
                    CodigoProveedor = (int)p.PROVPROVEEDOR,
                    NombreJuridica = p.PROVRAZONSOCIAL,
                    TipoPersona = (int)p.CLASTIPOPERSONAL1,
                    TipoDocumento = (int?)p.CLASTIPOIDENTIFICACION2,
                    Documento = p.PROVIDENTIFICACION,
                    LugarExpedicion = p.PROVLUGAREXPEDICION != null ? (int)p.PROVLUGAREXPEDICION : (int?)null,
                    Nombre = p.PROVRAZONSOCIAL,
                    CodigoCIIU = p.ACECCODIGOACTIVIDAD,
                    TipoDocumentoRep = (int?)p.CLASTIPOIDENTREPRES2,
                    DocumentoRep = p.PROVIDENTREPRESENTANTE,
                    NombreRep = p.PROVREPRESENTANTELEGAL,
                    FechaExpedicionRep = p.PROVFECEXPIDENTREPRESENTANTE,
                    LugarExpedicionRep = (int?)p.PROVLUGAREXPIDENTREPRESENT,
                    FechaNacimientoRep = p.PROVFECHANACIMIENTOREP,
                    FechaNacimiento = p.PROVFECHANACIMIENTO,
                    LugarNacimiento = (int?)p.PROVLUGARNACIMIENTO,
                    DireccionResidencia = p.PROVDIRECCIONPRINCIAL,
                    DireccionJuridica = p.PROVDIRECCIONPRINCIAL,
                    DireccionComercial = p.PROVDIRECCIONCOMERCIAL,
                    Telefono = (long)p.PROVTELEFONO,
                    Email = p.PROVEMAIL,
                    ActividadEconomica = p.ACECCODIGOACTIVIDAD,
                    Profesion = p.PROVPROFESION,
                    Actividad = p.PROVACTIVIDAD != null ? (int)p.PROVACTIVIDAD : (int?)null,
                    Empresa = p.PROVEMPRESATRABAJA,
                    Cargo = p.PROVCARGO,
                    Fax = p.PROVFAX,
                    Ciudad = p.PROVCIUDAD != null ? (int)p.PROVCIUDAD : (int?)null,
                    TelefonoEmpresa = p.PROVTELEFONOEMPRESA,
                    CiudadEmpresa = p.PROVCIUDADEMPRESA,
                    ManejaRecursos = (p.PROVMANEJARECURSOPUBLICOS == Configuracion.ValorSI),
                    ReconocimientoPublico = (p.PROVRECONOCIMIENTOPUBLICO == Configuracion.ValorSI),
                    PoderPublico = (p.PROVPODERPUBLICO == Configuracion.ValorSI),
                    RespuestaAfirmativa = p.PROVOBSERVACION,
                    TipoEmpresa = (int?)p.CLASTIPOEMPRESA13,
                    SectorEconomia = (int?)p.CLASSECTORECONOMICO14,
                    NacionalidadRep = (int?)p.PROVNACIONALIDADREPRESENTANTE,
                    IngresosMensuales = p.PROVINGRESOSMENSUALES,
                    EgresosMensuales = p.PROVEGRESOSMENSUALES,
                    Activos = p.PROVACTIVOS,
                    Pasivos = p.PROVPASIVOS,
                    Patrimonio = p.PROVPATRIMONIO,
                    OtrosIngresos = p.PROVOTROSINGRESOS,
                    ConceptoOtrosIngresos = p.PROVCONCEPTOOTROSINGRESOS,
                    MonedaExtranjera = (p.PROVTRANSMONEDAEXT == Configuracion.ValorSI),
                    TipoMonedaExtranjera = (int?)p.CLASTIPOMONEDA5,
                    CuentasMonedaExtranjera = (p.PROVPRODFINANEXT == Configuracion.ValorSI),
                    EntidadEstatal = (p.PROVENTIDADESTATAL != null) ? p.PROVENTIDADESTATAL == Configuracion.ValorSI ? true : false : false,
                    EntidadSinLucro = (p.PROVENTIDADSINANILUCRO != null) ? p.PROVENTIDADSINANILUCRO == Configuracion.ValorSI ? true : false : false,                    
                    ResolEntidadSinLucro = p.PROVRESOLENTSINANILUCRO,
                    GranContribuyente = (p.PROVGRANCONTRIBUYENTE != null) ? p.PROVGRANCONTRIBUYENTE == Configuracion.ValorSI ? true : false : false,                    
                    ResolGranContribuyente = p.PROVRESOLGRANCONTRIBUYENTE,
                    ResponsableIVA = (p.PROVRESPONSABLEIVA != null) ? p.PROVRESPONSABLEIVA == Configuracion.ValorSI ? true : false : false,                     
                    Autorretenedor = p.PROVAUTORRETENEDOR == Configuracion.ValorSI ? true : false,
                    ResolAutorretenedor = p.PROVRESOLAUTORRETENEDOR,
                    ContribuyenteRenta =p.PROVCONTRIBUYENTERENTA == Configuracion.ValorSI ? true : false,
                    AgenteRetenedorIVA = p.PROVAGENTERETENEDORIVA == Configuracion.ValorSI ? true : false,
                    ResolAgenteRetenedorIVA = p.PROVRESOLAGENTERETENEDORIVA,
                    ResponsableIndyComer = p.PROVINDUSTRIAYCOMERCIO == Configuracion.ValorSI ? true : false,
                    //ResponsableOtros = (p.PROVACTICOMERCIALOTROSMUNI != null && p.PROVACTICOMERCIALOTROSMUNI == "S") ? true : false,
                    EntidadBancaria = (int?)p.CLASENTIDADBANCARIA6,
                    Sucursal = p.PROVSUCURSALBANCO,
                    CiudadSucursal = (int?)p.PROVCIUDADSUCURSAL,
                    TelefonoSucursal = p.PROVTELEFONOSUCURSAL,
                    DireccionSucursal = p.PROVDIRECCIONSUCURSAL,
                    Cuenta = p.PROVNUMEROCUENTA,
                    TipoCuenta = (int?)p.CLASTIPOCUENTA7,
                    TitularCuenta = p.PROVTITULARCUENTA,
                    IdentificacionCuenta = p.PROVIDENTITULARCUENTA,
                    /*Accionistas = null,
                    Especialidades = null,
                    Certificaciones = null,*/
                    ActEcoPereira = p.CLASACTPEREIRA14, //(p.CLASACTPEREIRA14 != null) ? (int)p.CLASACTPEREIRA14 : (int?)null,
                    CodigoCIIUIndustriaPereira = p.CLASACTPEREIRA14,
                    //ActEcoOtros = p.CLASACTOTROS14, // (p.CLASACTOTROS14 != null) ? (int)p.CLASACTOTROS14 : (int?)null
                    //CodigoCIIUOtros = p.CLASACTOTROS14,
                    Nit = p.PROVIDENTIFICACION,
                    TelefonoPrincipal = p.PROVTELEFONO,
                    CiudadJuridica = (int?)p.PROVCIUDAD,
                    LugarNacimientoRep = (int?)p.PROVLUGARNACIMIENTOREP,
                    ClasificacionTamano = p.PROVCLASTAMANO,
                    ClasificacionSector = p.PROVCLASSECTOR,
                    OperacionesMercantiles = (p.PROVOPERMERCANTILES == Configuracion.ValorSI),
                    LogsUsuario = (int?)p.LOGSUSUARIO
                    
                };
            }
        }

        public int CodigoProveedor { get; set; }
        public string Nombre { get; set; }
        public int TipoPersona { get; set; }
        public int? TipoDocumento { get; set; }
        public string Documento { get; set; }
        public int? LugarExpedicion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public int? LugarNacimiento { get; set; }
        public string DireccionResidencia { get; set; }
        public int? Ciudad { get; set; }
        public long? Telefono { get; set; }
        public string Email { get; set; }
        public string Profesion { get; set; }
        public int? Actividad { get; set; }
        public string Empresa { get; set; }
        public string Cargo { get; set; }
        public string TelefonoEmpresa { get; set; }
        public string DireccionComercial { get; set; }
        public int? CiudadEmpresa { get; set; }
        public decimal? Fax { get; set; }
        public bool? ManejaRecursos { get; set; }
        public bool? ReconocimientoPublico { get; set; }
        public bool? PoderPublico { get; set; }
        public string RespuestaAfirmativa { get; set; }
        public string NombreJuridica { get; set; }
        public string Nit { get; set; }
        public string DireccionJuridica { get; set; }
        public int? CiudadJuridica { get; set; }
        public long? TelefonoPrincipal { get; set; }
        public string DireccionSucursal { get; set; }
        public int? CiudadSucursal { get; set; }
        public string TelefonoSucursal { get; set; }
        public int? TipoEmpresa { get; set; }
        public int? SectorEconomia { get; set; }
        public string NombreRep { get; set; }
        public int? TipoDocumentoRep { get; set; }
        public string DocumentoRep { get; set; }
        public DateTime? FechaExpedicionRep { get; set; }
        public int? LugarExpedicionRep { get; set; }
        public DateTime? FechaNacimientoRep { get; set; }
        public int? LugarNacimientoRep { get; set; }
        public int? NacionalidadRep { get; set; }
        // TODO incluir en una lista
        public List<Accionista> Accionistas { get; set; }
        public string ActividadEconomica { get; set; }
        public string CodigoCIIU { get; set; }
        public decimal? IngresosMensuales { get; set; }
        public decimal? EgresosMensuales { get; set; }
        public decimal? Activos { get; set; }
        public decimal? Pasivos { get; set; }
        public decimal? Patrimonio { get; set; }
        public decimal? OtrosIngresos { get; set; }
        public string ConceptoOtrosIngresos { get; set; }
        public bool MonedaExtranjera { get; set; }
        public int? TipoMonedaExtranjera { get; set; }
        public bool CuentasMonedaExtranjera { get; set; }
        public bool? EntidadEstatal { get; set; }
        public bool? EntidadSinLucro { get; set; }
        public string ResolEntidadSinLucro { get; set; }
        public bool? GranContribuyente { get; set; }
        public string ResolGranContribuyente { get; set; }
        public bool? ResponsableIVA { get; set; }                
        public bool? Autorretenedor { get; set; }
        public string ResolAutorretenedor { get; set; }
        public bool? ContribuyenteRenta { get; set; }        
        public bool? AgenteRetenedorIVA { get; set; }
        public string ResolAgenteRetenedorIVA { get; set; }
        public bool? ResponsableIndyComer { get; set; }        
        public List<Especialidad> Especialidades { get; set; }
        public int? EntidadBancaria { get; set; }
        public string Sucursal { get; set; }
        public string Cuenta { get; set; }
        public int? TipoCuenta { get; set; }
        public string TitularCuenta { get; set; }
        public string IdentificacionCuenta { get; set; }
        public List<Documento> Certificaciones { get; set; }
        public string ActEcoPereira { get; set; }
        public string ActEcoOtros { get; set; }
        public string ClasificacionTamano { get; set; }
        public string ClasificacionSector { get; set; }
        public bool OperacionesMercantiles { get; set; }
        public string CodigoCIIUIndustriaPereira { get; set; }
        public string CodigoCIIUOtros { get; set; }
        public int? LogsUsuario { get; set; }


        public static Proveedor CrearProveedor(PONEPROVEEDOR original)
        {
            var func = FromProveedor.Compile();
            var vm = func(original);

            return vm;
        }

    }
}
