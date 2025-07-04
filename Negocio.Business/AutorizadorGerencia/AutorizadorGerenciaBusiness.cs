using System;
using System.Collections.Generic;
using Negocio.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Negocio.Model;

namespace Negocio.Business
{
    public class AutorizadorGerenciaBusiness : IAutorizadorGerencia
    {
        private readonly IUtilidades _utilidades;
        public AutorizadorGerenciaBusiness(IUtilidades utilidades)
        {
            _utilidades = utilidades;
        }

        public async Task<List<POGEAUTORIZADORGERENCIA>> ObtenerAutorizadores()
        {
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();
            var query = from c in cx.POGEAUTORIZADORGERENCIAs
                        select c;

            return await Task.FromResult(query.ToList());
        }

        public async Task<List<POGEAUTORIZADORGERENCIA>> ObtenerAutorizadores(int idGerencia)
        {
            using PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext();
            var query = from c in cx.POGEAUTORIZADORGERENCIAs
                        where c.IDGERENCIA == idGerencia
                        select c;

            return await Task.FromResult(query.ToList());
        }

        public async Task<ResponseStatus> InsertarAutorizadorGerencia(POGEAUTORIZADORGERENCIA autorizador)
        {
            ResponseStatus resp = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();

                var cV = cx.POGEAUTORIZADORGERENCIAs.Where(a => a.IDGERENCIA == autorizador.IDGERENCIA && a.USUAUSUARIO == autorizador.USUAUSUARIO).SingleOrDefault();

                if (cV != null)
                {
                    resp.Status = Configuracion.StatusError;
                    resp.Message = $"Ya existe el autorizador para la gerencia";
                }
                else
                {
                    using (var dbContextTransaction = cx.Connection.BeginTransaction())
                    {
                        try
                        {
                            autorizador.LOGSFECHA = DateTime.Now;
                            autorizador.AUGEAUTORIZADORGERENCIA = _utilidades.GetSecuencia("SECU_POGEAUTORIZADORGERENCIA", cx);

                            cx.POGEAUTORIZADORGERENCIAs.InsertOnSubmit(autorizador);
                            cx.SubmitChanges();
                            await Task.Run(() => dbContextTransaction.Commit());

                            resp.Status = Configuracion.StatusOk;
                        }
                        catch (KeyNotFoundException dx)
                        {
                            await Task.Run(() => dbContextTransaction.Rollback());
                            throw new KeyNotFoundException("Error al insertar autorizador " + dx.Message);
                        }
                        catch (Exception ex)
                        {
                            await Task.Run(() => dbContextTransaction.Rollback());
                            throw new Exception("Error al insertar autorizador " + ex.Message);
                        }
                    }
                }
            }

            return resp;
        }

        public async Task<ResponseStatus> EliminarAutorizadorGerencia(int idAutorizarGerencia, int idUsuarioElimina)
        {
            ResponseStatus result = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var autorizador = cx.POGEAUTORIZADORGERENCIAs.SingleOrDefault(a => a.AUGEAUTORIZADORGERENCIA == idAutorizarGerencia);

                        if (autorizador != null)
                        {
                            //Primero actualizo el campo logsusuario con el valor del usuario que esta borrando
                            //Para que la auditoria tome en id del usuario que esta realizando la accion.
                            autorizador.LOGSUSUARIO = idUsuarioElimina;

                            cx.SubmitChanges();

                            cx.POGEAUTORIZADORGERENCIAs.DeleteOnSubmit(autorizador);
                            cx.SubmitChanges();
                            result.Status = Configuracion.StatusOk;
                        }
                        else
                        {
                            result.Status = Configuracion.StatusError;
                            result.Message = "No existe el registro indicado";
                        }

                        await Task.Run(() => dbContextTransaction.Commit());
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        result.Status = Configuracion.StatusError;
                        result.Message = "Error al eliminar autorizador de gerencia";
                    }
                }
            }

            return result;
        }
    }
}
