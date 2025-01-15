using System;
using System.Collections.Concurrent;

using UnityEngine;

namespace PCXL
{
    [ExecuteInEditMode]
    public class LASImporterDispatcher : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> m_ExecutionQueue = new();

        public void Enqueue( Action action )
        {
            lock ( m_ExecutionQueue )
            {
                m_ExecutionQueue.Enqueue( action );
            }
        }

        void Update()
        {
            while ( m_ExecutionQueue.Count > 0 && m_ExecutionQueue.TryDequeue( out var executionAction ) )
            {
                executionAction?.Invoke();
            }
        }

        public static LASImporterDispatcher Create()
        {
            GameObject dispatcherGameObject = new( "LASImporterDispatcher" );
            return dispatcherGameObject.AddComponent<LASImporterDispatcher>();
        }

        public static void Destroy( LASImporterDispatcher importerDispatcher )
        {
            if ( importerDispatcher && importerDispatcher.gameObject )
            {
                UnityEngine.Object.DestroyImmediate( importerDispatcher.gameObject );
            }
        }
    }
}