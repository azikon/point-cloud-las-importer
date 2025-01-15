using System;
using System.Collections.Generic;
using System.Threading;

namespace PCXL
{
    public abstract class ThreadScheduler : IDisposable
    {
        public const int MAX_CONCURRENT_THREADS = 3;
        protected volatile bool _cancel = false;
        private static readonly List<Thread> _threads = new();
        private static readonly object _lock = new();

        public void Dispose()
        {
            _cancel = true;
        }

        protected void QueueThread( Thread t )
        {
            _threads.Add( t );
            if ( RunningThreads < MAX_CONCURRENT_THREADS )
            {
                t.Start();
            }
        }

        protected int RunningThreads
        {
            get
            {
                int running = 0;
                for ( int i = 0; i < _threads.Count; i++ )
                {
                    if ( _threads[ i ].IsAlive && _threads[ i ].ThreadState.HasFlag( ThreadState.Running ) )
                    {
                        running++;
                    }
                }

                return running;
            }
        }

        protected void TryStartNextThread()
        {
            for ( int i = 0; i < _threads.Count; i++ )
            {
                if ( _threads[ i ].ThreadState.HasFlag( ThreadState.Unstarted ) )
                {
                    _threads[ i ].Start();
                    break;
                }
            }
        }

        protected void ThreadFinished( Thread t )
        {
            lock ( _lock )
            {
                _threads.Remove( t );
                TryStartNextThread();
            }
        }
    }
}