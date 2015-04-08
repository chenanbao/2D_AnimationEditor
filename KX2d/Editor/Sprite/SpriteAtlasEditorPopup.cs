using System.Collections.Generic;
using KX2d.Core.Sprite;
using UnityEditor;
using UnityEngine;

namespace KX2d.Editor.Sprite
{
    public class SpriteAtlasEditorPopup : EditorWindow
    {
        private SpriteAtlasData _spriteAtlasData;
        private SpriteAtlasProxy _spriteAtlasProxy = null;
        public SpriteAtlasProxy SpriteAtlasProxy { get { return _spriteAtlasProxy; } }
        private SpriteAtlasEditorSettingView settingsView;
        private SpriteAtlasEditorTextureView textureView;

        private Object[] deferredDroppedObjects;
        private int leftBarWidth = 220;
        private List<string> mDelNames = new List<string>();
        private string selection = null;
        private string dir;


        public void SetGenerator(SpriteAtlasData spriteAtlasData)
        {
            this._spriteAtlasData = spriteAtlasData;
            this._spriteAtlasProxy = new SpriteAtlasProxy(spriteAtlasData);
            dir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(spriteAtlasData)) + "/";
        }

        public Texture2D GetTexture(string name)
        {
            return AssetDatabase.LoadAssetAtPath(dir + name + ".png", typeof(Texture2D)) as Texture2D;
        }

        void OnEnable()
        {
            settingsView = new SpriteAtlasEditorSettingView(this);
            textureView = new SpriteAtlasEditorTextureView();
        }


        private void OnGUI()
        {
            GUILayout.BeginVertical();
            DrawToolBar();
            GUILayout.BeginHorizontal();

            DrawSpriteList();

           
            if (textureView != null)
            {
                textureView.Draw();
            }

            if (settingsView != null)
            {
                settingsView.Draw();
            }


            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }


        private void DrawToolBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

            // LHS
            GUILayout.BeginHorizontal(GUILayout.Width(leftBarWidth - 6));

            GUILayout.Label("图列表");

            GUILayout.Space(8);
            GUILayout.EndHorizontal();

            // Label
            if (_spriteAtlasData != null)
            {
                GUILayout.Label("图集名:"+_spriteAtlasData.name);
            }

