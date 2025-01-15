using System.IO;

namespace PCXL
{
    public class LASFileReader
    {
        public static byte[] StreamingAssetsReadBytes( string filename )
        {
            byte[] bytes = ReadAllBytes( StreamingAssetsPathCombine( filename ) );
            return bytes;
        }

        public static byte[] ReadAllBytes( string path )
        {
            return File.ReadAllBytes( path );
        }

        public static string StreamingAssetsPathCombine( string filename )
        {
            return Path.Combine( UnityEngine.Application.streamingAssetsPath, filename );
        }

        public static bool IsValidPath( string path )
        {
            if ( !File.Exists( path ) || !path.Contains( ".las" ) )
            {
                UnityEngine.Debug.LogError( "Trying to open a file that does not exist or is not a .las -file!" );
                return false;
            }
            return true;
        }
    }
}