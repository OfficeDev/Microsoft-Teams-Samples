using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPUploader.Repositories
{
    public interface ISharepointRepository
    {
        Task<T[]> GetAllItemsAsync<T>();

        Task<bool> UploadFileToSPAsync(string fileLocation);
    }
}
