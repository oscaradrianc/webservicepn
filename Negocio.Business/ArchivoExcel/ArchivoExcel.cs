using Negocio.Data;
using Negocio.Model;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public class ArchivoExcelBusiness : IArchivoExcel
    {
        private readonly IProveedor _proveedor;
        private readonly IUtilidades _utilidades;
        private readonly IDataContextFactory _factory;

        public ArchivoExcelBusiness(IProveedor proveedor, IUtilidades utilidades, IDataContextFactory factory)
        {
            _proveedor = proveedor;
            _utilidades = utilidades;
            _factory = factory;
        }
        /// <summary>
        /// Valida y registra el Formato de excel configurado en la base de datos
        /// </summary>
        /// <param name="request">Objeto tipo ArchivoExcel</param>
        public void RegistrarArchivo(ArchivoExcel request)
        {
            int codigoArchivo = 0;
            using (PORTALNEGOCIODataContext cx = _factory.Create())
            {
                cx.Connection.Open();
                var transaction = cx.Connection.BeginTransaction();
                try
                {

                    var configuraciones = ObtenerConfiguracionArchivo(cx, out codigoArchivo);
                    List<ValoresArchivo> valores = new List<ValoresArchivo>();

                    //lee el archivo de excel de un B64
                    byte[] bin = Convert.FromBase64String(request.ArchivoB64.Split(',')[1]);

                    //Crea el Archivo en Memoria
                    using (MemoryStream stream = new MemoryStream(bin))
                    {
                        HSSFWorkbook excelPackage = new HSSFWorkbook(stream);
                        int sheetIndex = 0;
                        //itera todas las hojas del documento
                        while (sheetIndex <= 3)
                        {
                            ISheet worksheet = excelPackage.GetSheetAt(sheetIndex);

                            if (worksheet == null) return;

                            //itera todas las filas
                            for (int i = 0; i <= worksheet.LastRowNum; i++)
                            {
                                //itera todas las columnas de la fila
                                for (int j = 0; j <= worksheet.GetRow(i).LastCellNum; j++)
                                {
                                    //Valida la celda segun la configuracion en la base de datos
                                    ICell celda = worksheet.GetRow(i).GetCell(j);
                                    var error = ValidaCelda(configuraciones, celda.Address.FormatAsString(), sheetIndex, celda.StringCellValue);
                                    if (error == string.Empty || error == "N")
                                    {
                                        if (celda.StringCellValue != null && error != "N")
                                        {
                                            ValoresArchivo valor = new ValoresArchivo();
                                            valor.celda = celda.Address.FormatAsString();
                                            valor.hoja = sheetIndex;
                                            valor.valor = celda.StringCellValue;
                                            valores.Add(valor);
                                        }

                                    }
                                    else
                                    {
                                        throw new Exception(string.Format("Error en Hoja: {0} Celda: {1} {2}", sheetIndex, celda.Address.FormatAsString()));
                                    }

                                }
                            }

                            sheetIndex++;
                        }

                        AlmacenarValores(cx, valores, request, bin.Length, codigoArchivo);
                    }

                        transaction.Commit();

                }
                catch
                {
                    transaction.Rollback();
                    cx.Connection.Close();
                    throw;
                }

            }
        }


        public async Task<string> ObtenerFormatoProveedor(int idProveedor)
        {
            // Obtener el blob de Base de datos
            using (PORTALNEGOCIODataContext cx = _factory.Create())
            {
                ProveedorFormato proveedorActual = await _proveedor.ObtenerProveedorFormato(idProveedor);

                return await EscribirFormatoProveedor(proveedorActual);
            }
        }

        public async Task<string> ObtenerFormatoProveedorJson(Proveedor proveedor)
        {
            // Obtener el blob de Base de datos
            using (PORTALNEGOCIODataContext cx = _factory.Create())
            {
                return await Task.Run(async () =>
                {

                    ProveedorFormato prov = await _proveedor.ObtenerProveedorFormatoJson(proveedor);

                    return await EscribirFormatoProveedor(prov);
                });
            }
        }

        private async Task<string> EscribirFormatoProveedor(ProveedorFormato proveedorActual)
        {
            using (PORTALNEGOCIODataContext cx = _factory.Create())
            {
                return await Task.Run(() =>
                {
                    byte[] blob = cx.PONEBLOBs.Where(x => x.BLOBBLOB == Configuracion.DocumentoFormatoProveedor).FirstOrDefault().BLOBDATO;

                    var stream = new MemoryStream();
                    stream.Write(blob, 0, blob.Length);
                    stream.Position = 0;

                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(stream, true))
                    {
                        WorkbookPart workbookPart = document.WorkbookPart;
                        Workbook workbook = workbookPart.Workbook;
                        Sheet sheet = workbook.Sheets.Elements<Sheet>().ElementAt(1);
                        WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id.Value);
                        Worksheet worksheet = worksheetPart.Worksheet;

                        if (proveedorActual.TipoPersona == Configuracion.TipoPersonaNatural)
                        {
                            //Nombre
                            if (proveedorActual.TipoPersona == Configuracion.TipoPersonaNatural)
                            {
                                SetCellString(worksheet, 1, 1, proveedorActual.Nombre);
                            }
                            else
                            {
                                SetCellString(worksheet, 1, 1, " ");
                            }

                            if (proveedorActual.TipoDocumento == Configuracion.TipoIdentificacionNIT)
                            {
                                SetCellString(worksheet, 2, 1, "X");
                            }
                            else
                            {
                                SetCellString(worksheet, 3, 1, "X");
                            }

                            //Documento
                            SetCellString(worksheet, 4, 1, proveedorActual.Documento);
                            //Lugar Expedicion documento
                            SetCellString(worksheet, 5, 1, proveedorActual.LugarExpedicion);
                            //Fecha nacimiento
                            SetCellString(worksheet, 6, 1, (proveedorActual.FechaNacimiento != null) ? ((DateTime)proveedorActual.FechaNacimiento).ToString("dd/MM/yyyy") : " ");
                            //Lugar nacimiento
                            SetCellString(worksheet, 7, 1, proveedorActual.LugarNacimiento);
                            //Dir residencia
                            SetCellString(worksheet, 8, 1, proveedorActual.DireccionResidencia);
                            //Ciudad residencia
                            SetCellString(worksheet, 9, 1, proveedorActual.Ciudad);
                            //Telefono residencia
                            SetCellString(worksheet, 10, 1, (proveedorActual.Telefono != null) ? ((double)proveedorActual.Telefono).ToString() : " ");
                            //Email
                            SetCellString(worksheet, 11, 1, proveedorActual.Email);
                            //Profesion
                            SetCellString(worksheet, 12, 1, proveedorActual.Profesion);
                            //Actividad
                            if (proveedorActual.Actividad == 1)
                            {
                                SetCellString(worksheet, 13, 1, "X");
                            }
                            else if (proveedorActual.Actividad == 2)
                            {
                                SetCellString(worksheet, 14, 1, "X");
                            }
                            //Empresa donde trabaja
                            SetCellString(worksheet, 15, 1, proveedorActual.Empresa != null ? proveedorActual.Empresa : " ");
                            //Cargo
                            SetCellString(worksheet, 16, 1, proveedorActual.Cargo != null ? proveedorActual.Cargo : " ");
                            //Telefono trabaja
                            SetCellString(worksheet, 17, 1, proveedorActual.TelefonoEmpresa != null ? proveedorActual.TelefonoEmpresa : " ");
                            //Direccion comercial
                            SetCellString(worksheet, 18, 1, proveedorActual.DireccionComercial != null ? proveedorActual.DireccionComercial : " ");
                            //Ciudad Empresa
                            SetCellString(worksheet, 19, 1, proveedorActual.CiudadEmpresa != null ? proveedorActual.CiudadEmpresa : " ");
                            //Fax
                            SetCellString(worksheet, 20, 1, proveedorActual.Fax.ToString());
                            //Maneja recurso publicos
                            if ((proveedorActual.ManejaRecursos != null) ? (bool)proveedorActual.ManejaRecursos : false)
                            {
                                SetCellString(worksheet, 21, 1, "X");
                            }
                            else
                            {
                                SetCellString(worksheet, 22, 1, "X");
                            }

                            //Reconocimiento publico
                            if ((proveedorActual.ReconocimientoPublico != null) ? (bool)proveedorActual.ReconocimientoPublico : false)
                            {
                                SetCellString(worksheet, 23, 1, "X");
                            }
                            else
                            {
                                SetCellString(worksheet, 24, 1, "X");
                            }

                            //Poder publico
                            if ((proveedorActual.PoderPublico != null) ? (bool)proveedorActual.PoderPublico : false)
                            {
                                SetCellString(worksheet, 25, 1, "X");
                            }
                            else
                            {
                                SetCellString(worksheet, 26, 1, "X");
                            }

                            //Explicacion poder publico
                            if ((proveedorActual.ManejaRecursos != null && (bool)proveedorActual.ManejaRecursos) ||
                                (proveedorActual.ReconocimientoPublico != null && (bool)proveedorActual.ReconocimientoPublico) ||
                                (proveedorActual.PoderPublico != null && (bool)proveedorActual.PoderPublico))
                            {
                                SetCellString(worksheet, 27, 1, proveedorActual.RespuestaAfirmativa);
                            }
                            else
                            {
                                SetCellString(worksheet, 27, 1, " ");
                            }
                        }

                        //Persona Juridica
                        if (proveedorActual.TipoPersona == Configuracion.TipoPersonaJuridica)
                        {
                            //Razon Social
                            SetCellString(worksheet, 29, 1, proveedorActual.Nombre);
                            //NIT
                            SetCellString(worksheet, 30, 1, proveedorActual.Documento);
                            //Direccion oficina ppal
                            SetCellString(worksheet, 31, 1, proveedorActual.DireccionJuridica);
                            //Ciudad oficina ppal
                            SetCellString(worksheet, 32, 1, proveedorActual.CiudadJuridica);
                            //Telefono oficina ppal
                            SetCellString(worksheet, 33, 1, (proveedorActual.TelefonoPrincipal == null) ? " " : proveedorActual.TelefonoPrincipal);
                            //Direccion Sucursal
                            SetCellString(worksheet, 34, 1, (proveedorActual.DireccionSucursal == null) ? " " : proveedorActual.DireccionSucursal);
                            //Ciudad Sucursal
                            SetCellString(worksheet, 35, 1, (proveedorActual.CiudadSucursal == null) ? " " : proveedorActual.CiudadSucursal);
                            //Telefono Sucursal
                            SetCellString(worksheet, 36, 1, (proveedorActual.TelefonoSucursal == null) ? " " : proveedorActual.TelefonoSucursal);
                            //Tipo de empresa
                            SetCellString(worksheet, 37, 1, proveedorActual.TipoEmpresa);
                            //Sector de la economia
                            SetCellString(worksheet, 38, 1, proveedorActual.SectorEconomia);
                            //Email persona juridica
                            SetCellString(worksheet, 39, 1, proveedorActual.Email);
                            //Nombre representante
                            SetCellString(worksheet, 41, 1, proveedorActual.NombreRep);
                            //Tipo doc rep
                            SetCellString(worksheet, 42, 1, proveedorActual.TipoDocumentoRep);
                            //Identificacion repre
                            SetCellString(worksheet, 43, 1, proveedorActual.DocumentoRep);
                            //Fecha expedicion doc repre
                            SetCellString(worksheet, 44, 1, (proveedorActual.FechaExpedicionRep != null) ? ((DateTime)proveedorActual.FechaExpedicionRep).ToString("dd/MM/yyyy") : " ");
                            //Lugar exp doc repre
                            SetCellString(worksheet, 45, 1, proveedorActual.LugarExpedicionRep);
                            //Fecha nacimiento repre
                            SetCellString(worksheet, 46, 1, (proveedorActual.FechaNacimientoRep != null) ? ((DateTime)proveedorActual.FechaNacimientoRep).ToString("dd/MM/yyyy") : " ");
                            //Lugar nacimiento repre
                            SetCellString(worksheet, 47, 1, proveedorActual.LugarNacimientoRep);
                            //Nacionalidad repre
                            SetCellString(worksheet, 48, 1, proveedorActual.NacionalidadRep);
                        }
                        else
                        {
                            SetCellString(worksheet, 29, 1, " ");
                        }

                        //ActividadEconomica
                        SetCellString(worksheet, 50, 1, proveedorActual.ActividadEconomica);
                        //Codigo CIIU
                        SetCellString(worksheet, 51, 1, proveedorActual.CodigoCIIU);
                        //Ingresos Mensuales (Pesos)
                        if (proveedorActual.IngresosMensuales != null)
                        {
                            SetCellNumber(worksheet, 52, 1, (double)proveedorActual.IngresosMensuales);
                        }
                        else
                        {
                            SetCellString(worksheet, 52, 1, " ");
                        }

                        //Egresos Mensuales
                        if (proveedorActual.EgresosMensuales != null)
                        {
                            SetCellNumber(worksheet, 53, 1, (double)proveedorActual.EgresosMensuales);
                        }
                        else
                        {
                            SetCellString(worksheet, 53, 1, " ");
                        }

                        //Activos (Pesos)
                        if (proveedorActual.Activos != null)
                        {
                            SetCellNumber(worksheet, 54, 1, (double)proveedorActual.Activos);
                        }
                        else
                        {
                            SetCellString(worksheet, 54, 1, " ");
                        }

                        //Pasivos
                        if (proveedorActual.Pasivos != null)
                        {
                            SetCellNumber(worksheet, 55, 1, (double)proveedorActual.Pasivos);
                        }
                        else
                        {
                            SetCellString(worksheet, 55, 1, " ");
                        }

                        //Patrimonio (Pesos)
                        if (proveedorActual.Patrimonio != null)
                        {
                            SetCellNumber(worksheet, 56, 1, (double)proveedorActual.Patrimonio);
                        }
                        else
                        {
                            SetCellString(worksheet, 56, 1, " ");
                        }

                        //Otros Ingresos
                        if (proveedorActual.OtrosIngresos != null)
                        {
                            SetCellNumber(worksheet, 57, 1, (double)proveedorActual.OtrosIngresos);
                        }
                        else
                        {
                            SetCellString(worksheet, 57, 1, " ");
                        }

                        //Concepto otros ingresos
                        SetCellString(worksheet, 58, 1, proveedorActual.ConceptoOtrosIngresos ?? " ");
                        //Transacciones moneda extranjera
                        if (proveedorActual.MonedaExtranjera)
                        {
                            SetCellString(worksheet, 59, 1, "X");
                            //Tipo Moneda Extranjera
                            SetCellString(worksheet, 61, 1, proveedorActual.TipoMoneda);
                        }
                        else
                        {
                            SetCellString(worksheet, 60, 1, "X");
                            //Tipo Moneda Extranjera
                            SetCellString(worksheet, 61, 1, " ");
                        }

                        //Productos en el exterior
                        if (proveedorActual.CuentasMonedaExtranjera)
                        {
                            SetCellString(worksheet, 62, 1, "X");
                        }
                        else
                        {
                            SetCellString(worksheet, 63, 1, "X");
                        }

                        //Entidad Estatal
                        SetCellBoolean(worksheet, 65, 1, proveedorActual.EntidadEstatal ?? false);
                        //Entidad sin animo de lucro
                        SetCellBoolean(worksheet, 99, 1, proveedorActual.EntidadSinLucro ?? false);
                        //Resolucion sin animo de lucro
                        SetCellString(worksheet, 100, 1, proveedorActual.ResolEntidadSinLucro ?? " ");
                        //Gran contribuyente
                        SetCellBoolean(worksheet, 66, 1, proveedorActual.GranContribuyente ?? false);
                        //No responsable iva
                        SetCellString(worksheet, 68, 1, proveedorActual.ResolGranContribuyente ?? " ");
                        //Responsable de iva
                        SetCellBoolean(worksheet, 67, 1, proveedorActual.ResponsableIVA ?? false);
                        //Autorretenedor
                        SetCellBoolean(worksheet, 69, 1, proveedorActual.Autorretenedor ?? false);
                        //Resolucion Autorretenedor
                        SetCellString(worksheet, 73, 1, proveedorActual.ResolAutorretenedor ?? " ");
                        //Contribuyente
                        SetCellBoolean(worksheet, 70, 1, proveedorActual.ContribuyenteRenta ?? false);
                        //Agente retenedor de iva
                        SetCellBoolean(worksheet, 71, 1, proveedorActual.AgenteRetenedorIVA ?? false);
                        //Resolucion agente retenedor de iva
                        SetCellString(worksheet, 74, 1, proveedorActual.ResolAgenteRetenedorIVA ?? " ");
                        //Industria y comercio
                        SetCellBoolean(worksheet, 72, 1, proveedorActual.ResponsableIndyComer ?? false);

                        //Entidad bancaria
                        SetCellString(worksheet, 76, 1, proveedorActual.EntidadBancaria);
                        //Sucursal
                        SetCellString(worksheet, 77, 1, proveedorActual.Sucursal);
                        //Numero cuenta
                        SetCellString(worksheet, 78, 1, proveedorActual.Cuenta);
                        //Tipo Cuenta
                        SetCellString(worksheet, 79, 1, proveedorActual.TipoCuenta);
                        //Titular cuenta
                        SetCellString(worksheet, 80, 1, proveedorActual.TitularCuenta);
                        //Identificacion titular
                        SetCellString(worksheet, 81, 1, proveedorActual.IdentificacionCuenta);

                        int index = 1;
                        int row = 85;
                        proveedorActual.Especialidades.ForEach(e =>
                        {
                            //item
                            SetCellNumber(worksheet, row, 0, index);
                            //Principales bienes
                            SetCellString(worksheet, row, 1, e.BienesOServicios);
                            //Importador
                            SetCellString(worksheet, row, 2, e.ComercioEspecialidad);
                            //Fabricante
                            SetCellString(worksheet, row, 3, e.ServiciosEspecialidad);
                            //Distribuidor Exclusivo
                            SetCellString(worksheet, row, 4, e.ManufacturaEspecialidad);
                            //Excluida
                            SetCellString(worksheet, row, 5, e.GravadaEspecialidad);
                            //Grabada
                            SetCellString(worksheet, row, 6, e.GravadaEspecialidad);

                            index++;
                            row++;
                        });

                        //Accionistas
                        row = 92;
                        proveedorActual.Accionistas.ForEach(a =>
                        {
                            //Nombre
                            SetCellString(worksheet, row, 0, a.NombreAccionista);
                            //Tipo identificacion
                            SetCellString(worksheet, row, 1, a.TipoDocumentoAccionista);
                            //Identificacion
                            SetCellString(worksheet, row, 2, a.IdentificacionAccionista);
                            //% Participacion
                            if (a.ParticipacionAccionista != null)
                            {
                                SetCellNumber(worksheet, row, 3, (double)a.ParticipacionAccionista);
                            }
                            else
                            {
                                SetCellString(worksheet, row, 3, " ");
                            }

                            row++;
                        });

                        //INFORMACIÓN CLASIFICACIÓN TAMAÑO EMPRESARIAL - DECRETO 957 DE 2019
                        SetCellString(worksheet, 105, 1, (proveedorActual.ClasificacionTamano == "M") ? "X" : " ");
                        SetCellString(worksheet, 106, 1, (proveedorActual.ClasificacionTamano == "P") ? "X" : " ");
                        SetCellString(worksheet, 107, 1, (proveedorActual.ClasificacionTamano == "E") ? "X" : " ");
                        SetCellString(worksheet, 108, 1, (proveedorActual.ClasificacionTamano == "G") ? "X" : " ");

                        SetCellString(worksheet, 109, 1, (proveedorActual.ClasificacionSector == "M") ? "X" : " ");
                        SetCellString(worksheet, 110, 1, (proveedorActual.ClasificacionSector == "S") ? "X" : " ");
                        SetCellString(worksheet, 111, 1, (proveedorActual.ClasificacionSector == "C") ? "X" : " ");

                        SetCellString(worksheet, 112, 1, (proveedorActual.OperacionesMercantiles) ? "X" : " ");
                        SetCellString(worksheet, 113, 1, (!proveedorActual.OperacionesMercantiles) ? "X" : " ");

                        //Force formula recalculation on open
                        var calcProps = workbook.GetFirstChild<CalculationProperties>();
                        if (calcProps != null)
                        {
                            calcProps.FullCalculationOnLoad = true;
                        }
                    }

                    return Convert.ToBase64String(stream.ToArray());
                });
            }
        }

        /// <summary>
        /// Guarda los valores en la BD
        /// </summary>
        /// <param name="cx"></param>
        /// <param name="valores"></param>
        /// <param name="request"></param>
        /// <param name="tamanio"></param>
        private void AlmacenarValores(PORTALNEGOCIODataContext cx, List<ValoresArchivo> valores, ArchivoExcel request, int tamanio, int codigoArchivo)
        {
            // Inserta el blob
            var tblBlobs = new PONEBLOB();
            tblBlobs.BLOBDATO = _utilidades.DecodificarArchivo(request.ArchivoB64);
            int codigoBlob = _utilidades.GetSecuencia("SECU_PONEBLOB", cx);
            tblBlobs.BLOBBLOB = codigoBlob;
            cx.PONEBLOBs.InsertOnSubmit(tblBlobs);
            cx.SubmitChanges();

            //Inserta el encabezado
            PONEFORMATOXPROVEEDOR tbl_formatoxproveedor = new PONEFORMATOXPROVEEDOR();
            var codigoEncabezado = _utilidades.GetSecuencia("SECU_PONEFORMATOXPROV", cx);
            tbl_formatoxproveedor.FOPRFOPR = codigoEncabezado;
            tbl_formatoxproveedor.FOPRVIGENCIA = request.Vigencia;
            tbl_formatoxproveedor.FOPRNOMBREARCHIVO = request.NombreArchivo;
            tbl_formatoxproveedor.FOPRAPROBADO = "N";
            tbl_formatoxproveedor.BLOBBLOB = codigoBlob;
            tbl_formatoxproveedor.FOPRTAMANIO = tamanio;
            tbl_formatoxproveedor.PROVPROVEEDOR = request.CodigoProveedor;
            tbl_formatoxproveedor.CONFCONF = codigoArchivo;

            cx.PONEFORMATOXPROVEEDORs.InsertOnSubmit(tbl_formatoxproveedor);
            cx.SubmitChanges();


            //Inserta el detalle
            foreach (var item in valores)
            {
                PONEDETAFORMATOXPROVEEDOR tbl_detaformato = new PONEDETAFORMATOXPROVEEDOR();
                tbl_detaformato.DEFODEFO = _utilidades.GetSecuencia("SECU_PONEFORMATOXPROV", cx);
                tbl_detaformato.DEFOCELDA = item.celda;
                tbl_detaformato.DEFOVALOR = item.valor;
                tbl_detaformato.DEFOHOJA = item.hoja;
                tbl_detaformato.FOPRFOPR = codigoEncabezado;
                cx.PONEDETAFORMATOXPROVEEDORs.InsertOnSubmit(tbl_detaformato);
            }

            cx.SubmitChanges();
        }

        /// <summary>
        /// Obtiene la configuracion del archivo de excel parametrizada en la BD
        /// </summary>
        /// <param name="cx"></param>
        /// <returns></returns>
        private List<ConfiguracionArchivo> ObtenerConfiguracionArchivo(PORTALNEGOCIODataContext cx, out int codigoArchivo)
        {
            List<ConfiguracionArchivo> lst_configuracion = new List<ConfiguracionArchivo>();

            lst_configuracion = (from p in cx.PONECONFARCHIVOs
                        join q in cx.PONEDETALLECONFARCHIVOs on p.CONFCONF equals q.CONFCONF
                        where p.CONFESTADO == "A"
                        select new ConfiguracionArchivo
                        {
                            codigoArchivo = (int)q.CONFCONF,
                            celda = q.DECFCELDA,
                            error = q.DECFERROR,
                            formula = q.DECFFORMULA,
                            hoja = (int)q.DECFHOJA,
                            validar = q.DECFVALIDAR,
                            valor = q.DECFVALOR,
                            guardar = q.DECFGUARDAR
                        }).ToList();

            codigoArchivo = lst_configuracion.FirstOrDefault().codigoArchivo;

            return lst_configuracion;

        }


        /// <summary>
        /// Valida la celda
        /// </summary>
        /// <param name="configuraciones"></param>
        /// <param name="celda"></param>
        /// <param name="hoja"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        private string ValidaCelda(List<ConfiguracionArchivo> configuraciones, string celda, int hoja, object valor)
        {
            string error = string.Empty;
            //Busca la celda
            var configuracion = configuraciones.Where(x => x.celda == celda && x.hoja == hoja)
                .FirstOrDefault();

            if(configuracion != null)
            {
                if (configuracion.validar == "S")
                {
                    if(configuracion.guardar == "S")
                    {
                        if (valor == null)
                        {
                            error = configuracion.error;
                        }

                    } else {
                        error = "N";
                    }
                }
            }
            else
            {
                error = "N";
            }


            return error;
        }

        private Proveedor ObtenerValoresProveedor(int idProveedor)
        {
            return _proveedor.ObtenerProveedor(idProveedor);
        }

        #region Open XML SDK Helpers

        private static string CellRef(int npoiRow, int npoiCol)
        {
            return GetColumnLetter(npoiCol) + (npoiRow + 1);
        }

        private static string GetColumnLetter(int col)
        {
            string column = "";
            while (col >= 26)
            {
                column = (char)('A' + (col % 26)) + column;
                col = col / 26 - 1;
            }
            return (char)('A' + col) + column;
        }

        private static Cell FindOrCreateCell(Worksheet worksheet, string cellRef)
        {
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            uint rowIndex = uint.Parse(new string(cellRef.Where(char.IsDigit).ToArray()));

            Row row = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndex);
            if (row == null)
            {
                row = new Row { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            Cell cell = row.Elements<Cell>().FirstOrDefault(c =>
                c.CellReference != null && c.CellReference.Value == cellRef);

            if (cell == null)
            {
                cell = new Cell { CellReference = cellRef };
                row.Append(cell);
            }

            return cell;
        }

        private static void SetCellString(Worksheet worksheet, int row, int col, string value)
        {
            Cell cell = FindOrCreateCell(worksheet, CellRef(row, col));
            cell.CellFormula = null;
            cell.DataType = new EnumValue<CellValues>(CellValues.InlineString);
            cell.InlineString = new InlineString { Text = new Text(value ?? "") };
        }

        private static void SetCellNumber(Worksheet worksheet, int row, int col, double value)
        {
            Cell cell = FindOrCreateCell(worksheet, CellRef(row, col));
            cell.CellFormula = null;
            cell.InlineString = null;
            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(value.ToString(CultureInfo.InvariantCulture));
        }

        private static void SetCellBoolean(Worksheet worksheet, int row, int col, bool value)
        {
            Cell cell = FindOrCreateCell(worksheet, CellRef(row, col));
            cell.CellFormula = null;
            cell.InlineString = null;
            cell.DataType = new EnumValue<CellValues>(CellValues.Boolean);
            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(value ? "1" : "0");
        }

        #endregion
    }
}
