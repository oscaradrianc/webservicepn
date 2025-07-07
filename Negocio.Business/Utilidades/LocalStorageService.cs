using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business.Utilidades
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _basePath;

        public LocalStorageService(string basePath)
        {
            //if (string.IsNullOrWhiteSpace(basePath))
            //    throw new ArgumentException($"Error de configuración: La ruta base no puede estar vacía. Validar con el área de TI.", nameof(basePath));

            //if (!Directory.Exists(basePath))
            //    throw new DirectoryNotFoundException($"La ruta base no existe: {basePath}");


            _basePath = basePath;
        }

        public async Task<string> SaveFileAsync(string folderPath, string fileName, Stream fileStream, string contentType)
        {
            // Construir la ruta completa del directorio
            var fullPath = Path.Combine(_basePath, folderPath);

            // Validar si el directorio existe
            if (!Directory.Exists(fullPath))
            {
                // Crear el directorio si no existe
                Directory.CreateDirectory(fullPath);
            }

            // Construir la ruta completa del archivo
            var filePath = Path.Combine(fullPath, fileName);

            // Guardar el archivo en el sistema
            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            return filePath; // Devuelve la ruta completa del archivo
        }

        public async Task<Stream> GetFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Archivo no encontrado.");
            }

            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            memoryStream.Position = 0;
            return memoryStream;
        }

        public Task DeleteFileAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return Task.CompletedTask;
        }

        public async Task<Stream> GetFileStreamAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Archivo no encontrado.");
            }

            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Devuelve true si la ruta de archivo existe
        /// </summary>
        /// <returns></returns>
        public bool ExistsDirectory()
        {
            return !string.IsNullOrWhiteSpace(this._basePath) && (Directory.Exists(this._basePath));
        }
    }
}
