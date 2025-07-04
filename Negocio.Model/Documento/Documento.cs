using Negocio.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Negocio.Model
{
    public class Documento
    {
        public static Expression<Func<PONEDOCUMENTO, Documento>> FromPONE_DOCUMENTO
        {
            get
            {
                return d => new Documento
                {
                    CodigoDocumento = (int)d.DOCUDOCUMENTO,
                    Nombre = d.DOCUNOMBRE,
                    Tipo = (int)d.CLASTIPODOCUMENTO8,
                    //DataB64 = null,
                    //CodigoBlob = (int)d.BLOBBLOB,
                    FechaCreacion = d.DOCUFECHACREACION,
                    NombreTipo = null,
                    IdUsuario = (int)d.LOGSUSUARIO,
                    Ruta = d.DOCURUTA,
                    ContentType = d.DOCUCONTENTTYPE


                };
            }
        }

        public int CodigoDocumento { get; set; }
        public string Nombre { get; set; }
        public int Tipo { get; set; }
        public string DataB64 { get; set; }
        public DateTime FechaCreacion { get; set; }
        //public int CodigoBlob { get; set; }
        public string NombreTipo { get; set; }
        public int IdUsuario { get; set; }
        public string Ruta { get; set; }
        public string ContentType { get; set; }
        public static List<Documento> CrearDocumentos(List<PONEDOCUMENTO> original)
        {
            List<Documento> lta = new List<Documento>();
            foreach (PONEDOCUMENTO item in original)
            {
                var func = FromPONE_DOCUMENTO.Compile();
                lta.Add(func(item));
            }
            return lta;
        }

    }
}
