using System;
using System.Collections.Generic;
using KX2d.Core.Sprite;
using KX2d.Editor.Atlas;
using KX2d.Editor.Sprite;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Rect = UnityEngine.Rect;

/*************************************************** 
 * Copyright (c) 2013 KaiXin All Rights Reserved 
 * author name      : cab 
 * author email     : anbao@corp.kaixin001.com
 * appVersion       : 1.0 
*****************************************************/
namespace KX2d
{
    /// <summary>
    /// 贴图打包工具
    /// </summary>
    [CustomEditor(typeof(SpriteAtlasData))]
    public class SpriteAtlasBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            GUILayout.Space(8);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("打开图集编辑器", GUILayout.MinWidth(120)))
            {
                SpriteAtlasData gen = (SpriteAtlasData)target;
                if (gen.name != defaultSpriteAtlasnName)
                {
                    EditorUtility.DisplayDialog("提示", "请修改预设名" + defaultSpriteAtlasnName + "再操作", "Ok");
                }
                else
                {
                    SpriteAtlasEditorPopup v = EditorWindow.GetWindow(typeof(SpriteAtlasEditorPopup), false, "图集编辑器") as SpriteAtlasEditorPopup;
                    v.SetGenerator(gen);
                    v.Show();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.Space(8);
        }

        private const string defaultSpriteAtlasnName = "atlasPrefab";
        [MenuItem("Assets/Create SpriteAtlasData Prefab")]
        public static void CreateSpriteAtlas()
        {
            string path = GameEditorUtility.CreateNewPrefab(defaultSpriteAtlasnName);
            if (path.Length != 0)
            {
               
                SpriteAtlasData spriteAtlasData = ScriptableObject.CreateInstance<SpriteAtlasData>();
                spriteAtlasData.version = SpriteAtlasData.CURRENT_VERSION;

                AssetDatabase.CreateAsset(spriteAtlasData,path);
                // Select object
                Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(SpriteAtlasData));
            }
        }

        public static bool Build(SpriteAtlasProxy gen)
        {
            if (gen == null) return false;
            string prefabDirName = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(gen.obj)) + "/";
            //强制导图设置配置
            int numTexturesReimported = 0;
            for (int i = gen.spriteDataList.Count - 1; i >= 0 ; i--)
            {
                Texture2D curTexture = AssetDatabase.LoadAssetAtPath(prefabDirName + gen.spriteDataList[i].name + ".png", typeof(Texture2D)) as Texture2D;
                string thisTexturePath = AssetDatabase.GetAssetPath(curTexture);
                if (!String.IsNullOrEmpty(thisTexturePath))
                {
                   
                    if (ConfigureSpriteTextureImporter(thisTexturePath))
                    {
                        numTexturesReimported++;
                        AssetDatabase.ImportAsset(thisTexturePath);
                    }

                    //去除透明
                    if (gen.disableTrimming)
                    {
                        gen.spriteDataList[i].bound = new Rect(0, 0, curTexture.width, curTexture.height);
                    }
                    else
                    {
                      
                        gen.spriteDataList[i].bound = GameEditorUtility.GetTextureBound(curTexture);
                    }

                    gen.spriteDataList[i].sourceWidth = curTexture.width;
                    gen.spriteDataList[i].sourceHeight = curTexture.height;
                }
                else
                {
                    gen.spriteDataList.RemoveAt(i);
                }
            }
            if (numTexturesReimported > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            //计算小图排列
            bool forceAtlasSize = gen.forceTextureSize;
            int atlasWidth = forceAtlasSize ? gen.forcedTextureWidth : gen.maxTextureSize;
            int atlasHeight = forceAtlasSize ? gen.forcedTextureHeight : gen.maxTextureSize;
            bool forceSquareAtlas = forceAtlasSize ? false : gen.forceSquareAtlas;
            bool allowFindingOptimalSize = !forceAtlasSize;
            bool allowRotation = !gen.disableRotation;

            Builder atlasBuilder = new Builder(atlasWidth, atlasHeight, gen.allowMultipleAtlases ? 64 : 1, allowFindingOptimalSize, forceSquareAtlas, allowRotation);

            for (int i = 0; i < gen.spriteDataList.Count; ++i)
            {
                Rect curRect = gen.spriteDataList[i].bound;
                atlasBuilder.AddRect((int)curRect.width - (int)curRect.x + gen.padding, (int)curRect.height - (int)curRect.y + gen.padding);
            }

            if (atlasBuilder.Build() != 0)
            {
                if (atlasBuilder.HasOversizeTextures())
                {
                    EditorUtility.DisplayDialog("提示", "小图片太大超过图集尺寸,图集放不下,请修改小图片尺寸", "Ok");
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "图片太多了，图集放不下,请减少图片数量", "Ok");
                }
                return false;
            }

            //拼出大图
            KX2d.Editor.Atlas.Data[] atlasData = atlasBuilder.GetAtlasData();
            gen.atlasTextures = new Texture2D[atlasData.Length];
            gen.atlasMaterials = new Material[atlasData.Length];
            for (short atlasIndex = 0; atlasIndex < atlasData.Length; ++atlasIndex)
            {
                Texture2D tex = new Texture2D(atlasData[atlasIndex].width, atlasData[atlasIndex].height,
                    TextureFormat.ARGB32, false);
                tex.SetPixels(new Color[tex.height * tex.width]);

                gen.atlasWastage = (1.0f - atlasData[0].occupancy) * 100.0f;
                gen.atlasWidth = atlasData[0].width;
                gen.atlasHeight = atlasData[0].height;

                for (int i = 0; i < atlasData[atlasIndex].entries.Length; ++i)
                {
                    var entry = atlasData[atlasIndex].entries[i];
                    SpriteAtlasData.SpriteData sd = gen.spriteDataList[entry.index];
                    Texture2D source = AssetDatabase.LoadAssetAtPath(prefabDirName + sd.name + ".png", typeof(Texture2D)) as Texture2D;
                    Color[] newColors = gen.disableTrimming ? source.GetPixels() : GameEditorUtility.GetTextureColor(source, sd.bound);
                    int width = (int) sd.bound.width - (int) sd.bound.x;
                    int height = (int) sd.bound.height - (int) sd.bound.y;

                    sd.regionX = entry.x;
                    sd.regionY = entry.y;
                    sd.regionW = width;
                    sd.regionH = height;
                    sd.padding = gen.padding;
                    sd.atlasIndex = atlasIndex;

                    if (!entry.flipped)
                    {
                        tex.SetPixels(entry.x + gen.padding, entry.y + gen.padding, width, height, newColors);
//                        for (int y = 0; y < height; ++y)
//                        {
//                            for (int x = 0; x < width; ++x)
//                            {
//                                tex.SetPixel(entry.x + x, entry.y + y, newColors[y * width + x]);
//                            }
//                        }
                    }
                    else
                    {
                        //翻转
                        for (int y = 0; y < height; ++y)
                        {
                            for (int x = 0; x < width; ++x)
                            {
                                tex.SetPixel(entry.x + y + gen.padding, entry.y + x + gen.padding, newColors[y * width + x]);
                            }
                        }
                    }
                }
                tex.Apply();

                //贴图生产
                string texturePath = prefabDirName + "atlas" + atlasIndex + ".png";
                BuildDirectoryToFile(texturePath);

                byte[] bytes = tex.EncodeToPNG();
                System.IO.FileStream fs = System.IO.File.Create(texturePath);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
                
                Object.DestroyImmediate(tex);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                //材质生产
                //CreateAssetBundle.SetTextureSetting(true, texturePath);
                tex = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
                Material mat = new Material(Shader.Find("Unlit/Transparent Colored"));
                mat.mainTexture = tex;
                string materialPath = prefabDirName + "atlas" + atlasIndex + "material.mat";
                BuildDirectoryToFile(materialPath);

                AssetDatabase.CreateAsset(mat, materialPath);
                AssetDatabase.SaveAssets();

                gen.atlasTextures[atlasIndex] = tex;
                gen.atlasMaterials[atlasIndex] = mat;
               
            }

            DeleteUnusedAssets(gen.atlasTextures, gen.obj.atlasTextures);
            DeleteUnusedAssets(gen.atlasMaterials, gen.obj.atlasMaterials);

            gen.CopyToTarget(gen.obj);

            EditorUtility.SetDirty(gen.obj);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

        static void DeleteUnusedAssets<T>(T[] newAssets, T[] oldAssets) where T : UnityEngine.Object
        {
            if (oldAssets == null) return;
            foreach (T asset in oldAssets)
            {
                bool found = false;
                foreach (T t in newAssets)
                {
                    if (t == asset)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    UnityEditor.AssetDatabase.DeleteAsset(UnityEditor.AssetDatabase.GetAssetPath(asset));
            }
        }

        static void BuildDirectoryToFile(string localPath)
        {
            string basePath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(basePath + localPath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
        }

        public static bool ConfigureSpriteTextureImporter(string assetPath)
        {

            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);
            if (importer == null) return false;
            if (importer.textureType != TextureImporterType.Advanced ||
                importer.textureFormat != TextureImporterFormat.AutomaticTruecolor ||
                importer.npotScale != TextureImporterNPOTScale.None ||
                importer.isReadable != true ||
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1)
 !importer.alphaIsTransparency ||
#endif
 importer.maxTextureSize < 4096)
            {
                importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                importer.textureType = TextureImporterType.Advanced;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.isReadable = true;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 4096;
#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1)
                importer.alphaIsTransparency = true;
#endif

                return true;
            }

            return false;
        }
    }
}
