using System;
using System.Collections.Generic;

namespace MemoryCacheManager
{
    internal class StoragedCache
    {
        public object[] Parameters;
        public object Value;
        public DateTime ExpirationDate;
    }
}
