using Negocio.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface INoticias
    {
        Task<List<Noticias>> ListNoticias();
        Task<Noticias> ConsultarNoticiaPorId(int id);
        Task<ResponseStatus> ActualizarNoticia(decimal id, Noticias noticia);
    }
}
