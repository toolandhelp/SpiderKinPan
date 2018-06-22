using System;

namespace Kinpan.DAL
{    
    #region
    public class BaseServiceEf<T> where T : IDisposable, new()
    {
        public T _context;

        public BaseServiceEf()
        {
            if (_context == null)
            {
                _context = new T();
            }
        }
        public void Dispose()
        {
            if (_context != null)
                _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    #endregion
}
