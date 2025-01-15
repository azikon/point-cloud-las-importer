using System;

using UnityEngine;

namespace PCXL
{
    public abstract class LASImporter
    {
        public string FileName;
        public float ProgressValue;

        public Action<LASDataHeader_1_2, LASDataBody_1_2> OnSuccess = null;
        public Action<string> OnError = null;
        public Action<LASImporter, bool> OnOperationsEnd;
        public Action<LASImporter, float> OnProgress;

        protected int m_PointsSkip = 25;
        protected bool m_UseFirstPointAsAnchor = true;

        protected LASPointsReader m_PointReader;
        protected GameObject m_LASGameObject;
        protected MeshFilter m_MeshFilter;
        protected LASImporterDispatcher m_Dispatcher;

        public GameObject LASGameObject => m_LASGameObject;
        public MeshFilter MeshFilter => m_MeshFilter;

        protected void Initialize()
        {
            Debug.Log( "LAS :: Initialize" );

            m_PointReader?.Dispose();
            m_PointReader = new LASPointsReader( m_PointsSkip, m_UseFirstPointAsAnchor, ReadSuccessAsync, ReadErrorAsync, ReadProgressAsync );
            ReadPoints();
        }

        public abstract void ReadPoints();

        private void ReadSuccessAsync( LASDataHeader_1_2 header, LASDataBody_1_2 body )
        {
            Debug.Log( "LAS :: ReadSuccess" );

            m_Dispatcher?.Enqueue
            (
                () =>
                {
                    ReadPointsSuccess( header, body );

                    NotifySuccess( header, body );

                    NotifyOperationsEnd( true );

                    LASImporterDispatcher.Destroy( m_Dispatcher );
                }
            );
        }

        private void ReadErrorAsync( string error )
        {
            Debug.Log( "LAS :: ReadError" );

            m_Dispatcher?.Enqueue
            (
                () =>
                {
                    Debug.Log( "Import error : " + error );

                    NotifyError( error );

                    NotifyOperationsEnd( false );

                    LASImporterDispatcher.Destroy( m_Dispatcher );
                }
            );
        }

        private void ReadProgressAsync( float percent )
        {
            ProgressValue = percent;
            //m_Dispatcher?.Enqueue
            //(
            //    () =>
            //    {
            //        NotifyProgress( percent );
            //        Debug.Log( "Import progress : " + percent );
            //    }
            //);
        }

        private void NotifySuccess( LASDataHeader_1_2 header, LASDataBody_1_2 body )
        {
            OnSuccess?.Invoke( header, body );
        }

        private void NotifyError( string error )
        {
            OnError?.Invoke( error );
        }

        private void NotifyProgress( float percent )
        {
            OnProgress?.Invoke( this, percent );
        }

        private void NotifyOperationsEnd( bool state )
        {
            OnOperationsEnd?.Invoke( this, state );
        }

        public abstract void ReadPointsSuccess( LASDataHeader_1_2 header, LASDataBody_1_2 body );
    }
}