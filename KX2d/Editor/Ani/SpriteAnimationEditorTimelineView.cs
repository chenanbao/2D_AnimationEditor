
using System.Collections.Generic;
using System.Linq;
using KX2d.Core.Sprite;
using KX2d.Editor.Ani;
using UnityEditor;
using UnityEngine;

namespace KX2d.Editor.Sprite
{
    public class SpriteAnimationEditorTimelineView
    {
        private SpriteAnimationEditorPopup host;
        private SpriteAnimationData SpriteAnimationData { get { return host.SpriteAnimationData; } }
        private SpriteAnimationData.ActionData CurActionData = null;
        private Vector2 clipScrollbar = Vector2.zero;
        private int widthPerTick = 60;
        private int heightPerTick = 60;
        public int selectedFrame = 0;
        private int dragSelectFrame = -1;

        public SpriteAnimationEditorTimelineView(SpriteAnimationEditorPopup host)
        {
            this.host = host;
        }

        public void InsertFrame(string spriteName,int offset)
        {
            if (this.selectedFrame != -1)
            {
                List<SpriteAnimationData.FrameData> list = CurActionData.FrameList.ToList();
                SpriteAnimationData.FrameData tempData = new SpriteAnimationData.FrameData();
                tempData.SpriteName = spriteName;
                int insertPos = Mathf.Clamp((int)(this.selectedFrame + offset), 0, CurActionData.FrameList.Length);
                list.Insert(insertPos, tempData);
                this.selectedFrame = insertPos;
                CurActionData.FrameList = list.ToArray();
                HandleUtility.Repaint();
            }
           
        }

        public void AddFrame()
        {
            if (this.host.settingView.selection != null)
            {
                InsertFrame(this.host.settingView.selection.name,1);
            }
        }

