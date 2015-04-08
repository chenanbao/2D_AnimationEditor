using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KX2d
{
    /// <summary>
    /// 编辑器工具类
    /// </summary>
    public class GameEditorUtility
    {
        static public Texture2D blankTexture
        {
            get
            {
                return EditorGUIUtility.whiteTexture;
            }
        }

        static void DeleteUnusedAssets<T>(List<T> oldAssets, T[] newAssets) where T : UnityEngine.Object
        {
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

        public static string CreateNewPrefab(string name) // name is the filename of the prefab EXCLUDING .prefab
        {
            Object obj = Selection.activeObject;
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (assetPath.Length == 0)
            {
                assetPath = SaveFileInProject("Create...", "Assets/", name, "prefab");
            }
            else
            {
                // is a directory
                string path = System.IO.Directory.Exists(assetPath) ? assetPath : System.IO.Path.GetDirectoryName(assetPath);
                assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".prefab");
            }

            return assetPath;
        }

        public static string SaveFileInProject(string title, string directory, string filename, string ext)
        {
            string path = EditorUtility.SaveFilePanel(title, directory, filename, ext);
            if (path.Length == 0) // cancelled
                return "";
            string cwd = System.IO.Directory.GetCurrentDirectory().Replace("\\", "/") + "/assets/";
            if (path.ToLower().IndexOf(cwd.ToLower()) != 0)
            {
                path = "";
                EditorUtility.DisplayDialog(title, "Assets must be saved inside the Assets folder", "Ok");
            }
            else
            {
                path = path.Substring(cwd.Length - "/assets".Length);
            }
            return path;
        }

        public enum DragDirection
        {
            Horizontal,
        }
        // Size is the offset into the rect to draw the DragableHandle
        const float resizeBarHotSpotSize = 2.0f;
        public static float DragableHandle(int id, Rect windowRect, float offset, DragDirection direction)
        {
            int controlID = GUIUtility.GetControlID(id, FocusType.Passive);

            Vector2 positionFilter = Vector2.zero;
            Rect controlRect = windowRect;
            switch (direction)
            {
                case DragDirection.Horizontal:
                    controlRect = new Rect(controlRect.x + offset - resizeBarHotSpotSize,
                                           controlRect.y,
                                           resizeBarHotSpotSize * 2 + 1.0f,
                                           controlRect.height);
                    positionFilter.x = 1.0f;
                    break;
            }
            EditorGUIUtility.AddCursorRect(controlRect, MouseCursor.ResizeHorizontal);

            if (GUIUtility.hotControl == 0)
            {
                if (Event.current.type == EventType.MouseDown && controlRect.Contains(Event.current.mousePosition))
                {
                    GUIUtility.hotControl = controlID;
                    Event.current.Use();
                }
            }
            else if (GUIUtility.hotControl == controlID)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    Vector2 mousePosition = Event.current.mousePosition;
                    Vector2 handleOffset = new Vector2((mousePosition.x - windowRect.x) * positionFilter.x,
                                                       (mousePosition.y - windowRect.y) * positionFilter.y);
                    offset = handleOffset.x + handleOffset.y;
                    HandleUtility.Repaint();
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                }
            }

            // Debug draw
            // GUI.Box(controlRect, "");

            return offset;
        }

        /// <summary>
        /// 画图边框
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        static public void DrawOutline(Rect rect, Color color)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Texture2D tex = blankTexture;
                GUI.color = color;
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
                GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
                GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
                GUI.color = Color.white;
            }
        }

        /// <summary>
        /// 取图实心大小（去透明）
        /// </summary>
        /// <param name="tex"></param>
        /// <returns></returns>
        public static Rect GetTextureBound(Texture2D tex)
        {
            //(0,0)在tex的左下角
            Color32[] pixels = tex.GetPixels32();
            int xmin = tex.width;
            int xmax = 0;
            int ymin = tex.height;
            int ymax = 0;
            int oldWidth = tex.width;
            int oldHeight = tex.height;
            for (int y = 0, yw = oldHeight; y < yw; ++y)
            {
                for (int x = 0, xw = oldWidth; x < xw; ++x)
                {
                    Color32 c = pixels[y * xw + x];

                    if (c.a != 0)
                    {
                        if (y < ymin) ymin = y;
                        if (y > ymax) ymax = y;
                        if (x < xmin) xmin = x;
                        if (x > xmax) xmax = x;
                    }
                }
            }
            return new Rect(xmin, ymin, xmax, ymax);
        }

        /// <summary>
        /// 获取制定区域图
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Color[] GetTextureColor(Texture2D tex, Rect rect)
        {
            Color[] newColors = new Color[((int)rect.width - (int)rect.x +1) * ((int)rect.height - (int)rect.y +1)];
            int index = 0;
            Color32[] pixels = tex.GetPixels32();
            for (int y = (int)rect.y; y < rect.height; y++)
            {
                for (int x = (int)rect.x; x < rect.width; x++)
                {
                    Color32 c = pixels[y * tex.width + x];
                    newColors[index] = c;
                    index++;
                }
            }
            return newColors;
        }
    }
}
