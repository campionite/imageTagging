﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace QvcImageTagger.Common.Services
{
    public interface IStorageService
    {
        Task UploadFileAsync(string fileName, byte[] bytes, string contentType);

        Task<IDictionary<string, string>> GetMetadataFromFileAsync(string fileName);

        Task SetMetadataOnFileAsync(string fileName, IDictionary<string, string> metadata);
    }
}
