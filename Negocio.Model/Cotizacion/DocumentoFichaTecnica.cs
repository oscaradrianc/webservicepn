namespace Negocio.Model
{
    public class DocumentoFichaTecnica
    {
        public int CodigoDetalle { get; set; }
        public string CodigoCatalogo { get; set; }
        public string NombreCatalogo { get; set; }
        public int CodigoDocumento { get; set; }
        public string NombreDocumento { get; set; }
        public int CodigoBlob { get; set; }
        public string DataB64 { get; set; }             
    }  
}
