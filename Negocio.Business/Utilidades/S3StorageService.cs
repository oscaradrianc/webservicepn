using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Business.Utilidades
{
    public class S3StorageService : IStorageService
    {
        /*
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3StorageService(IAmazonS3 s3Client, string bucketName)
        {
            _s3Client = s3Client;
            _bucketName = bucketName;
        }

        public Task<Stream> GetFileStreamAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        public async Task<string> SaveFileAsync(string folderPath, string fileName, Stream fileStream, string contentType)
        {
            var key = $"{folderPath}/{fileName}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(request);

            return $"https://{_bucketName}.s3.amazonaws.com/{key}";
        }

        */
        public Task<Stream> GetFileStreamAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<string> SaveFileAsync(string folderPath, string fileName, Stream fileStream, string contentType)
        {
            throw new NotImplementedException();
        }
    }


}
