using System;
using System.IO;

using System.Linq;

using System.Threading;

using UnityEngine;

namespace PCXL
{
    public class LASPointsReader : ThreadScheduler
    {
        public int m_ProgressNotifyAmount = 0;

        private readonly int m_PointsSkip = 0;
        private readonly bool m_UseFirstPointAsAnchor = true;
        private const ushort DEFAULT_COLOR = 49151;

        private readonly Action<LASDataHeader_1_2, LASDataBody_1_2> m_OnSuccess = null;
        private readonly Action<string> m_OnError = null;
        private readonly Action<float> m_OnProgress = null;

        public LASPointsReader( int pointsSkip, bool useFirstPointAsAnchor, Action<LASDataHeader_1_2, LASDataBody_1_2> onSuccess, Action<string> onError = null, Action<float> onProgress = null )
        {
            m_PointsSkip = pointsSkip;
            m_UseFirstPointAsAnchor = useFirstPointAsAnchor;
            m_OnSuccess = onSuccess;
            m_OnError = onError;
            m_OnProgress = onProgress;
        }

        public Thread ReadPointsAsync( string path )
        {
            if ( LASFileReader.IsValidPath( path ) == false )
            {
                NotifyError( "File path is not valid." );
                return null;
            }

            Thread thread = new( () =>
            {
                byte[] bytes = ReadData( path );
                LASDataHeader_1_2 header = ReadHeaderAsync( bytes, m_PointsSkip );
                ReadBody( bytes, m_PointsSkip, header );
                ThreadFinished( Thread.CurrentThread );
            } )
            {
                IsBackground = true
            };

            QueueThread( thread );

            return thread;
        }

        public Thread ReadPointsAsync( byte[] bytes )
        {
            Thread thread = new( () =>
            {
                LASDataHeader_1_2 header = ReadHeaderAsync( bytes, m_PointsSkip );
                ReadBody( bytes, m_PointsSkip, header );
                ThreadFinished( Thread.CurrentThread );
            } )
            {
                IsBackground = true
            };

            QueueThread( thread );

            return thread;
        }

        private LASDataHeader_1_2 ReadHeaderAsync( byte[] bytes, int pointsSkip )
        {
            LASDataHeader_1_2 header = LASDataHeaders.MarshalHeader( bytes, true );
            return header;
        }

        private byte[] ReadData( string path )
        {
            return LASFileReader.ReadAllBytes( path );
        }

        private void ReadBody( byte[] bytes, int pointsSkip, LASDataHeader_1_2 header )
        {
            bytes = bytes.Skip( ( int )header.OffsetToPointData ).ToArray();

            LASDataBody_1_2 dataBody = new( ( int )header.NumberOfPointRecords );
            Vector3 anchorOffset = Vector3.zero;

            int targetAmount = Mathf.FloorToInt( ( float )header.NumberOfPointRecords / ( 1 + pointsSkip ) + 1 );
            //int progressStepAmount = ( int )( header.NumberOfPointRecords / 100f ) * 5;
            int progressStepAmount = ( int )( targetAmount / 100f ) * 5;
            NotifyProgress( 1f );

            using MemoryStream stream = new( bytes );
            using ( BinaryReader reader = new( stream ) )
            {
                for ( int i = 0; i < header.NumberOfPointRecords; i++ )
                {
                    HandleProgressNotify( ( int )header.NumberOfPointRecords, i, progressStepAmount );

                    byte[] pointsBytes = reader.ReadBytes( header.PointDataRecordLength );

                    if ( i == 0 && m_UseFirstPointAsAnchor )
                    {
                        anchorOffset = GetFirstPoint( pointsBytes, header );
                    }

                    if ( i % ( 1 + pointsSkip ) != 0 )
                    {
                        continue;
                    }
                    int x = BitConverter.ToInt32( pointsBytes, 0 );
                    int y = BitConverter.ToInt32( pointsBytes, 4 );
                    int z = BitConverter.ToInt32( pointsBytes, 8 );

                    ushort R = DEFAULT_COLOR;
                    ushort G = DEFAULT_COLOR;
                    ushort B = DEFAULT_COLOR;

                    if ( pointsBytes.Length >= 34 )
                    {
                        R = BitConverter.ToUInt16( pointsBytes, 28 );
                        G = BitConverter.ToUInt16( pointsBytes, 30 );
                        B = BitConverter.ToUInt16( pointsBytes, 32 );
                    }

                    Vector3 pos = new( ( float )( ( x * header.XScaleFactor ) + ( header.XOffset ) ),
                                     ( ( float )( ( y * header.YScaleFactor ) + ( header.YOffset ) ) ),
                                     ( ( float )( ( z * header.ZScaleFactor ) + ( header.ZOffset ) ) ) );

                    pos -= anchorOffset;

                    dataBody.AddPoint( pos.x, pos.y, pos.z, R / 65535f, G / 65535f, B / 65535f, 1f );

                    if ( _cancel )
                    {
                        NotifyError( "Read points thread is canceled." );
                        break;
                    }
                }
            }

            NotifyProgress( 100f );

            NotifySuccess( header, dataBody );
        }

