using Negocio.Data;
using Negocio.Model;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
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

        public ArchivoExcelBusiness(IProveedor proveedor, IUtilidades utilidades)
        {
            _proveedor = proveedor;
            _utilidades = utilidades;
        }
        /// <summary>
        /// Valida y registra el Formato de excel configurado en la base de datos
        /// </summary>
        /// <param name="request">Objeto tipo ArchivoExcel</param>
        public void RegistrarArchivo(ArchivoExcel request)
        {
            int codigoArchivo = 0;
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
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
                catch (Exception ex)
                {
                    transaction.Rollback();
                    cx.Connection.Close();
                    throw ex;
                }

            }
        }


        public async Task<string> ObtenerFormatoProveedor(int idProveedor)
        {
            // Obtener el blob de Base de datos
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                ProveedorFormato proveedorActual = await _proveedor.ObtenerProveedorFormato(idProveedor);

                return await EscribirFormatoProveedor(proveedorActual);
            }
        }

        public async Task<string> ObtenerFormatoProveedorJson(Proveedor proveedor)
        {
            // Obtener el blob de Base de datos
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return await Task.Run(async () =>
                {

                    //Proveedor prov = JsonConvert.DeserializeObject<Proveedor>(proveedorJson);

                    ProveedorFormato prov = await _proveedor.ObtenerProveedorFormatoJson(proveedor);

                    return await EscribirFormatoProveedor(prov);
                });
            }            
        }

        private async  Task<string> EscribirFormatoProveedor(ProveedorFormato proveedorActual)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return await Task.Run(() =>
                {
                    //int codigoBlob = (int)cx.PONEDOCUMENTOs.Where(x => x.DOCUDOCUMENTO == Configuracion.DocumentoFormatoProveedor).FirstOrDefault();
                    byte[] blob = cx.PONEBLOBs.Where(x => x.BLOBBLOB == Configuracion.DocumentoFormatoProveedor).FirstOrDefault().BLOBDATO;


                    using (MemoryStream stream = new MemoryStream(blob))
                    {
                        //Proveedor proveedorActual = ObtenerValoresProveedor(idProveedor);


                        XSSFWorkbook excelPackage = new XSSFWorkbook(stream);
                        ISheet sheet = excelPackage.GetSheetAt(1);

                        if (proveedorActual.TipoPersona == Configuracion.TipoPersonaNatural)
                        {
                            //Nombre
                            if (proveedorActual.TipoPersona == Configuracion.TipoPersonaNatural)
                            {
                                sheet.GetRow(1).CreateCell(1).SetCellValue(proveedorActual.Nombre);
                            }
                            else
                            {
                                sheet.GetRow(1).CreateCell(1).SetCellValue(" ");
                            }
                           
                            if (proveedorActual.TipoDocumento == Configuracion.TipoIdentificacionNIT)
                            {
                                sheet.GetRow(2).CreateCell(1).SetCellValue("X");
                            }
                            else                            
                            {
                                sheet.GetRow(3).CreateCell(1).SetCellValue("X");
                            }
                            
                            //Documento
                            sheet.GetRow(4).CreateCell(1).SetCellValue(proveedorActual.Documento);
                            //Lugar Expedicion documento
                            sheet.GetRow(5).CreateCell(1).SetCellValue(proveedorActual.LugarExpedicion);
                            //Fecha nacimiento
                            sheet.GetRow(6).CreateCell(1).SetCellValue((proveedorActual.FechaNacimiento != null) ? ((DateTime)proveedorActual.FechaNacimiento).ToString("dd/MM/yyyy") : " ");
                            //Lugar nacimiento
                            sheet.GetRow(7).CreateCell(1).SetCellValue(proveedorActual.LugarNacimiento);
                            //Dir residencia 
                            sheet.GetRow(8).CreateCell(1).SetCellValue(proveedorActual.DireccionResidencia);
                            //Ciudad residencia
                            sheet.GetRow(9).CreateCell(1).SetCellValue(proveedorActual.Ciudad);
                            //Telefono residencia
                            sheet.GetRow(10).CreateCell(1).SetCellValue((proveedorActual.Telefono != null) ? ((double)proveedorActual.Telefono).ToString() : " ");
                            //Email
                            sheet.GetRow(11).CreateCell(1).SetCellValue(proveedorActual.Email);
                            //Profesion
                            sheet.GetRow(12).CreateCell(1).SetCellValue(proveedorActual.Profesion);
                            //Actividad
                            if (proveedorActual.Actividad == 1)
                            {
                                sheet.GetRow(13).CreateCell(1).SetCellValue("X");
                            }
                            else
                            if (proveedorActual.Actividad == 2)
                            {
                                sheet.GetRow(14).CreateCell(1).SetCellValue("X");
                            }
                            //Empresa donde trabaja
                            sheet.GetRow(15).CreateCell(1).SetCellValue(proveedorActual.Empresa != null ? proveedorActual.Empresa : " ");
                            //Cargo
                            sheet.GetRow(16).CreateCell(1).SetCellValue(proveedorActual.Cargo != null ? proveedorActual.Cargo : " ");
                            //Telefono trabaja
                            sheet.GetRow(17).CreateCell(1).SetCellValue(proveedorActual.TelefonoEmpresa != null ? proveedorActual.TelefonoEmpresa : " ");
                            //Direccion comercial
                            sheet.GetRow(18).CreateCell(1).SetCellValue(proveedorActual.DireccionComercial != null ? proveedorActual.DireccionComercial : " ");
                            //Ciudad Empresa
                            sheet.GetRow(19).CreateCell(1).SetCellValue(proveedorActual.CiudadEmpresa != null ? proveedorActual.CiudadEmpresa : " ");
                            //Fax
                            sheet.GetRow(20).CreateCell(1).SetCellValue(proveedorActual.Fax.ToString());
                            //Maneja recurso publicos
                            if ((proveedorActual.ManejaRecursos != null) ? (bool)proveedorActual.ManejaRecursos : false)
                            {
                                sheet.GetRow(21).CreateCell(1).SetCellValue("X");
                            }
                            else
                            {
                                sheet.GetRow(22).CreateCell(1).SetCellValue("X");
                            }

                            //Reconocimiento publico
                            if ((proveedorActual.ReconocimientoPublico != null) ? (bool)proveedorActual.ReconocimientoPublico : false)
                            {
                                sheet.GetRow(23).CreateCell(1).SetCellValue("X");
                            }
                            else
                            {
                                sheet.GetRow(24).CreateCell(1).SetCellValue("X");
                            }

                            //Poder publico
                            if ((proveedorActual.PoderPublico != null) ? (bool)proveedorActual.PoderPublico : false)
                            {
                                sheet.GetRow(25).CreateCell(1).SetCellValue("X");
                            }
                            else
                            {
                                sheet.GetRow(26).CreateCell(1).SetCellValue("X");
                            }

                            //Explicacion poder publico
                            if ((proveedorActual.ManejaRecursos != null && (bool)proveedorActual.ManejaRecursos) || 
                                (proveedorActual.ReconocimientoPublico != null && (bool)proveedorActual.ReconocimientoPublico) || 
                                (proveedorActual.PoderPublico != null && (bool)proveedorActual.PoderPublico))
                            {
                                sheet.GetRow(27).CreateCell(1).SetCellValue(proveedorActual.RespuestaAfirmativa);
                            }
                            else
                            {
                                sheet.GetRow(27).CreateCell(1).SetCellValue(" ");
                            }
                        }

                        //Persona Juridica
                        if (proveedorActual.TipoPersona == Configuracion.TipoPersonaJuridica)
                        {
                            //Razon Social
                            sheet.GetRow(29).CreateCell(1).SetCellValue(proveedorActual.Nombre);
                            //NIT
                            sheet.GetRow(30).CreateCell(1).SetCellValue(proveedorActual.Documento);
                            //Direccion oficina ppal
                            sheet.GetRow(31).CreateCell(1).SetCellValue(proveedorActual.DireccionJuridica);
                            //Ciudad oficina ppal
                            sheet.GetRow(32).CreateCell(1).SetCellValue(proveedorActual.CiudadJuridica);
                            //Telefono oficina ppal
                            sheet.GetRow(33).CreateCell(1).SetCellValue((proveedorActual.TelefonoPrincipal == null) ? " " : proveedorActual.TelefonoPrincipal);
                            //Direccion Sucursal
                            sheet.GetRow(34).CreateCell(1).SetCellValue((proveedorActual.DireccionSucursal == null) ? " " : proveedorActual.DireccionSucursal);
                            //Ciudad Sucursal
                            sheet.GetRow(35).CreateCell(1).SetCellValue((proveedorActual.CiudadSucursal == null) ? " " : proveedorActual.CiudadSucursal);
                            //Telefono Sucursal
                            sheet.GetRow(36).CreateCell(1).SetCellValue((proveedorActual.TelefonoSucursal == null) ? " " : proveedorActual.TelefonoSucursal);
                            //Tipo de empresa
                            sheet.GetRow(37).CreateCell(1).SetCellValue(proveedorActual.TipoEmpresa);
                            //Sector de la economina
                            sheet.GetRow(38).CreateCell(1).SetCellValue(proveedorActual.SectorEconomia);
                            //Email persona juridica
                            sheet.GetRow(39).CreateCell(1).SetCellValue(proveedorActual.Email);
                            //Nombre representante
                            sheet.GetRow(41).CreateCell(1).SetCellValue(proveedorActual.NombreRep);
                            //Tipo doc rep
                            sheet.GetRow(42).CreateCell(1).SetCellValue(proveedorActual.TipoDocumentoRep);
                            //Identificacion repre
                            sheet.GetRow(43).CreateCell(1).SetCellValue(proveedorActual.DocumentoRep);
                            //Fecha expedicion doc repre
                            sheet.GetRow(44).CreateCell(1).SetCellValue((proveedorActual.FechaExpedicionRep != null) ? ((DateTime)proveedorActual.FechaExpedicionRep).ToString("dd/MM/yyyy") : " ");
                            //Lugar exp doc repre
                            sheet.GetRow(45).CreateCell(1).SetCellValue(proveedorActual.LugarExpedicionRep);
                            //Fecha nacimiento repre
                            sheet.GetRow(46).CreateCell(1).SetCellValue((proveedorActual.FechaNacimientoRep != null) ? ((DateTime)proveedorActual.FechaNacimientoRep).ToString("dd/MM/yyyy") : " ");
                            //Lugar nacimiento repre
                            sheet.GetRow(47).CreateCell(1).SetCellValue(proveedorActual.LugarNacimientoRep);
                            //Nacionalidad repre
                            sheet.GetRow(48).CreateCell(1).SetCellValue(proveedorActual.NacionalidadRep);
                        }
                        else
                        {
                            sheet.GetRow(29).CreateCell(1).SetCellValue(" ");
                        }

                        //ActividadEconomica
                        sheet.GetRow(50).CreateCell(1).SetCellValue(proveedorActual.ActividadEconomica);
                        //Codigo CIIU
                        sheet.GetRow(51).CreateCell(1).SetCellValue(proveedorActual.CodigoCIIU);
                        //Ingresos Mensuales (Pesos)
                        if (proveedorActual.IngresosMensuales != null)
                        {
                            sheet.GetRow(52).CreateCell(1).SetCellValue((double)proveedorActual.IngresosMensuales);
                        }
                        else
                        {
                            sheet.GetRow(52).CreateCell(1).SetCellValue(" ");
                        }

                        //Egresos Mensuales
                        if (proveedorActual.EgresosMensuales != null)
                        {
                            sheet.GetRow(53).CreateCell(1).SetCellValue((double)proveedorActual.EgresosMensuales);
                        }
                        else
                        {
                            sheet.GetRow(53).CreateCell(1).SetCellValue(" ");
                        }

                        //Activos (Pesos)
                        if (proveedorActual.Activos != null)
                        {
                            sheet.GetRow(54).CreateCell(1).SetCellValue((double)proveedorActual.Activos);
                        }
                        else
                        {
                            sheet.GetRow(54).CreateCell(1).SetCellValue(" ");
                        }

                        //Pasivos
                        if (proveedorActual.Pasivos != null)
                        {
                            sheet.GetRow(55).CreateCell(1).SetCellValue((double)proveedorActual.Pasivos);
                        }
                        else
                        {
                            sheet.GetRow(55).CreateCell(1).SetCellValue(" ");
                        }

                        //Patrimonio (Pesos)
                        if (proveedorActual.Patrimonio != null)
                        {
                            sheet.GetRow(56).CreateCell(1).SetCellValue((double)proveedorActual.Patrimonio);
                        }
                        else
                        {
                            sheet.GetRow(56).CreateCell(1).SetCellValue(" ");
                        }

                        //Otros Ingresos
                        if (proveedorActual.OtrosIngresos != null)
                        {
                            sheet.GetRow(57).CreateCell(1).SetCellValue((double)proveedorActual.OtrosIngresos);
                        }
                        else
                        {
                            sheet.GetRow(57).CreateCell(1).SetCellValue(" ");
                        }

                        //Concepto otros ingresos
                        sheet.GetRow(58).CreateCell(1).SetCellValue(proveedorActual.ConceptoOtrosIngresos ?? " ");
                        //Transacciones moneda extranjera
                        if (proveedorActual.MonedaExtranjera)
                        {
                            sheet.GetRow(59).CreateCell(1).SetCellValue("X");
                            //Tipo Moneda Extranjera
                            sheet.GetRow(61).CreateCell(1).SetCellValue(proveedorActual.TipoMoneda);
                        }
                        else
                        {
                            sheet.GetRow(60).CreateCell(1).SetCellValue("X");
                            //Tipo Moneda Extranjera
                            sheet.GetRow(61).CreateCell(1).SetCellValue(" ");
                        }

                        //Productos en el exterior
                        if (proveedorActual.CuentasMonedaExtranjera)
                        {
                            sheet.GetRow(62).CreateCell(1).SetCellValue("X");
                        }
                        else
                        {
                            sheet.GetRow(63).CreateCell(1).SetCellValue("X");
                        }


                        //Entidad Estatal
                        sheet.GetRow(65).CreateCell(1).SetCellValue(proveedorActual.EntidadEstatal ?? false);
                        //Entidad sin animo de lucro
                        sheet.GetRow(99).CreateCell(1).SetCellValue(proveedorActual.EntidadSinLucro ?? false);
                        //Resolucion sin animo de lucro
                        sheet.GetRow(100).CreateCell(1).SetCellValue(proveedorActual.ResolEntidadSinLucro ?? " ");
                        //Gran contribuyente
                        sheet.GetRow(66).CreateCell(1).SetCellValue(proveedorActual.GranContribuyente ?? false);
                        //No responsable iva
                        sheet.GetRow(68).CreateCell(1).SetCellValue(proveedorActual.ResolGranContribuyente ?? " ");
                        //Reponsable de iva
                        sheet.GetRow(67).CreateCell(1).SetCellValue(proveedorActual.ResponsableIVA ?? false);                        
                        //Autorretenedor
                        sheet.GetRow(69).CreateCell(1).SetCellValue(proveedorActual.Autorretenedor ?? false);
                        //Resolucion Autorretenedor
                        sheet.GetRow(73).CreateCell(1).SetCellValue(proveedorActual.ResolAutorretenedor ?? " ");
                        //Contribuyente
                        sheet.GetRow(70).CreateCell(1).SetCellValue(proveedorActual.ContribuyenteRenta ?? false);
                        //Agente retenedor de iva
                        sheet.GetRow(71).CreateCell(1).SetCellValue(proveedorActual.AgenteRetenedorIVA ?? false);
                        //Resolucion agente retenedor de iva
                        sheet.GetRow(74).CreateCell(1).SetCellValue(proveedorActual.ResolAgenteRetenedorIVA ?? " ");
                        //Indistria y comercio
                        sheet.GetRow(72).CreateCell(1).SetCellValue(proveedorActual.ResponsableIndyComer ?? false);
                        //Actividad industria y comercio pereira
                        //sheet.GetRow(98).CreateCell(1).SetCellValue(proveedorActual.NomIndYComPereira ?? " ");
                        //Codigo ind y com pereira
                        //sheet.GetRow(99).CreateCell(1).SetCellValue(proveedorActual.CodIndYComPereira != null ? proveedorActual.CodIndYComPereira.ToString() :  " ");
                        //Actividad industria y  comercio otros
                        //sheet.GetRow(100).CreateCell(1).SetCellValue(proveedorActual.NomdIndYComOtros ?? " ");
                        //Codigo ind y com otros
                        //sheet.GetRow(101).CreateCell(1).SetCellValue(proveedorActual.CodIndYComOtros != null ? proveedorActual.CodIndYComOtros.ToString() : " ");


                        //Entidad bancaria
                        sheet.GetRow(76).CreateCell(1).SetCellValue(proveedorActual.EntidadBancaria);
                        //Sucursal
                        sheet.GetRow(77).CreateCell(1).SetCellValue(proveedorActual.Sucursal);
                        //Numero cuenta
                        sheet.GetRow(78).CreateCell(1).SetCellValue(proveedorActual.Cuenta);
                        //Tipo Cuenta
                        sheet.GetRow(79).CreateCell(1).SetCellValue(proveedorActual.TipoCuenta);
                        //Titular cuenta
                        sheet.GetRow(80).CreateCell(1).SetCellValue(proveedorActual.TitularCuenta);
                        //Identificacion titular
                        sheet.GetRow(81).CreateCell(1).SetCellValue(proveedorActual.IdentificacionCuenta);

                        int index = 1;
                        int row = 85;
                        //ltaEspecialidad.ForEach(e =>
                        proveedorActual.Especialidades.ForEach(e =>
                        {
                        //item
                        sheet.GetRow(row).CreateCell(0).SetCellValue(index);
                        //Pricipales bienes
                        sheet.GetRow(row).CreateCell(1).SetCellValue(e.BienesOServicios);
                        //Importador
                        sheet.GetRow(row).CreateCell(2).SetCellValue(e.ComercioEspecialidad);
                        //Fabricante
                        sheet.GetRow(row).CreateCell(3).SetCellValue(e.ServiciosEspecialidad);
                        //Distribuidor Exclusivo
                        sheet.GetRow(row).CreateCell(4).SetCellValue(e.ManufacturaEspecialidad);
                        //Comercializador
                        /*sheet.GetRow(row).CreateCell(5).SetCellValue(e.ComercializadorEspecialidad);
                        //Proveedor Servicios
                        sheet.GetRow(row).CreateCell(6).SetCellValue(e.ProveedorEspecialidad);
                        //Consultor
                        sheet.GetRow(row).CreateCell(7).SetCellValue(e.ConsultorEspecialidad);*/
                        //Excluida
                        sheet.GetRow(row).CreateCell(5).SetCellValue(e.GravadaEspecialidad);
                        //Grabada   
                        sheet.GetRow(row).CreateCell(6).SetCellValue(e.GravadaEspecialidad);

                            index++;
                            row++;
                        });//

                        //Accionistas                    
                        row = 92;
                        //ltaAccionistas.ForEach(a =>
                        proveedorActual.Accionistas.ForEach(a =>
                        {
                        //Nombre
                        sheet.GetRow(row).CreateCell(0).SetCellValue(a.NombreAccionista);
                        //Tipo indentificacion
                        sheet.GetRow(row).CreateCell(1).SetCellValue(a.TipoDocumentoAccionista);
                        //Identificacion 
                        sheet.GetRow(row).CreateCell(2).SetCellValue(a.IdentificacionAccionista);
                        //% Participacion
                        if (a.ParticipacionAccionista != null)
                            {
                                sheet.GetRow(row).CreateCell(3).SetCellValue((double)a.ParticipacionAccionista);
                            }
                            else
                            {
                                sheet.GetRow(row).CreateCell(3).SetCellValue(" ");
                            }

                            row++;
                        });

                        ///INFORMACIÓN CLASIFICACIÓN TAMAÑO EMPRESARIAL - DECRETO 957 DE 2019                       
                        sheet.GetRow(105).CreateCell(1).SetCellValue((proveedorActual.ClasificacionTamano == "M") ? "X" : " ");
                        sheet.GetRow(106).CreateCell(1).SetCellValue((proveedorActual.ClasificacionTamano == "P") ? "X" : " ");
                        sheet.GetRow(107).CreateCell(1).SetCellValue((proveedorActual.ClasificacionTamano == "E") ? "X" : " ");
                        sheet.GetRow(108).CreateCell(1).SetCellValue((proveedorActual.ClasificacionTamano == "G") ? "X" : " ");

                        sheet.GetRow(109).CreateCell(1).SetCellValue((proveedorActual.ClasificacionSector == "M") ? "X" : " ");
                        sheet.GetRow(110).CreateCell(1).SetCellValue((proveedorActual.ClasificacionSector == "S") ? "X" : " ");
                        sheet.GetRow(111).CreateCell(1).SetCellValue((proveedorActual.ClasificacionSector == "C") ? "X" : " ");

                        sheet.GetRow(112).CreateCell(1).SetCellValue((proveedorActual.OperacionesMercantiles) ? "X" : " ");
                        sheet.GetRow(113).CreateCell(1).SetCellValue((!proveedorActual.OperacionesMercantiles) ? "X" : " ");

                        //XSSFFormulaEvaluator.EvaluateAllFormulaCells(excelPackage);
                        //workbook.getCreationHelper().createFormulaEvaluator().evaluateAll();
                        //excelPackage.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();\
                        //wb.setForceFormulaRecalculation(true);
                        excelPackage.SetForceFormulaRecalculation(true);

                        MemoryStream streamOutput = new MemoryStream();
                        excelPackage.Write(streamOutput);
                        return Convert.ToBase64String(streamOutput.ToArray());
                    };
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
    }
}
