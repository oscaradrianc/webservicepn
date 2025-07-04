
using Newtonsoft.Json;

namespace Negocio.Model
{
    public class ResponseStatus
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }

        public ResponseStatus()
        {
            this.Status = string.Empty;
            this.Message = string.Empty;
        }
    }
}