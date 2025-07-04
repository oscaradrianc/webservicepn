using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface IUtilidades
    {
        List<ActividadEconomica> ObtenerActividadEconomica();
        Task<ActividadEconomica> ObtenerActividadEconomica(string codigoCIIU);
        List<Catalogo> ObtenerCatalogo(string id = null);
        Task<List<ClaseValor>> ObtenerClaseValor(int idClase);
        string ObtenerValorClaseValor(int idClaseValor);
        List<Municipio> ObtenerMunicipios();
        string Encriptar(string texto, string machineKey);
        List<Areas> ObtenerAreas();
        List<Gerencias> ObtenerGerencias();
        string GetConstante(string constante);
        List<DocumentosxPersona> ObtenerDocumentos();
        List<Pais> ObtenerPais();
        List<Departamento> ObtenerDepartamento();
        string GetRandomKey();
        string GetStringEncriptado(string texto, string machineKey);
        int GetSecuencia(string nombreSecuencia, PORTALNEGOCIODataContext ctx);
        byte[] DecodificarArchivo(string b64);
        //string GetConstante(string nombreConstante, PORTALNEGOCIODataContext ctx);
        void SendMail(List<string> listaCorreos, string asunto, string mensaje, bool bcc = false);
        string ConvertirMensaje(string mensaje, string parametros);
        string ObtenerBlob(int idBlob, PORTALNEGOCIODataContext cx);
        decimal? IsDecimal(string strNumber);
        DateTime TruncateToDayStart(DateTime dt);
        List<string> ObtenerCorreosProveedor();
        List<string> ObtenerCorreoProveedoresSolicitud(int codigoSolicitud);
        Task<Municipio> ObtenerMunicipio(int idMunicipio);
        Task<PONEDOCUMENTO> ObtenerDocumentoxId(int idDocumento);
        (string contentType, byte[] data) DecodificarArchivoContentType(string base64Content);
    }
}
