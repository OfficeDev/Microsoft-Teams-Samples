using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPUploader.Repositories
{
    public interface IAuthProvider
    {
        Task<string> GetAccessTokenAsync();
    }
}
