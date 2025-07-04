using Negocio.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Negocio.Model
{
    public class ProveedorFormato
    {
        public static Expression<Func<FOBTENERPROVEEDORResult, ProveedorFormato>> FromProveedor
        {
            get
            {
                return p => new ProveedorFormato
                {
                    CodigoProveedor = (int)p.PROVPROVEEDOR,
                    Nombre = p.PROVRAZONSOCIAL,
                    LugarNacimiento = p.PROVLUGARNACIMIENTO,
                    LugarExpedicion = p.PROVLUGARNACIMIENTO,
                    FechaNacimiento = p.PROVFECHANACIMIENTO,
                    DireccionResidencia = p.PROVDIRECCIONPRINCIAL,
                    Ciudad = p.PROVCIUDAD,
                    Telefono = (long?)p.PROVTELEFONO,
                    Email = p.PROVEMAIL,
                    Profesion = p.PROVPROFESION,
                    Actividad = p.PROVACTIVIDAD != null ? (int)p.PROVACTIVIDAD : (int?)null,
                    Empresa = p.PROVEMPRESATRABAJA,
                    Cargo = p.PROVCARGO,
                    Fax = p.PROVFAX,                    
                    TelefonoEmpresa = p.PROVTELEFONOEMPRESA,
                    CiudadEmpresa = p.PROVCIUDADEMPRESA,
                    Documento = p.PROVIDENTIFICACION,
                    DireccionComercial = p.PROVDIRECCIONCOMERCIAL,
                    TipoDocumento = (int)p.CLASTIPOIDENTIFICACION2,
                    ManejaRecursos = (p.PROVMANEJARECURSOPUBLICOS == Configuracion.ValorSI) ? true : false,
                    ReconocimientoPublico = (p.PROVRECONOCIMIENTOPUBLICO == Configuracion.ValorSI) ? true : false,
                    PoderPublico = (p.PROVPODERPUBLICO == Configuracion.ValorSI) ? true : false,
                    RespuestaAfirmativa = p.PROVOBSERVACION,                    
                    //Persona juridica
                    TipoPersona = (int)p.CLASTIPOPERSONAL1,
                    CiudadJuridica = p.PROVCIUDAD,
                    DireccionJuridica = p.PROVDIRECCIONPRINCIAL,
                    CiudadSucursal = p.PROVCIUDADSUCURSAL,
                    TipoEmpresa = p.CLASTIPOEMPRESA13,
                    SectorEconomia = p.CLASSECTORECONOMICO14,
                    NombreRep = p.PROVREPRESENTANTELEGAL,
                    TipoDocumentoRep = p.CLASTIPOIDENTREPRES2,
                    DocumentoRep = p.PROVIDENTREPRESENTANTE,
                    FechaExpedicionRep = p.PROVFECEXPIDENTREPRESENTANTE,
                    LugarNacimientoRep = p.PROVLUGARNACIMIENTOREP,
                    LugarExpedicionRep = p.PROVLUGAREXPIDENTREPRESENT,
                    FechaNacimientoRep = p.PROVFECHANACIMIENTOREP,
                    NacionalidadRep = p.PROVNACIONALIDADREPRESENTANTE,                    
                    ActividadEconomica = p.ACECNOMBRE,
                    CodigoCIIU = p.ACECCODIGOACTIVIDAD,
                    IngresosMensuales = p.PROVINGRESOSMENSUALES,
                    EgresosMensuales = p.PROVEGRESOSMENSUALES,
                    Activos = p.PROVACTIVOS,
                    Pasivos = p.PROVPASIVOS,
                    Patrimonio = p.PROVPATRIMONIO,
                    OtrosIngresos = p.PROVOTROSINGRESOS,
                    ConceptoOtrosIngresos = p.PROVCONCEPTOOTROSINGRESOS,
                    MonedaExtranjera = (p.PROVTRANSMONEDAEXT == Configuracion.ValorSI) ? true : false,
                    TipoMoneda = p.CLASTIPOMONEDA5,
                    CuentasMonedaExtranjera = (p.PROVPRODFINANEXT == Configuracion.ValorSI) ? true : false,
                    EntidadEstatal = (p.PROVENTIDADESTATAL != null) ? p.PROVENTIDADESTATAL == Configuracion.ValorSI ? true : false : false,
                    EntidadSinLucro = (p.PROVENTIDADSINANILUCRO != null) ? p.PROVENTIDADSINANILUCRO == Configuracion.ValorSI ? true : false : false,
                    ResolEntidadSinLucro = p.PROVRESOLENTSINANILUCRO,
                    GranContribuyente = (p.PROVGRANCONTRIBUYENTE != null) ? p.PROVGRANCONTRIBUYENTE == Configuracion.ValorSI ? true : false : false,
                    ResolGranContribuyente = p.PROVRESOLGRANCONTRIBUYENTE,
                    ResponsableIVA = (p.PROVRESPONSABLEIVA != null) ? p.PROVRESPONSABLEIVA == Configuracion.ValorSI ? true : false : false,
                    Autorretenedor = p.PROVAUTORRETENEDOR == Configuracion.ValorSI ? true : false,
                    ResolAutorretenedor = p.PROVRESOLAUTORRETENEDOR,
                    ContribuyenteRenta = p.PROVCONTRIBUYENTERENTA == Configuracion.ValorSI ? true : false,
                    AgenteRetenedorIVA = p.PROVAGENTERETENEDORIVA == Configuracion.ValorSI ? true : false,
                    ResolAgenteRetenedorIVA = p.PROVRESOLAGENTERETENEDORIVA,
                    ResponsableIndyComer = p.PROVINDUSTRIAYCOMERCIO == Configuracion.ValorSI ? true : false,
                    EntidadBancaria = p.CLASENTIDADBANCARIA6,
                    Sucursal = p.PROVSUCURSALBANCO,
                    Cuenta = p.PROVNUMEROCUENTA,
                    TipoCuenta = p.CLASTIPOCUENTA7,
                    TitularCuenta = p.PROVTITULARCUENTA,
                    IdentificacionCuenta = p.PROVIDENTITULARCUENTA,
                    DireccionSucursal = p.PROVDIRECCIONSUCURSAL,
                    TelefonoSucursal = p.PROVTELEFONOSUCURSAL,
                    ClasificacionTamano = p.PROVCLASTAMANO,
                    ClasificacionSector = p.PROVCLASSECTOR,
                    OperacionesMercantiles = (p.PROVOPERMERCANTILES == Configuracion.ValorSI)
                };
            }
        }
        
        public int CodigoProveedor { get; set; }
        public string Nombre { get; set; }
        public int TipoPersona { get; set; }
        public int? TipoDocumento { get; set; }
        public string Documento { get; set; }
        public string LugarExpedicion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string LugarNacimiento { get; set; }
        public string DireccionResidencia { get; set; }
        public string Ciudad { get; set; }
        public long? Telefono { get; set; }
        public string Email { get; set; }
        public string Profesion { get; set; }
        public int? Actividad { get; set; }
        public string Empresa { get; set; }
        public string Cargo { get; set; }
        public string TelefonoEmpresa { get; set; }
        public string DireccionComercial { get; set; }
        public string CiudadEmpresa { get; set; }
        public decimal? Fax { get; set; }
        public bool? ManejaRecursos { get; set; }
        public bool? ReconocimientoPublico { get; set; }
        public bool? PoderPublico { get; set; }
        public string RespuestaAfirmativa { get; set; }
        public string NombreJuridica { get; set; }
        public string Nit { get; set; }
        public string DireccionJuridica { get; set; }
        public string CiudadJuridica { get; set; }
        public string TelefonoPrincipal { get; set; }
        public string DireccionSucursal { get; set; }
        public string CiudadSucursal { get; set; }
        public string TelefonoSucursal { get; set; }
        public string TipoEmpresa { get; set; }
        public string SectorEconomia { get; set; }
        public string NombreRep { get; set; }
        public string TipoDocumentoRep { get; set; }
        public string DocumentoRep { get; set; }
        public DateTime? FechaExpedicionRep { get; set; }
        public string LugarExpedicionRep { get; set; }
        public DateTime? FechaNacimientoRep { get; set; }
        public string LugarNacimientoRep { get; set; }
        public string NacionalidadRep { get; set; }
        // TODO incluir en una lista
        public List<AccionistaFormato> Accionistas { get; set; }
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
        public string TipoMoneda { get; set; }
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
        public List<EspecialidadFormato> Especialidades { get; set; }
        public string EntidadBancaria { get; set; }
        public string Sucursal { get; set; }
        public string Cuenta { get; set; }
        public string TipoCuenta { get; set; }
        public string TitularCuenta { get; set; }
        public string IdentificacionCuenta { get; set; }
        public List<Documento> Certificaciones { get; set; }
        public string ClasificacionTamano { get; set; }
        public string ClasificacionSector { get; set; }
        public bool OperacionesMercantiles { get; set; }

        public static ProveedorFormato CrearProveedorFormato(FOBTENERPROVEEDORResult original)
        {
            var func = FromProveedor.Compile();
            var vm = func(original);

            return vm;
        }

    }
}
