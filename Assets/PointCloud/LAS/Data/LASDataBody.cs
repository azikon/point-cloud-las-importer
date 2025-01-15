using System.Collections.Generic;

using UnityEngine;

namespace PCXL
{
    public class LASDataBody_1_2
    {
        public List<Vector3> vertices;
        public List<Color> colors;

        public LASDataBody_1_2( int vertexCount )
        {
            vertices = new List<Vector3>( vertexCount );
            colors = new List<Color>( vertexCount );
        }

        public void AddPoint(
            float x, float y, float z,
            float r, float g, float b, float a
        )
        {
            vertices.Add( new Vector3( x, y, z ) );
            colors.Add( new Color( r, g, b, a ) );
        }
    }
}