        public void Draw()
        {
           


            CurActionData = this.host.CurActionData;

            clipScrollbar = GUILayout.BeginScrollView(clipScrollbar, GUILayout.Height(100), GUILayout.ExpandWidth(true));
            GUILayout.BeginVertical();
            //time
            GUILayout.Box("", EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            Rect r = GUILayoutUtility.GetLastRect();
            float t = 0.0f;
            float x = r.x;
            float fps = CurActionData != null ? CurActionData.fps : 30;
            while (x < r.x + r.width)
            {
                GUI.Label(new Rect(x, r.y, r.width, r.height), t.ToString("0.00"), EditorStyles.miniLabel);
                x += widthPerTick;
                t += 1 / fps;
            }
            //frame
           
            GUILayout.BeginHorizontal();
            if (CurActionData != null)
            {
                for (int i = 0; i < CurActionData.FrameList.Length; i++)
                {
                    SpriteAnimationData.FrameData frameData = CurActionData.FrameList[i];
                    DrawFrame(i, frameData);
                }

                GUI.color = Color.red;
                if (GUILayout.Button("+", "button", GUILayout.Height(60), GUILayout.Width(widthPerTick)))
                {
                    AddFrame();
                }
                GUI.color = Color.white;
              
                // Keyboard shortcuts
                Event ev = Event.current;
                int controlId = "Animation.TimeLineView".GetHashCode();
                if (ev.type == EventType.KeyDown && GUIUtility.keyboardControl == 0 && selectedFrame != -1)
                {
                    int newFrame = selectedFrame;
                    switch (ev.keyCode)
                    {
                        case KeyCode.LeftArrow:
                        case KeyCode.Comma: newFrame--; break;
                        case KeyCode.RightArrow:
                        case KeyCode.Period: newFrame++; break;
                        case KeyCode.Home: newFrame = 0; break;
                        case KeyCode.End: newFrame = CurActionData.FrameList.Length - 1; break;
                        case KeyCode.Escape: selectedFrame = -1; HandleUtility.Repaint(); ev.Use(); break;
                    }

                    if (ev.type != EventType.Used && CurActionData.FrameList.Length > 0)
                    {
                        newFrame = Mathf.Clamp(newFrame, 0, CurActionData.FrameList.Length - 1);
                        if (newFrame != selectedFrame)
                        {
                            selectedFrame = newFrame;
                            HandleUtility.Repaint();
                            ev.Use();
                        }
                    }
                }


                if (ev.type == EventType.KeyDown && (GUIUtility.hotControl == controlId || GUIUtility.keyboardControl == 0) && CurActionData.FrameList.Length > 0 && selectedFrame != -1)
                {
                    //delete
                    if ((ev.keyCode == KeyCode.Delete || ev.keyCode == KeyCode.Backspace))
                    {

                        List<SpriteAnimationData.FrameData> list = CurActionData.FrameList.ToList();
                        list.RemoveAt(selectedFrame);
                        CurActionData.FrameList = list.ToArray();
                        GUIUtility.hotControl = 0;
                        HandleUtility.Repaint();
                    }
                    else if (ev.keyCode == KeyCode.Insert)
                    {
                        List<SpriteAnimationData.FrameData> list = CurActionData.FrameList.ToList();
                        SpriteAnimationData.FrameData tempData = list[selectedFrame];
                        list.Insert(this.selectedFrame + 1, tempData);
                        this.selectedFrame = this.selectedFrame + 1;
                        CurActionData.FrameList = list.ToArray();
                        HandleUtility.Repaint();
                    }
                }

                if (ev.type == EventType.MouseDown || GUIUtility.hotControl == controlId)
                {
                    switch (ev.GetTypeForControl(controlId))
                    {
                        case EventType.MouseDown:
                            int selectFrame = Mathf.Max(0, (int)Mathf.Floor(ev.mousePosition.x / widthPerTick));
                            this.selectedFrame = selectFrame;
                            GUIUtility.keyboardControl = 0;
                            GUIUtility.hotControl = controlId;
                            HandleUtility.Repaint();
                            break;
                        case EventType.MouseUp:
                            if (this.dragSelectFrame != -1 && this.selectedFrame != -1)
                            {
                                List<SpriteAnimationData.FrameData> list = CurActionData.FrameList.ToList();
                                SpriteAnimationData.FrameData tempData = list[selectedFrame];
                                list.RemoveAt(selectedFrame);
                                list.Insert(this.dragSelectFrame, tempData);
                                CurActionData.FrameList = list.ToArray();
                                this.selectedFrame = this.dragSelectFrame;
                                this.dragSelectFrame = -1;
                                HandleUtility.Repaint();
                            }
                            GUIUtility.keyboardControl = 0;
                            GUIUtility.hotControl = 0;
                            break;
                        case EventType.MouseDrag:

                            dragSelectFrame = Mathf.Max(0, (int)Mathf.Floor(ev.mousePosition.x / widthPerTick));
                            dragSelectFrame = Mathf.Clamp(dragSelectFrame, 0, CurActionData.FrameList.Length - 1);
                            HandleUtility.Repaint();
                            break;
                    }
                }


                if (selectedFrame != -1 && this.CurActionData != null && this.CurActionData.FrameList.Length > 0)
                {
                    selectedFrame = Mathf.Clamp(selectedFrame, 0, this.CurActionData.FrameList.Length - 1);
                    string spriteName = this.CurActionData.FrameList[selectedFrame].SpriteName;
                    this.host.textureView.CurTexture = this.host.GetTexture(spriteName);
                }
                else
                {
                    this.host.textureView.CurTexture = null;
                }

            }
            GUILayout.EndHorizontal();


            GUILayout.EndVertical();
            GUILayout.EndScrollView();


            Rect rect = GUILayoutUtility.GetLastRect();
            GameEditorUtility.DrawOutline(rect, Color.gray);
        }



        private void DrawFrame(int frameIndex, SpriteAnimationData.FrameData frameData)
        {
            if (selectedFrame == frameIndex)
            {
                GUI.color = new Color(0.0f, 1f, 0.0f, 1.0f);
            }
            else
            {
                GUI.color = Color.white;
            }
            if (frameData != null)
            {

                GUILayout.Label("[" + frameIndex + "]\n" + frameData.SpriteName, "button", GUILayout.Height(heightPerTick), GUILayout.Width(widthPerTick));

            }
            else
            {
                GUILayout.Label("[" + frameIndex + "]\n", "button", GUILayout.Height(heightPerTick), GUILayout.Width(widthPerTick));
            }

            if (dragSelectFrame != -1)
            {
                Handles.color = new Color(0, 1, 0, 0.5f);
                Handles.DrawLine(new Vector3(dragSelectFrame * widthPerTick + 4, 21),
                    new Vector3(dragSelectFrame * widthPerTick + 4, 21 + heightPerTick));
               
            }


            GUILayout.Space(-4);
        }
    }
}