            // RHS
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("导出图集", EditorStyles.toolbarButton) && _spriteAtlasData != null)
            {
                BuildTexture();
            }

            GUILayout.EndHorizontal();
        }

        private Vector2 spriteListScroll = Vector2.zero;
        private void DrawSpriteList()
        {
            Rect rect = new Rect(0, 0, leftBarWidth, Screen.height);
            if (rect.Contains(Event.current.mousePosition))
            {
                switch (Event.current.type)
                {
                    case EventType.DragUpdated:
                        if (IsValidDragPayload())
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        else
                            DragAndDrop.visualMode = DragAndDropVisualMode.None;
                        break;

                    case EventType.DragPerform:
                        var droppedObjectsList = new List<Object>();
                        for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                        {
                            var type = DragAndDrop.objectReferences[i].GetType();
                            if (type == typeof(Texture2D))
                                droppedObjectsList.Add(DragAndDrop.objectReferences[i]);
                            else if (type == typeof(Object) && System.IO.Directory.Exists(DragAndDrop.paths[i]))
                                droppedObjectsList.AddRange(AddTexturesInPath(DragAndDrop.paths[i]));
                        }
                        deferredDroppedObjects = droppedObjectsList.ToArray();
                        Repaint();
                        break;
                }
            }

            if (Event.current.type == EventType.Layout && deferredDroppedObjects != null)
            {
                HandleDroppedPayload(deferredDroppedObjects);
                deferredDroppedObjects = null;
            }

           

            if (_spriteAtlasProxy != null)
            {
                if (_spriteAtlasProxy.Empty)
                {
                    GUILayout.BeginVertical(GUILayout.Width(leftBarWidth), GUILayout.ExpandHeight(true));
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("拖动贴图到这里");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                    return;
                }
            }
            else
            {
                return;
            }

            //line
            Texture2D blackTexture = new Texture2D(1, 1);
            blackTexture.SetPixel(0, 0, Color.black);
            blackTexture.Apply();
            GUI.DrawTexture(new Rect(leftBarWidth, 16, 1f, Screen.height - 16), blackTexture);
            //Handles.DrawLine(new Vector3(leftBarWidth, 0), new Vector3(leftBarWidth,));

            spriteListScroll = GUILayout.BeginScrollView(spriteListScroll, GUILayout.Width(leftBarWidth), GUILayout.ExpandHeight(true));
            bool delete = false;
            
            for (int spriteIndex = 0; spriteIndex < _spriteAtlasProxy.spriteDataList.Count; ++spriteIndex)
            {
                var sprite = _spriteAtlasProxy.spriteDataList[spriteIndex];
                var spriteSourceTexture = GetTexture(sprite.name); ;
                if (spriteSourceTexture == null && sprite.name.Length == 0) continue;
                bool highlight = selection == sprite.name;
                GUI.backgroundColor = highlight ? Color.green : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;
                GUILayout.Label(spriteIndex.ToString(), GUILayout.Width(24f));
                if (GUILayout.Button(sprite.name, "OL TextField", GUILayout.Height(20f)))
                {
                    selection = sprite.name;
                    textureView.CurTexture = spriteSourceTexture;
                }
                if (mDelNames.Contains(sprite.name))
                {
                    GUI.backgroundColor = Color.red;

                    if (GUILayout.Button("删除", GUILayout.Width(60f)))
                    {
                        delete = true;
                    }
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("X", GUILayout.Width(22f)))
                    {
                        mDelNames.Remove(sprite.name);
                        delete = false;
                    }
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    // If we have not yet selected a sprite for deletion, show a small "X" button
                    if (GUILayout.Button("X", GUILayout.Width(22f))) mDelNames.Add(sprite.name);
                }


                GUILayout.EndHorizontal();

               
            }

            if (delete)
            {
                for (int spriteIndex = _spriteAtlasProxy.spriteDataList.Count - 1; spriteIndex >= 0 ; spriteIndex--)
                {
                    var sprite = _spriteAtlasProxy.spriteDataList[spriteIndex];
                    var spriteSourceTexture = GetTexture(sprite.name);
                    if (spriteSourceTexture == null && sprite.name.Length == 0) continue;
                    if (mDelNames.Contains(sprite.name))
                    {
                        _spriteAtlasProxy.spriteDataList.RemoveAt(spriteIndex);
                        if (textureView.CurTexture == spriteSourceTexture)
                        {
                            textureView.CurTexture = null;
                            textureView.Draw();
                        }
                    }
                }
                mDelNames.Clear();
            }

            GUILayout.EndScrollView();

            Rect viewRect = GUILayoutUtility.GetLastRect();
            leftBarWidth = (int)GameEditorUtility.DragableHandle(4819283,
                viewRect, leftBarWidth,
               GameEditorUtility.DragDirection.Horizontal);
        }

     
        private bool IsValidDragPayload()
        {
            int idx = 0;
            foreach (var v in DragAndDrop.objectReferences)
            {
                var type = v.GetType();
                if (type == typeof(Texture2D))
                    return true;
                else if (type == typeof(Object) && System.IO.Directory.Exists(DragAndDrop.paths[idx]))
                    return true;
                ++idx;
            }
            return false;
        }

        private List<Object> AddTexturesInPath(string path)
        {
            List<Object> localObjects = new List<Object>();
            foreach (var q in System.IO.Directory.GetFiles(path))
            {
                string f = q.Replace('\\', '/');
                System.IO.FileInfo fi = new System.IO.FileInfo(f);
                if (fi.Extension.ToLower() == ".meta")
                    continue;

                Object obj = AssetDatabase.LoadAssetAtPath(f, typeof(Texture2D));
                if (obj != null) localObjects.Add(obj);
            }
            foreach (var q in System.IO.Directory.GetDirectories(path))
            {
                string d = q.Replace('\\', '/');
                localObjects.AddRange(AddTexturesInPath(d));
            }

            return localObjects;
        }

        private void HandleDroppedPayload(Object[] objects)
        {
            if (_spriteAtlasProxy == null) return;
            foreach (var obj in objects)
            {
                Texture2D tex = obj as Texture2D;
                
                if (tex != null)
                {
                    int index = _spriteAtlasProxy.FindSpriteBySource(tex.name);
                    if (index == -1)
                    {
                        //新添
                        int slot = _spriteAtlasProxy.FindOrCreateEmptySpriteSlot();
                        _spriteAtlasProxy.spriteDataList[slot].name = tex.name;
                        //_spriteAtlasProxy.spriteDataList[slot].texture = tex;
                    }
                    else
                    {
                        //替换
                        //_spriteAtlasProxy.spriteDataList[index].texture = tex;
                        if (textureView.CurTexture != null && textureView.CurTexture.name == tex.name)
                        {
                            textureView.CurTexture = tex;
                            textureView.Draw();
                        }
                    }
                }
            }
        }


        private void BuildTexture()
        {
            if (!SpriteAtlasBuilderEditor.Build(_spriteAtlasProxy))
            {
                EditorUtility.DisplayDialog("导出贴图失败",
                    "请检查日志查看错误", "确定");
            }
        }

    }
}
