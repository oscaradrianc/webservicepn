using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business
{
    public interface IStorageService
    {
        Task<string> SaveFileAsync(string folderPath, string fileName, Stream fileStream, string contentType);
        Task<Stream> GetFileStreamAsync(string filePath); // Nuevo método
        bool ExistsDirectory();
    }
}
