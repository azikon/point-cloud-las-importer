using UnityEngine;

namespace PCXL
{
    public class LASImporterStreamingAssets : LASImporter
    {
        private string m_FilesPath;

        public void Import( string streamingAssetFileName, Transform parent, int pointsSkip, bool useFirstPointAsAnchor )
        {
            FileName = streamingAssetFileName;
            m_FilesPath = LASFileReader.StreamingAssetsPathCombine( streamingAssetFileName );
            m_PointsSkip = pointsSkip;
            m_UseFirstPointAsAnchor = useFirstPointAsAnchor;

            m_Dispatcher = LASImporterDispatcher.Create();

            m_LASGameObject = LASRendererHelper.CreateDefaultGameObject( streamingAssetFileName, parent );
            m_MeshFilter = LASRendererHelper.CreateDefaultMeshFilter( m_LASGameObject );
            MeshRenderer meshRenderer = LASRendererHelper.CreateDefaultMeshRenderer( m_LASGameObject );
            meshRenderer.material = LASRendererHelper.CreateDefaultMaterial();

            Initialize();
        }

        public override void ReadPoints()
        {
            m_PointReader.ReadPointsAsync( m_FilesPath );
        }

        public override void ReadPointsSuccess( LASDataHeader_1_2 header, LASDataBody_1_2 body )
        {
            Mesh mesh = LASRendererHelper.CreateDefaultMesh( header, body );
            m_MeshFilter.mesh = mesh;
        }
    }
}