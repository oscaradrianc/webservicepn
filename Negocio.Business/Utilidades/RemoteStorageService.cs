using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business.Utilidades
{
    public class RemoteStorageService // : IStorageService
    {
        private readonly string _serverBasePath;
        private readonly NetworkCredential _credentials;
        private readonly HttpClient _httpClient;
        public RemoteStorageService(HttpClient httpClient, string serverBasePath, NetworkCredential credentials)
        {
            _serverBasePath = serverBasePath;
            _credentials = credentials;
        }
        Task<Stream> GetFileStreamAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        async Task<string> SaveFileAsync(string folderPath, string fileName, Stream fileStream, string contentType)
        {
            var fullPath = $"{_serverBasePath}/{folderPath}/{fileName}";

            using var content = new StreamContent(fileStream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            var response = await _httpClient.PutAsync(fullPath, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error al subir el archivo: {response.ReasonPhrase}");
            }

            return fullPath; // Devuelve la URL del archivo
        }
    }
}
