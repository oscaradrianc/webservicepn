using System;
using System.Collections.Generic;
using Negocio.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Negocio.Model;

namespace Negocio.Business
{
    public class ParametroGeneral : IParametroGeneral
    {
        private readonly IUtilidades _utilidades;
        public ParametroGeneral(IUtilidades utilidades)
        {
            _utilidades = utilidades;
        }

        public async Task<List<POGECLASE>> ObtenerClases()
        {
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();
            var query = from c in cx.POGECLASEs
                        select c;

            return await Task.FromResult(query.ToList());
        }

        public async Task<POGECLASEVALOR> ObtenerClaseValor(int idClaseValor)
        {
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();

            return await Task.FromResult((from c in cx.POGECLASEVALORs select c).Where(c => c.CLVACLASEVALOR == idClaseValor).SingleOrDefault());
        }

        public async Task<List<POGECLASEVALOR>> ObtenerClaseValorPorClase(int idClase)
        {
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();

            return await Task.FromResult((from c in cx.POGECLASEVALORs select c).Where(c => c.CLASCLASE == idClase).ToList());
        }

        public async Task<ResponseStatus> InsertarClaseValor(POGECLASEVALOR claseValor)
        {
            ResponseStatus resp = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();

                var cV = cx.POGECLASEVALORs.Where(c => c.CLASCLASE == claseValor.CLASCLASE && c.CLVACODIGOVALOR == claseValor.CLVACODIGOVALOR).SingleOrDefault();

                if(cV != null)
                {                    
                    resp.Status = Configuracion.StatusError;
                    resp.Message = $"Ya existe el código de valor { claseValor.CLVACODIGOVALOR } para el Id. Parámetro { claseValor.CLASCLASE }";
                }
                else
                {
                    using (var dbContextTransaction = cx.Connection.BeginTransaction())
                    {
                        try
                        {
                            int codigo = _utilidades.GetSecuencia("SECU_POGECLASEVALOR", cx);
                            claseValor.CLVACLASEVALOR = codigo;

                            cx.POGECLASEVALORs.InsertOnSubmit(claseValor);
                            cx.SubmitChanges();
                            await Task.Run(() => dbContextTransaction.Commit());

                            resp.Status = Configuracion.StatusOk;
                        }
                        catch (KeyNotFoundException dx)
                        {
                            await Task.Run(() => dbContextTransaction.Rollback());
                            throw new KeyNotFoundException("Error al actualizar párametro general " + dx.Message);
                        }
                        catch (Exception ex)
                        {
                            await Task.Run(() => dbContextTransaction.Rollback());
                            throw new Exception("Error al actualizar párametro general " + ex.Message);
                        }
                    }
                }               
            }

            return resp;
        }

        public async Task<ResponseStatus> ActualizarClaseValor(int id, POGECLASEVALOR claseValor)
        {
            ResponseStatus resp = new ResponseStatus();
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var cValor = cx.POGECLASEVALORs.Where(c => c.CLVACLASEVALOR == id).SingleOrDefault();

                        if (cValor == null)
                        {                            
                            //throw new KeyNotFoundException($"El id clase valor { id }, no se encontro en la base de datos");
                            resp.Status = Configuracion.StatusError;
                            resp.Message = $"El código clase valor { id }, no se encontro en la base de datos";
                        }

                        cValor.CLVACODIGOVALOR = claseValor.CLVACODIGOVALOR;
                        cValor.CLVADESCRIPCION = claseValor.CLVADESCRIPCION;
                        cValor.CLVAESTADO = claseValor.CLVAESTADO;
                        cValor.CLVAVALOR = claseValor.CLVAVALOR;
                        cValor.LOGSUSUARIO = claseValor.LOGSUSUARIO;
                       
                        cx.SubmitChanges();
                        await Task.Run(() => dbContextTransaction.Commit());

                        resp.Status = Configuracion.StatusOk;
                    }
                    catch (KeyNotFoundException dx)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        throw new KeyNotFoundException("Error al actualizar clase valor " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        throw new Exception("Error al actualizar clase valor " + ex.Message);
                    }

                }
            }

            return resp;
        }
    }
}
