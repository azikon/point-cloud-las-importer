using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.Rendering;

namespace PCXL
{
    public class LASRendererHelper
    {
        public static Mesh CreateDefaultMesh( LASDataHeader_1_2 header, LASDataBody_1_2 body )
        {
            try
            {
                var mesh = new Mesh
                {
                    name = "point_cloud_mesh",
                    indexFormat = body.vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
                };
                mesh.SetVertices( body.vertices );
                mesh.SetColors( body.colors );
                mesh.SetIndices( Enumerable.Range( 0, body.vertices.Count ).ToArray(), MeshTopology.Points, 0 );
                mesh.UploadMeshData( false );
                return mesh;
            }
            catch ( Exception e )
            {
                Debug.LogError( "Failed importing " + "point_cloud_mesh" + ". " + e.Message );
                return null;
            }
        }

        public static Material CreateDefaultMaterial()
        {
            Material material = new( Shader.Find( "Point Cloud/QuadTransparent" ) ); //new( Shader.Find( "Point Cloud/Quad" ) );
            return material;
        }

        public static GameObject CreateDefaultGameObject( string name, Transform parent )
        {
            string withoutExtension = Path.GetFileNameWithoutExtension( name );
            GameObject createdGameObject = new( withoutExtension );
            if ( parent != null )
            {
                createdGameObject.transform.SetParent( parent );
            }
            return createdGameObject;
        }

        public static MeshFilter CreateDefaultMeshFilter( GameObject targetGameObject )
        {
            MeshFilter createdMeshFilter = targetGameObject.AddComponent<MeshFilter>();
            return createdMeshFilter;
        }

        public static MeshRenderer CreateDefaultMeshRenderer( GameObject targetGameObject )
        {
            MeshRenderer createdMeshRenderer = targetGameObject.AddComponent<MeshRenderer>();
            return createdMeshRenderer;
        }

        public static Mesh CreateQuadMesh()
        {
            return new()
            {
                vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, 0f),
                    new Vector3(-0.5f, 0.5f, 0f),
                    new Vector3(0.5f, 0.5f, 0f),
                    new Vector3(0.5f, -0.5f, 0f),
                },
                triangles = new[]
                {
                    0, 1, 2, 0, 2, 3,
                }
            };
        }
    }
}