using Newtonsoft.Json;


namespace Negocio.Model
{
    public class Response<T>
    {
        [JsonProperty("status")]
        public ResponseStatus Status { get; set; }
        [JsonProperty("data")]
        public T Data { get; set; }
    }    
}