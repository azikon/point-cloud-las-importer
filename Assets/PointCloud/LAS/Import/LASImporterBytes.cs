using UnityEngine;

namespace PCXL
{
    public class LASImporterBytes : LASImporter
    {
        private byte[] m_Bytes;

        public void Import( byte[] bytes, string name, Transform parent, int pointsSkip, bool useFirstPointAsAnchor )
        {
            Debug.Log( "LAS :: Import" );

            m_Bytes = bytes;
            FileName = name;
            m_PointsSkip = pointsSkip;
            m_UseFirstPointAsAnchor = useFirstPointAsAnchor;

            m_Dispatcher = LASImporterDispatcher.Create();

            m_LASGameObject = LASRendererHelper.CreateDefaultGameObject( name, parent );
            m_MeshFilter = LASRendererHelper.CreateDefaultMeshFilter( m_LASGameObject );
            MeshRenderer meshRenderer = LASRendererHelper.CreateDefaultMeshRenderer( m_LASGameObject );
            meshRenderer.material = LASRendererHelper.CreateDefaultMaterial();

            Initialize();
        }

        public override void ReadPoints()
        {
            Debug.Log( "LAS :: ReadPoints" );

            m_PointReader.ReadPointsAsync( m_Bytes );
        }

        public override void ReadPointsSuccess( LASDataHeader_1_2 header, LASDataBody_1_2 body )
        {
            //Mesh mesh = LASRendererHelper.CreateDefaultMesh( header, body );
            //m_MeshFilter.mesh = mesh;
        }
    }
}