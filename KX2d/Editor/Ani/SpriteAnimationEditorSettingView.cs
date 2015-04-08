using KX2d.Core.Sprite;
using KX2d.Editor.Ani;
using UnityEditor;
using UnityEngine;

namespace KX2d.Editor.Sprite
{
    public class SpriteAnimationEditorSettingView
    {
        private SpriteAnimationEditorPopup host;
        private SpriteAnimationData SpriteAnimationData { get { return host.SpriteAnimationData; } }
        private SpriteAnimationData.ActionData CurActionData = null;
        private Vector2 spriteListScroll = Vector2.zero;
        public SpriteAtlasData.SpriteData selection = null;
        public Texture2D CurTexture;

        public SpriteAnimationEditorSettingView(SpriteAnimationEditorPopup host)
        {
            this.host = host;
        }

        public void Draw()
        {
            GUILayout.BeginVertical(GUILayout.Width(300), GUILayout.ExpandHeight(true));
            DrawTextureSettings();
            GUILayout.EndVertical();
        }

        private void DrawTextureSettings()
        {

           

            CurActionData = this.host.CurActionData;
            if (CurActionData == null) return;
            GUI.color = Color.white;
            BeginHeader("动作设置");


            string changedName = EditorGUILayout.TextField("动作名", CurActionData.name).Trim();
            if (changedName != CurActionData.name && changedName.Length > 0)
            {
                CurActionData.name = changedName;
                HandleUtility.Repaint();
            }

            float fps = EditorGUILayout.FloatField("帧率", CurActionData.fps);
            if (fps > 0) CurActionData.fps = fps;
            float clipTime = CurActionData.FrameList.Length / fps;
            float newClipTime = EditorGUILayout.FloatField("时长", clipTime);
            if (newClipTime > 0 && newClipTime != clipTime)
                CurActionData.fps = CurActionData.FrameList.Length / newClipTime;



            EndHeader();

            BeginHeader("图集设置");

            if (SpriteAnimationData.SpriteAtlasData != null)
            {
                string atlasName = SpriteAnimationData.SpriteAtlasData.name;
                EditorGUILayout.LabelField("图集名", atlasName);
                 int iwidth = 256;
                 int iheight = 256;
                Rect rect = GUILayoutUtility.GetRect(iwidth, iheight, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                TextureGrid.Draw(rect);
                GameEditorUtility.DrawOutline(rect, Color.gray);

                if (CurTexture != null)
                {
                    iwidth = (int)rect.width;
                    iheight = (int)rect.height;
                    if (CurTexture.width / CurTexture.height >= iwidth / iheight)
                    {
                        if (CurTexture.width > iwidth)
                        {
                            rect.width = iwidth;
                            rect.height = (CurTexture.height * iwidth) / CurTexture.width;
                        }
                        else
                        {
                            rect.width = CurTexture.width;
                            rect.height = CurTexture.height;
                        }
                    }
                    else
                    {
                        if (CurTexture.height > iheight)
                        {
                            rect.height = iheight;
                            rect.width = (CurTexture.width * iheight) / CurTexture.height;
                        }
                        else
                        {
                            rect.width = CurTexture.width;
                            rect.height = CurTexture.height;
                        }
                    }
                    rect.x += (iwidth - rect.width) / 2;
                    rect.y += (iheight - rect.height) / 2;
                    GUI.DrawTexture(rect, CurTexture);
                }

                GUILayout.Space(1);

                spriteListScroll = GUILayout.BeginScrollView(spriteListScroll, GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true));
                for (int i = 0; i < SpriteAnimationData.SpriteAtlasData.spriteDataList.Length; i++)
                {
                    SpriteAtlasData.SpriteData spriteData = SpriteAnimationData.SpriteAtlasData.spriteDataList[i];
                    if (spriteData == null) continue;
                    bool highlight = selection == spriteData;
                    GUI.backgroundColor = highlight ? Color.green : new Color(0.8f, 0.8f, 0.8f);
                    GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                    GUI.backgroundColor = Color.white;
                    GUILayout.Label(i.ToString(), GUILayout.Width(24f));
                    if (GUILayout.Button(spriteData.name, "OL TextField", GUILayout.Height(20f)))
                    {
                        selection = spriteData;
                    }
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("<+", GUILayout.Width(26f)))
                    {
                        selection = spriteData;
                        host.timeLineView.InsertFrame(selection.name, -1);
                    }
                    if (GUILayout.Button("+>", GUILayout.Width(26f)))
                    {
                        selection = spriteData;
                        host.timeLineView.InsertFrame(selection.name, +1);
                    }

                    if (selection != null)
                    {
                        CurTexture = this.host.GetTexture(selection.name);
                    }
                    else
                    {
                        CurTexture = null;
                    }
                    HandleUtility.Repaint();
                    GUI.backgroundColor = Color.white;
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("请先创建图集，在创建动画！");
            }
            EndHeader();
        }

        void DrawHeaderLabel(string name)
        {
            GUILayout.Label(name, EditorStyles.boldLabel);
        }

        void BeginHeader(string name)
        {
            DrawHeaderLabel(name);
            GUILayout.Space(2);
            EditorGUI.indentLevel++;
        }

        void EndHeader()
        {
            EditorGUI.indentLevel--;
            GUILayout.Space(8);
        }
    }
}
