using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.Business
{
    public interface IPreguntas
    {
        List<Preguntas> ListPreguntas(int id);
        List<Preguntas> ListPreguntasSinRespuesta(int id);
        string CrearPregunta(CrearPregunta request);
        string CrearRespuesta(CrearRespuesta request);

    }
}
