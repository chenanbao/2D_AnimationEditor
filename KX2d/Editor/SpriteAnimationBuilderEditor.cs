using System.IO;
using KX2d.Core.Sprite;
using KX2d.Editor.Ani;
using UnityEditor;
using UnityEngine;

namespace KX2d
{
    [CustomEditor(typeof(SpriteAnimationData))]
    public class SpriteAnimationBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("打开动画编辑器", GUILayout.MinWidth(120)))
            {
                SpriteAnimationData gen = (SpriteAnimationData)target;
                if (gen.name != defaultSpriteAnimationName)
                {
                    EditorUtility.DisplayDialog("提示", "请修改预设名" + defaultSpriteAnimationName + "再操作", "Ok");
                }
                else
                {
                    SpriteAnimationEditorPopup v = EditorWindow.GetWindow(typeof(SpriteAnimationEditorPopup), false, "动画编辑器") as SpriteAnimationEditorPopup;
                    
                    v.SetGenerator(gen);
                    v.Show();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.Space(8);
        }

        private const string defaultSpriteAnimationName = "aniPrefab";
        [MenuItem("Assets/Create SpriteAnimationData Prefab")]
        public static void CreateSpriteCollection()
        {
            string path = GameEditorUtility.CreateNewPrefab(defaultSpriteAnimationName);
            if (path.Length != 0)
            {


                SpriteAnimationData spriteAnimationData = ScriptableObject.CreateInstance<SpriteAnimationData>();
                spriteAnimationData.version = SpriteAnimationData.CURRENT_VERSION;
                SetAtlas(path, spriteAnimationData);
               

                AssetDatabase.CreateAsset(spriteAnimationData, path);
                // Select object
                Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(SpriteAnimationData));
            }
        }

        private static void SetAtlas(string path, SpriteAnimationData spriteAnimationData)
        {
            DirectoryInfo info = new DirectoryInfo(Path.GetDirectoryName(path));
            FileInfo[] files = info.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.FullName.EndsWith("prefab")) //only prefab
                {
                    string fullPath = getAssetPath(file.FullName);
                    SpriteAtlasData objSpriteAtlasData = AssetDatabase.LoadAssetAtPath(fullPath, typeof(SpriteAtlasData)) as SpriteAtlasData;
                    if (objSpriteAtlasData != null)
                    {
                        spriteAnimationData.SpriteAtlasData = objSpriteAtlasData;
                    }
                }
            }
        }

        static string getAssetPath(string fullPath)
        {
            fullPath = fullPath.Replace('\\', '/');
            return fullPath.Replace(Application.dataPath, "Assets");
        }
    }
}
