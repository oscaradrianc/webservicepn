
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Negocio.Data;

namespace Negocio.Model
{
    public class Accionista
    {
        public static Expression<Func<PONEDETALLEACCIONISTA, Accionista>> FromPONE_DETALLEACCIONISTA
        {
            get
            {
                return a => new Accionista
                {
                    NombreAccionista = a.DEACNOMBRE,
                    IdentificacionAccionista = a.DEACIDENTIFICACION,
                    TipoDocumentoAccionista = (int)a.CLASTIPOIDENTIFICACION,
                    ParticipacionAccionista = a.DEACPORCENTAJE
                };
            }
        }

        public string NombreAccionista { get; set; }
        public int TipoDocumentoAccionista { get; set; }
        public string IdentificacionAccionista { get; set; }
        public double? ParticipacionAccionista { get; set; }
        public static List<Accionista> CrearAccionista(List<PONEDETALLEACCIONISTA> original)
        {
            List<Accionista> lta = new List<Accionista>();
            foreach (PONEDETALLEACCIONISTA item in original)
            {
                var func = FromPONE_DETALLEACCIONISTA.Compile();
                lta.Add(func(item));
            }
            return lta;
        }

    }
}
