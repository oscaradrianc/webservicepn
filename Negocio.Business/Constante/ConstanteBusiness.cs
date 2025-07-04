using Microsoft.Extensions.Configuration;
using Negocio.Data;
using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Negocio.Business
{
    public class ConstanteBusiness : IConstante
    {
        private readonly IConfiguration _configuration;
        private readonly IUtilidades _utilidades;

        public ConstanteBusiness(IConfiguration configuracion, IUtilidades utilidades)
        {
            _configuration = configuracion;
            _utilidades = utilidades;
        }

        public async Task<List<POGECONSTANTE>> GetConstante()
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return await Task.Run(() => (from p in cx.POGECONSTANTEs select p).ToList());
            }
        }

        public async Task<POGECONSTANTE> GetConstante(int IdConstante)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                return await Task.Run(() => (from p in cx.POGECONSTANTEs
                                             where p.CONSCONSTANTE == IdConstante
                                             select p).SingleOrDefault());
            }
        }

        public async Task<ResponseStatus> InsertConstante(POGECONSTANTE constante)
        {
            //int codigo = 0;
            ResponseStatus resp = new ResponseStatus();

            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();

                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        //Valido si la referencia ya existe
                        var existeConstante = cx.POGECONSTANTEs.Where(c => c.CONSREFERENCIA.Trim().ToUpper() == constante.CONSREFERENCIA.Trim().ToUpper()).SingleOrDefault();

                        if (existeConstante != null)
                        {
                            resp.Status = Configuracion.StatusError;
                            resp.Message = "Ya existe una constante con la misma referencia.";
                        }
                        else
                        {
                            constante.CONSCONSTANTE = _utilidades.GetSecuencia("SECU_POGECONSTANTE", cx);
                            constante.LOGSFECHA = DateTime.Now;
                            cx.POGECONSTANTEs.InsertOnSubmit(constante);
                            cx.SubmitChanges();
                            await Task.Run(() => dbContextTransaction.Commit());

                            resp.Status = Configuracion.StatusOk;
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        resp.Status = Configuracion.StatusError;
                        resp.Message = $"Error al insertar constante { ex.Message }";
                    }
                }
            }

            return resp;
        }

        public async Task UpdateConstante(int id, POGECONSTANTE constante)
        {
            using (PORTALNEGOCIODataContext cx = new PORTALNEGOCIODataContext())
            {
                cx.Connection.Open();
                using (var dbContextTransaction = cx.Connection.BeginTransaction())
                {
                    try
                    {
                        var query = cx.POGECONSTANTEs.Where(x => x.CONSCONSTANTE == id).FirstOrDefault();

                        if (query == null)
                        {
                            throw new KeyNotFoundException($"No se encontro constante con el id: { id }");
                        }
                        else
                        {
                            //Valido si la referencia ya existe
                            var existeConstante = cx.POGECONSTANTEs.Where(c => c.CONSREFERENCIA.Trim().ToUpper() == constante.CONSREFERENCIA.Trim().ToUpper() && c.CONSCONSTANTE != id).SingleOrDefault();

                            if (existeConstante != null)
                            {
                                await Task.Run(() => dbContextTransaction.Rollback());
                                throw new Exception("Ya existe una registro de constate con la referencia ingresasa");
                            }
                            else
                            {
                                query.CONSREFERENCIA = constante.CONSREFERENCIA;
                                query.CONSDESCRIPCION = constante.CONSDESCRIPCION;
                                query.CONSVALOR = constante.CONSVALOR;
                                query.LOGSUSUARIO = constante.LOGSUSUARIO;
                                query.LOGSFECHA = DateTime.Now;
                            }

                            cx.SubmitChanges();
                            await Task.Run(() => dbContextTransaction.Commit());
                        }
                    }
                    catch (KeyNotFoundException dx)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        throw new KeyNotFoundException("Error al actualizar usuario " + dx.Message);
                    }
                    catch (Exception ex)
                    {
                        await Task.Run(() => dbContextTransaction.Rollback());
                        throw new Exception("Error al actualizar usuario " + ex.Message);
                    }
                }
            }
        }
    }
}
