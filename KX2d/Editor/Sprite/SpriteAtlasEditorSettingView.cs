using UnityEditor;
using UnityEngine;

namespace KX2d.Editor.Sprite
{
    public class SpriteAtlasEditorSettingView
    {
        private SpriteAtlasEditorPopup host;
        private SpriteAtlasProxy _spriteAtlasProxy { get { return host.SpriteAtlasProxy; } }

        public SpriteAtlasEditorSettingView(SpriteAtlasEditorPopup host)
        {
            this.host = host;
        }

        public void Draw()
        {
            GUILayout.BeginVertical(GUILayout.Width(160), GUILayout.ExpandHeight(true));
            DrawTextureSettings();
            GUILayout.EndVertical();
        }

        private void DrawTextureSettings()
        {

            if (_spriteAtlasProxy == null) return;

            BeginHeader("图集设置");

            int[] allowedAtlasSizes = { 64, 128, 256, 512, 1024, 2048};
            string[] allowedAtlasSizesString = new string[allowedAtlasSizes.Length];
            for (int i = 0; i < allowedAtlasSizes.Length; ++i)
                allowedAtlasSizesString[i] = allowedAtlasSizes[i].ToString();

           

            _spriteAtlasProxy.forceTextureSize = EditorGUILayout.Toggle("强制图集大小", _spriteAtlasProxy.forceTextureSize);
            EditorGUI.indentLevel++;
            if (_spriteAtlasProxy.forceTextureSize)
            {
                _spriteAtlasProxy.forcedTextureWidth = EditorGUILayout.IntPopup("宽", _spriteAtlasProxy.forcedTextureWidth, allowedAtlasSizesString, allowedAtlasSizes);
                _spriteAtlasProxy.forcedTextureHeight = EditorGUILayout.IntPopup("高", _spriteAtlasProxy.forcedTextureHeight, allowedAtlasSizesString, allowedAtlasSizes);
            }
            else
            {
                _spriteAtlasProxy.maxTextureSize = EditorGUILayout.IntPopup("最大尺寸", _spriteAtlasProxy.maxTextureSize, allowedAtlasSizesString, allowedAtlasSizes);
                _spriteAtlasProxy.forceSquareAtlas = EditorGUILayout.Toggle("强制正方尺寸", _spriteAtlasProxy.forceSquareAtlas);
            }
            EditorGUI.indentLevel--;

            //bool allowMultipleAtlases = EditorGUILayout.Toggle("Multiple Atlases", _spriteAtlasProxy.allowMultipleAtlases);

            EditorGUILayout.LabelField("输出宽", _spriteAtlasProxy.atlasWidth.ToString());
            EditorGUILayout.LabelField("输出高", _spriteAtlasProxy.atlasHeight.ToString());
            EditorGUILayout.LabelField("图集布局浪费率", _spriteAtlasProxy.atlasWastage.ToString("0.00") + "%");
            _spriteAtlasProxy.disableRotation = EditorGUILayout.Toggle("禁止旋转", _spriteAtlasProxy.disableRotation);
            _spriteAtlasProxy.disableTrimming = EditorGUILayout.Toggle("禁止裁剪", _spriteAtlasProxy.disableTrimming);
            _spriteAtlasProxy.allowMultipleAtlases = EditorGUILayout.Toggle("允许生成多图级", _spriteAtlasProxy.allowMultipleAtlases);
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
