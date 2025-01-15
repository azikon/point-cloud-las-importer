using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace PCXL
{
#if UNITY_EDITOR
    public class LASImporterEditor : EditorWindow
    {
        public List<string> InputFileNames = new();
        public int PointsSkipAmount = 15;
        public bool UseFirstPointAsAnchor = true;

        private Vector2 m_Scrollpos;
        private Transform m_Parent = null;
        private bool m_IsLockImportButton;
        private Dictionary<LASImporter, float> m_ProgressValues;

        private SerializedObject serializedObject;
        private SerializedProperty fileNamesProp;
        private SerializedProperty pointsSkipProp;
        private SerializedProperty anchorToFirstPointProp;

        [MenuItem( "Point Cloud/LAS Importer" )]
        private static void Init()
        {
            LASImporterEditor window = ( LASImporterEditor )EditorWindow.GetWindow( typeof( LASImporterEditor ) );
            window.titleContent.text = "LAS Importer";
            window.Show();
        }

        private void OnEnable()
        {
            serializedObject = new SerializedObject( this );

            fileNamesProp = serializedObject.FindProperty( "InputFileNames" );
            pointsSkipProp = serializedObject.FindProperty( "PointsSkipAmount" );
            anchorToFirstPointProp = serializedObject.FindProperty( "UseFirstPointAsAnchor" );

            InputFileNames = new();
            m_ProgressValues = new();
        }

        public void OnGUI()
        {
            m_Scrollpos = EditorGUILayout.BeginScrollView( m_Scrollpos );
            EditorGUILayout.LabelField( "\n" );

            EditorGUILayout.LabelField( ".las files in StreamingAssets:" );
            EditorGUILayout.PropertyField( fileNamesProp, true );
            if ( GUILayout.Button( "Add file" ) )
            {
                string path = EditorUtility.OpenFilePanel( "select .las file", "Assets/StreamingAssets", "las" );
                string[] filepath = path.Split( '/' );
                InputFileNames.Add( filepath[ ^1 ] );
            }
            EditorGUILayout.LabelField( "\n" );

            EditorGUILayout.LabelField( "Amount of points to skip in between readings:" );
            EditorGUILayout.PropertyField( pointsSkipProp, true );
            EditorGUILayout.LabelField( "\n" );

            EditorGUILayout.LabelField( "Whether to center the first point to zero and use it as origin:" );
            EditorGUILayout.PropertyField( anchorToFirstPointProp, true );
            EditorGUILayout.LabelField( "\n" );

            pointsSkipProp.intValue = Mathf.Clamp( pointsSkipProp.intValue, 0, 10000 );

            if ( GUILayout.Button( "Add to selected gameobject" ) && m_IsLockImportButton == false )
            {
                if ( Selection.transforms != null && Selection.transforms.Length > 0 )
                {
                    m_Parent = Selection.transforms[ 0 ];
                }

                HandleInputFIles();
            }

            EditorGUILayout.LabelField( "\n" );

            int idx = 0;
            foreach ( var keyval in m_ProgressValues )
            {
                float progress = keyval.Key.ProgressValue == 0 ? 0f : keyval.Key.ProgressValue / 100f;
                Rect rect = EditorGUILayout.BeginVertical();
                if ( progress >= 1f )
                {
                    EditorGUI.ProgressBar( new Rect( 3, rect.y + 21 * idx, position.width - 6, 20 ), progress, "Instancing: " + keyval.Key.FileName );
                }
                else
                {
                    EditorGUI.ProgressBar( new Rect( 3, rect.y + 21 * idx, position.width - 6, 20 ), progress, "Read: " + keyval.Key.FileName + " " + ( ( progress * 100f ).ToString( "0" ) + "%" ) );
                }
                EditorGUILayout.EndVertical();
                idx++;
            }

            EditorGUILayout.EndScrollView();

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        void Update()
        {
            Repaint();
        }

        private void HandleInputFIles()
        {
            m_IsLockImportButton = true;

            foreach ( string filename in InputFileNames )
            {
                LASImporterStreamingAssets importer = new();
                importer.Import( filename, m_Parent, PointsSkipAmount, UseFirstPointAsAnchor );
                importer.OnOperationsEnd += ( importer, result ) =>
                {
                    if ( result == true )
                    {
                    }
                    RemoveProgress( importer );
                };
                importer.OnSuccess += ( header, body ) =>
                {
                    Mesh mesh = LASRendererHelper.CreateDefaultMesh( header, body );
                    importer.MeshFilter.mesh = mesh;
                };

                AddProgress( importer );
            }
            InputFileNames.Clear();

            m_IsLockImportButton = false;
        }

        private void AddProgress( LASImporter importer )
        {
            if ( m_ProgressValues.ContainsKey( importer ) == false )
            {
                m_ProgressValues.Add( importer, 0f );
            }
        }

        private void RemoveProgress( LASImporter importer )
        {
            if ( m_ProgressValues.ContainsKey( importer ) == true )
            {
                m_ProgressValues.Remove( importer );
            }
        }
    }
#endif
}