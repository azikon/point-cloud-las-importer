using UnityEngine;

namespace PCXL
{
    public class LASCreateTest : MonoBehaviour
    {
        [SerializeField] private string m_FileName = "";
        [SerializeField] private int m_PointsSkip = 50;
        [SerializeField] private bool m_UseFirstPointAsAnchor = true;
        [SerializeField] private TextAsset m_TextAsset = null;

        private void Start()
        {
            CreateFromBytes();
        }

        private void CreateFromBytes()
        {
            Debug.Log( "LAS :: CreateFromBytes" );

            LASImporterBytes importer = new();
            importer.Import( m_TextAsset.bytes, m_TextAsset.name, this.transform, m_PointsSkip, m_UseFirstPointAsAnchor );
            importer.OnOperationsEnd += ( importer, result ) =>
            {
                Debug.LogError( "OnOperationsEnd" );
                importer.LASGameObject.transform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity );
                importer.LASGameObject.transform.localScale = Vector3.one;

                if ( result == true )
                {
                }
            };
            importer.OnSuccess += ( header, body ) =>
            {
                Debug.LogError( "OnSuccess" );

                Mesh mesh = LASRendererHelper.CreateDefaultMesh( header, body );
                importer.MeshFilter.mesh = mesh;

                // Alternative render method
                //LASRenderer renderer = importer.LASGameObject.AddComponent<LASRenderer>();
                //renderer.Initialize( header, body );
            };
        }

        private void CreateFromStreamingAssets()
        {
            Debug.Log( "LAS :: CreateFromStreamingAssets" );

            LASImporterStreamingAssets importer = new();
            importer.Import( m_FileName, this.transform, m_PointsSkip, m_UseFirstPointAsAnchor );
            importer.OnOperationsEnd += ( importer, result ) =>
            {
                Debug.Log( "OnOperationsEnd" );
                importer.LASGameObject.transform.SetLocalPositionAndRotation( Vector3.zero, Quaternion.identity );
                importer.LASGameObject.transform.localScale = Vector3.one;
                if ( result == true )
                {
                }
            };
            importer.OnSuccess += ( header, body ) =>
            {
                Mesh mesh = LASRendererHelper.CreateDefaultMesh( header, body );
                importer.MeshFilter.mesh = mesh;
            };
        }
    }
}