using Negocio.Model;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business.Utilidades
{
    public static class ExcelUtilities
    {
        /// <summary>
        /// Lee archivo de excel a partir de una cadena base 64
        /// </summary>
        /// <param name="archivoBase64">contenido archivo excel</param>
        /// <param name="hojas">cantidad de hojas a leer</param>
        /// <param name="conEncabezado">Si el archivo contiene encabezado, este se excluira</param>
        /// <returns></returns>
        public static List<ValoresArchivo> LeerArchvoExcel(this string archivoBase64, int hojas, bool conEncabezado = true)
        {
            List<ValoresArchivo> valores = new List<ValoresArchivo>();
            //lee el archivo de excel de un B64

            string fileType = archivoBase64.Split(',')[0];

            switch (fileType)
            {
                case "data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64": //".xlsx":
                    valores = LeerXlsx(archivoBase64, hojas, conEncabezado);
                    break;
                    case "data:application/vnd.ms-excel;base64": //".xls":
                    valores = LeerXls(archivoBase64, hojas, conEncabezado);
                    break;
            }

            return valores;           
        }

        private static List<ValoresArchivo> LeerXlsx(string archivoBase64, int hojas, bool conEncabezado = true)
        {
            List<ValoresArchivo> valores = new List<ValoresArchivo>();
            //lee el archivo de excel de un B64
            string b64string = archivoBase64.Split(',').Length > 1 ? archivoBase64.Split(',')[1] : archivoBase64;
                        
            byte[] bin = Convert.FromBase64String(b64string);

            //Crea el Archivo en Memoria
            using (MemoryStream stream = new MemoryStream(bin))
            {
                XSSFWorkbook excelPackage = new XSSFWorkbook(stream);
            
                int sheetIndex = 0;
                //itera todas las hojas del documento
                while (sheetIndex < hojas)
                {
                    ISheet worksheet = excelPackage.GetSheetAt(sheetIndex);                   

                    if (worksheet == null) return null;

                    int lastRow = worksheet.LastRowNum;
                    int firstRow = conEncabezado ? 1 : 0;
                    //itera todas las filas
                    for (int i = firstRow; i <= lastRow; i++)
                    {
                        int lastColumn = worksheet.GetRow(i).LastCellNum;
                        //itera todas las columnas de la fila
                        for (int j = 0; j <= lastColumn; j++)
                        {
                            //Valida la celda segun la configuracion en la base de datos
                            ICell celda = worksheet.GetRow(i).GetCell(j, MissingCellPolicy.CREATE_NULL_AS_BLANK);

                            if (celda == null) continue;

                            ValoresArchivo valor = new ValoresArchivo();
                            valor.celda = celda?.Address.FormatAsString();
                            valor.hoja = sheetIndex;
                            valor.valor = celda.CellType == CellType.Numeric ? celda.NumericCellValue.ToString() : celda.StringCellValue;
                            valores.Add(valor);

                        }
                    }

                    sheetIndex++;
                }

                return valores;
            }
        }

        private static List<ValoresArchivo> LeerXls(string archivoBase64, int hojas, bool conEncabezado = true)
        {
            List<ValoresArchivo> valores = new List<ValoresArchivo>();
            //lee el archivo de excel de un B64
            string b64string = archivoBase64.Split(',').Length > 1 ? archivoBase64.Split(',')[1] : archivoBase64;

            byte[] bin = Convert.FromBase64String(b64string);

            //Crea el Archivo en Memoria
            using (MemoryStream stream = new MemoryStream(bin))
            {
                HSSFWorkbook excelPackage = new HSSFWorkbook(stream);

                int sheetIndex = 0;
                //itera todas las hojas del documento
                while (sheetIndex < hojas)
                {
                    ISheet worksheet = excelPackage.GetSheetAt(sheetIndex);

                    if (worksheet == null) return null;

                    int lastRow = worksheet.LastRowNum;
                    int firstRow = conEncabezado ? 1 : 0;
                    //itera todas las filas
                    for (int i = firstRow; i <= lastRow; i++)
                    {
                        int lastColumn = worksheet.GetRow(i).LastCellNum;
                        //itera todas las columnas de la fila
                        for (int j = 0; j <= lastColumn; j++)
                        {
                            //Valida la celda segun la configuracion en la base de datos
                            ICell celda = worksheet.GetRow(i).GetCell(j, MissingCellPolicy.CREATE_NULL_AS_BLANK);

                            if (celda == null) continue;

                            ValoresArchivo valor = new ValoresArchivo();
                            valor.celda = celda?.Address.FormatAsString();
                            valor.hoja = sheetIndex;
                            valor.valor = celda.CellType == CellType.Numeric ? celda.NumericCellValue.ToString() : celda.StringCellValue;
                            valores.Add(valor);

                        }
                    }

                    sheetIndex++;
                }

                return valores;
            }
        }

        public static List<ValoresArchivo> ValidarArchivoExcel(
            Func<List<ValoresArchivo>, List<ConfiguracionArchivo>, List<ValoresArchivo>> validar,
            List<ValoresArchivo> valoresArchivo, 
            List<ConfiguracionArchivo> configuracionArchivo = null)
        {
            return validar(valoresArchivo, configuracionArchivo);
        }

        public static List<ValoresArchivo> ValidarArchivoExcelCotizacion(
            int codigoSolicitud,
            Func<int, List<ValoresArchivo>, List<ConfiguracionArchivo>, List<ValoresArchivo>> validarCotizacion,
            List<ValoresArchivo> valoresArchivo,
            List<ConfiguracionArchivo> configuracionArchivo = null)
        {
            return validarCotizacion(codigoSolicitud, valoresArchivo, configuracionArchivo);
        }
    }
}
