using Negocio.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Negocio.Model
{
    public class Especialidad
    {
        public static Expression<Func<PONEESPECIALIDADPROVEEDOR, Especialidad>> FromPONE_ESPECIALIDADPROVEEDOR
        {
            get
            {
                return e => new Especialidad
                {
                    SecuenciaEspecialidad = (int)e.LIESSECUENCIA,
                    BienesServiciosEspecialidad = e.LIESBIENESOSERVICIOS,
                    ComercioEspecialidad = e.LIESCOMERCIO == "S" ? true : false,
                    ServiciosEspecialidad = e.LIESSERVICIOS == "S" ? true : false,
                    ManufacturaEspecialidad = e.LIESMANUFACTURA == "S" ? true : false,
                    GravadaEspecialidad = e.LIESGRAVADA == "S" ? true : false,
                    LogsUsuario = (int?)e.LOGSUSUARIO
                };
            }
        }

        public int SecuenciaEspecialidad { get; set; }
        public string ItemEspecialidad { get; set; }
        public string BienesServiciosEspecialidad { get; set; }
        public bool ComercioEspecialidad { get; set; }
        public bool ServiciosEspecialidad { get; set; }
        public bool ManufacturaEspecialidad { get; set; }
        public bool GravadaEspecialidad { get; set; }
        public int? LogsUsuario { get; set; }
        public string EstadoRegistro { get; set; }
        public static List<Especialidad> CrearEspecialidad(List<PONEESPECIALIDADPROVEEDOR> original)
        {
            List<Especialidad> lta = new List<Especialidad>();
            foreach (PONEESPECIALIDADPROVEEDOR item in original)
            {
                var func = FromPONE_ESPECIALIDADPROVEEDOR.Compile();
                lta.Add(func(item));
            }
            return lta;
        }

    }
}
