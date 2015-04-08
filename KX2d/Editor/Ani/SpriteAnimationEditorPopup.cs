using System.Collections.Generic;
using System.Linq;
using KX2d.Core.Sprite;
using KX2d.Editor.Sprite;
using UnityEditor;
using UnityEngine;

namespace KX2d.Editor.Ani
{
    public class SpriteAnimationEditorPopup : EditorWindow
    {
        private SpriteAnimationData _spriteAnimationData;
        public SpriteAnimationData SpriteAnimationData { get { return _spriteAnimationData; } }
        private int leftBarWidth = 220;
        private List<string> mDelNames = new List<string>();
        private string selection = null;
        private List<SpriteAnimationData.ActionData> allAction = new List<SpriteAnimationData.ActionData>();
        public SpriteAnimationEditorTextureView textureView;
        public SpriteAnimationEditorSettingView settingView;
        public SpriteAnimationEditorTimelineView timeLineView;
        public SpriteAnimationData.ActionData CurActionData = null;
        public bool isPlaying = false;
        private string dir;

        public void SetGenerator(SpriteAnimationData spriteAnimationData)
        {
            this._spriteAnimationData = spriteAnimationData;
            allAction = new List<SpriteAnimationData.ActionData>();
            for (int i = 0; i < spriteAnimationData.ActionList.Count(); i++)
            {
                allAction.Add(spriteAnimationData.ActionList[i]);
            }
            dir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(spriteAnimationData)) + "/";
        }

        void OnEnable()
        {
            textureView = new SpriteAnimationEditorTextureView();
            settingView = new SpriteAnimationEditorSettingView(this);
            timeLineView = new SpriteAnimationEditorTimelineView(this);
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            DrawToolBar();
            GUILayout.BeginHorizontal();

            DrawActionList();

            GUILayout.BeginVertical();

            if (this.isPlaying)
            {
                PreviewAni();
            }

            if (timeLineView != null)
            {
                timeLineView.Draw();
            }
            GUILayout.Space(1);
            GUILayout.BeginHorizontal();
           
            if (textureView != null)
            {
               
                textureView.Draw();
            }

            if (settingView != null)
            {
                
                settingView.Draw();
            }
            GUILayout.EndHorizontal();



            GUILayout.EndVertical();



            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawToolBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

            // LHS
            GUILayout.BeginHorizontal(GUILayout.Width(leftBarWidth - 6));
            if (GUILayout.Button("创建动作", EditorStyles.toolbarButton) && SpriteAnimationData != null)
            {
                CreateAction();
            }
            //GUILayout.Label("动作列表");
            GUILayout.EndHorizontal();

            // Label
            if (SpriteAnimationData != null)
            {
                GUILayout.Label("动画名:" + SpriteAnimationData.name);
            }

            if (isPlaying == false)
            {
                if (GUILayout.Button("▶", EditorStyles.toolbarButton) && CurActionData != null)
                {
                    isPlaying = true;
                    frameTime = 1.0f / CurActionData.fps;
                    startTime = 0;
                    totalTime = 0;
                    curFrame = 0;
                    totalFrame = CurActionData.FrameList.Length;
                }
            }
            else
            {
                if (GUILayout.Button("■", EditorStyles.toolbarButton) && CurActionData != null)
                {
                    isPlaying = false;
                }
            }

            // RHS
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("导出动画", EditorStyles.toolbarButton) && SpriteAnimationData != null)
            {
                BuildAni();
            }

            GUILayout.EndHorizontal();
        }

        private Vector2 actionListScroll = Vector2.zero;
        private void DrawActionList()
        {
            Texture2D blackTexture = new Texture2D(1, 1);
            blackTexture.SetPixel(0, 0, Color.black);
            blackTexture.Apply();
            GUI.DrawTexture(new Rect(leftBarWidth, 16, 1f, Screen.height - 16), blackTexture);


            actionListScroll = GUILayout.BeginScrollView(actionListScroll, GUILayout.Width(leftBarWidth), GUILayout.ExpandHeight(true));
            bool delete = false;

            for (int i = 0; i < allAction.Count; i++)
            {
                SpriteAnimationData.ActionData actionData = allAction[i];
                if (actionData == null) continue;
                bool highlight = selection == actionData.name;
                GUI.backgroundColor = highlight ? Color.green : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;
                GUILayout.Label(i.ToString(), GUILayout.Width(24f));
                if (GUILayout.Button(actionData.name, "OL TextField", GUILayout.Height(20f)))
                {
                    selection = actionData.name;
                    CurActionData = actionData;
                    settingView.Draw();
                    timeLineView.Draw();
                }
                if (mDelNames.Contains(actionData.name))
                {
                    GUI.backgroundColor = Color.red;

                    if (GUILayout.Button("删除", GUILayout.Width(60f)))
                    {
                        delete = true;
                    }
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("X", GUILayout.Width(22f)))
                    {
                        mDelNames.Remove(actionData.name);
                        delete = false;
                    }
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    // If we have not yet selected a sprite for deletion, show a small "X" button
                    if (GUILayout.Button("X", GUILayout.Width(22f))) mDelNames.Add(actionData.name);
                }


                GUILayout.EndHorizontal();
            }


            if (delete)
            {
                for (int i = allAction.Count - 1; i >= 0; i--)
                {
                    SpriteAnimationData.ActionData actionData = allAction[i];
                    if (actionData != null && mDelNames.Contains(actionData.name))
                    {
                        allAction.RemoveAt(i);
                        if (CurActionData == actionData)
                        {
                            CurActionData = null;
                            settingView.Draw();
                            timeLineView.Draw();
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

        
        public Texture2D GetTexture(string name)
        {
            return AssetDatabase.LoadAssetAtPath(dir + name + ".png", typeof(Texture2D)) as Texture2D; 
        }

        private void CreateAction()
        {
            SpriteAnimationData.ActionData actionData = new SpriteAnimationData.ActionData();
            actionData.name = UniqueActionName("NewAction");
            allAction.Add(actionData);
            selection = actionData.name;
            CurActionData = actionData;
            settingView.Draw();
            timeLineView.Draw();
        }

        private string UniqueActionName(string baseName)
        {
            bool found = false;
            for (int i = 0; i < allAction.Count; ++i)
            {
                if (allAction[i].name == baseName)
                {
                    found = true;
                    break;
                }
            }
            if (!found) return baseName;

            string uniqueName = baseName + " ";
            int uniqueId = 1;
            for (int i = 0; i < allAction.Count; ++i)
            {
                string uname = uniqueName + uniqueId.ToString();
                if (allAction[i].name == uname)
                {
                    uniqueId++;
                    i = -1;
                    continue;
                }
            }
            uniqueName = uniqueName + uniqueId.ToString();
            return uniqueName;
        }

        private float startTime;
        private float totalTime; // 记录播放的时间 
        private float frameTime; // 每一帧的时间（1 / fps）
        private int curFrame;
        private int totalFrame;
        private void PreviewAni()
        {
            if (CurActionData != null && isPlaying)
            {
                totalTime += Time.deltaTime;
                if ((totalTime - startTime) >= frameTime)
                {
                    startTime = totalTime;
                    this.timeLineView.selectedFrame = curFrame;
                    curFrame++;
                    if (curFrame >= totalFrame)
                    {
                        curFrame = 0;
                    }
                }
            }
        }


        private void BuildAni()
        {
            SpriteAnimationData.ActionList = allAction.ToArray();
            EditorUtility.SetDirty(SpriteAnimationData);
        }
    }
}
