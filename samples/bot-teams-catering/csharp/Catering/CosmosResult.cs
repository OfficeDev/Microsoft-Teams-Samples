using System.Collections.Generic;

namespace Catering
{
    public class CosmosResult<T>
    {
        public string ContinuationToken { get; set; }
        public IList<T> Items { get; set; }
    }
}
