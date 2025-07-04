using Devart.Data.Oracle;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Business;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using System.Threading.Tasks;

namespace SWNegocio.Controllers
{
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class FormatoController : ControllerBase
    {
        private readonly IArchivoExcel _excelBusiness;
        public FormatoController(IArchivoExcel formatoFinanciero)
        {
            _excelBusiness = formatoFinanciero;
        }

        [HttpPost]
        [Route("registrar")]
        public IActionResult RegistrarFormato(ArchivoExcel request)
        {
            try
            {
               //Registra el archivo de formato financiero en el sistema
               _excelBusiness.RegistrarArchivo(request);
               return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("formato_proveedor")]
        public async Task<IActionResult> ObtenerFormato(int idProveedor)
        {
            try
            {
                //OracleMonitor myMonitor = new OracleMonitor();
                
                //myMonitor.IsActive = true;
                //Obtiene el formato de proveedor
                string formatoProveedor = await _excelBusiness.ObtenerFormatoProveedor(idProveedor);
                return Ok(new {  Formato = formatoProveedor });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        [Route("formato_proveedor_json")]
        public async Task<IActionResult> ObtenerFormatoJson([FromBody] Proveedor proveedor)
        {
            try
            {
                //OracleMonitor myMonitor = new OracleMonitor();

                //myMonitor.IsActive = true;
                //Obtiene el formato de proveedor
                string formatoProveedor = await _excelBusiness.ObtenerFormatoProveedorJson(proveedor);
                return Ok(new { Formato = formatoProveedor });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}