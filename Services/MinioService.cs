using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel.Args;

namespace AuthApi.Services
{
    public class MinioService(IConfiguration configuration, ILogger<MinioService> logger, IMinioClient minioClient)
    {

        private readonly IConfiguration configuration = configuration;
        private readonly ILogger<MinioService> logger = logger;
        private readonly IMinioClient minioClient = minioClient;


        public async Task EnsureBuketIsCreated(string bucketName)
        {
            var beArgs = new BucketExistsArgs().WithBucket( bucketName);
            var found = await minioClient.BucketExistsAsync(beArgs);
            if(!found) {
                var mbArgs = new MakeBucketArgs().WithBucket( bucketName );
                await minioClient.MakeBucketAsync(mbArgs);
                logger.LogInformation("Bucket {buketName} created", bucketName);
            }
        }

        
        public async Task<string> UploadFile(string filePath, Stream stream, string? bucketName = null)
        {
            // * upload the file
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName ?? configuration["MinioSettings:BucketName"] )
                .WithObject(filePath)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("application/octet-stream");
            var response = await minioClient.PutObjectAsync(putObjectArgs);

            return response.ObjectName;
        }

        public async Task<string> UploadFile(string originalName, Stream stream, string path = "", string? bucketName = null)
        {
            // * make file path
            var filePath = string.Format("{0}.{1}", Guid.NewGuid().ToString(), originalName.Split(".").Last() );
            if( !string.IsNullOrEmpty(path)){
                filePath = Path.Join(path, filePath);
            }
            
            // * upload the file
            var putObjectArgs = new PutObjectArgs()
                .WithBucket( bucketName ?? configuration["MinioSettings:BucketName"] )
                .WithObject(filePath)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("application/octet-stream");
            var response = await minioClient.PutObjectAsync(putObjectArgs);

            return response.ObjectName;
        }

        public async Task<string> UploadFile(string originalName, Stream stream, Guid guid, string path = "", string? bucketName = null)
        {
            // * make file path
            var filePath = string.Format("{0}.{1}", guid.ToString(), originalName.Split(".").Last() );
            if( !string.IsNullOrEmpty(path)){
                filePath = Path.Join(path, filePath);
            }
            
            // * upload the file
            var putObjectArgs = new PutObjectArgs()
                .WithBucket( bucketName ?? configuration["MinioSettings:BucketName"] )
                .WithObject(filePath)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("application/octet-stream");
            var response = await minioClient.PutObjectAsync(putObjectArgs);

            return response.ObjectName;
        }

        public async Task<string?> MakeTemporalUrl(string fileName, string mimmeType, string? bucketName = null)
        {
            // URL expiration time (in seconds)
            int expiryDurationInSeconds = Convert.ToInt32(configuration["MinioSettings:ExpiryDuration"]);
            string? presignedUrl = null;
            try {
                // Generate a presigned URL for the file
                var reqParams = new Dictionary<string, string>(StringComparer.Ordinal) {
                    { "response-content-type", mimmeType }
                };
                var presignedGetObjectArgs = new PresignedGetObjectArgs()
                    .WithBucket(bucketName ?? configuration["MinioSettings:BucketName"])
                    .WithObject(fileName)
                    .WithExpiry(expiryDurationInSeconds)
                    .WithHeaders(reqParams);
                presignedUrl = await minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
            }catch(Exception err){
                logger.LogError(err, "Fail at generate the temporally url for the file {fileName}", fileName);
            }

            return presignedUrl;
        }

        public async Task RemoveFiles(IEnumerable<string> fileNames, string? bucketName = null)
        {
            var tasks = fileNames.Select( async fileName => {
                var removeArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName ?? configuration["MinioSettings:BucketName"])
                    .WithObject(fileName);
                await minioClient.RemoveObjectAsync(removeArgs);
            });

            await Task.WhenAll( tasks.ToArray() );
        }

        public async Task<bool> FileExist(string filePath, string bucketName)
        {
            var stateArgs = new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(filePath);
            
            Minio.DataModel.ObjectStat? objectMinio = null;
            try
            {
                objectMinio = await minioClient.StatObjectAsync(stateArgs);
            }
            catch (System.Exception)
            {
                return false;
            }

            return objectMinio != null;
        }
    }
}