        public static Vector3 GetFirstPoint( byte[] bytes, LASDataHeader_1_2 header )
        {
            int x = BitConverter.ToInt32( bytes, 0 );
            int y = BitConverter.ToInt32( bytes, 4 );
            int z = BitConverter.ToInt32( bytes, 8 );
            Vector3 pos = new( ( float )( ( x * header.XScaleFactor ) + ( header.XOffset ) ),
                             ( ( float )( ( y * header.YScaleFactor ) + ( header.YOffset ) ) ),
                             ( ( float )( ( z * header.ZScaleFactor ) + ( header.ZOffset ) ) ) );
            return pos;
        }

        private void HandleProgressNotify( int numberOfPointRecords, int index, int progressStepAmount )
        {
            if ( index >= m_ProgressNotifyAmount )
            {
                m_ProgressNotifyAmount = index + progressStepAmount;

                if ( m_ProgressNotifyAmount < numberOfPointRecords )
                {
                    float percent = ( float )m_ProgressNotifyAmount / ( float )numberOfPointRecords * 100f;

                    NotifyProgress( percent );
                }
            }
        }

        private void NotifySuccess( LASDataHeader_1_2 header, LASDataBody_1_2 body )
        {
            m_OnSuccess?.Invoke( header, body );
        }

        private void NotifyError( string error )
        {
            m_OnError?.Invoke( error );
        }

        private void NotifyProgress( float percent )
        {
            m_OnProgress?.Invoke( percent );
        }

        //public static Vector3 GetFirstPoint( byte[] bytes, LASDataHeader_1_2 header )
        //{
        //    bytes = bytes.Skip( ( int )header.OffsetToPointData ).ToArray();

        //    using MemoryStream stream = new( bytes );
        //    using BinaryReader reader = new( stream );

        //    byte[] pointsBytes = reader.ReadBytes( header.PointDataRecordLength );

        //    int x = BitConverter.ToInt32( pointsBytes, 0 );
        //    int y = BitConverter.ToInt32( pointsBytes, 4 );
        //    int z = BitConverter.ToInt32( pointsBytes, 8 );

        //    //ushort R = BitConverter.ToUInt16(pointsBytes, 28);
        //    //ushort G = BitConverter.ToUInt16(pointsBytes, 30);
        //    //ushort B = BitConverter.ToUInt16(pointsBytes, 32);

        //    Vector3 pos = new( ( float )( ( x * header.XScaleFactor ) + ( header.XOffset ) ),
        //                     ( ( float )( ( y * header.YScaleFactor ) + ( header.YOffset ) ) ),
        //                     ( ( float )( ( z * header.ZScaleFactor ) + ( header.ZOffset ) ) ) );
        //    return pos;
        //}
    }
}