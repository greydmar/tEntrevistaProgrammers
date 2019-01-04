using System;

namespace testProgrammers.CRUD.Core
{
    public abstract class DisposableObject: IDisposable
    {
        private bool _disposed;

        ~DisposableObject()
        {
            DisposeInternal(false);
        }

        public void Dispose()
        {
            DisposeInternal(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void EnsureNotDisposed(string message=null)
        {
            if (_disposed)
                throw new ObjectDisposedException(message);
        }

        protected void DisposeInternal(bool disposing)
        {
            Dispose(disposing);
            _disposed = true;
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}