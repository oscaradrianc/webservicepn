namespace Negocio.Model
{
    public class ClaseValor
    {
        //CLAS_CLASE, CLVA_CLASEVALOR, CLVA_CODIGOVALOR, CLVA_VALOR, CLVA_ESTADO 
        public int Clase { get; set; }
        public int IdClaseValor { get; set; }
        public int CodigoValor { get; set; }
        public string Valor { get; set; }
        public string Estado { get; set; }
        public string Descripcion { get; set; }
        public int LogsUsuario { get; set; }

    }
}