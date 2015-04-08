using UnityEngine;

namespace KX2d.Core.Sprite
{

    public class SpriteAtlasData : ScriptableObject
    {
        [System.Serializable]
        public class SpriteData
        {
            /// <summary>
            /// 原始图名
            /// </summary>
            public string name = "";
            /// <summary>
            /// 裁剪透明后的区域
            /// </summary>
            public Rect bound;
            /// <summary>
            /// 在图集上的位置
            /// </summary>
            public int regionX, regionY, regionW, regionH;
            /// <summary>
            /// 图片间隔
            /// </summary>
            public int padding = 0;
            /// <summary>
            /// 原始图大小
            /// </summary>
            public int sourceWidth = 512;
            public int sourceHeight = 512;
            /// <summary>
            /// 所在大图索引
            /// </summary>
            public int atlasIndex = 0;

            public void CopyFrom(SpriteData src)
            {
                name = src.name;
                bound = new Rect(src.bound.x, src.bound.y, src.bound.width, bound.height);
                regionX = src.regionX;
                regionY = src.regionY;
                regionW = src.regionW;
                regionH = src.regionH;
                padding = src.padding;
                sourceWidth = src.sourceWidth;
                sourceWidth = src.sourceHeight;
                atlasIndex = src.atlasIndex;
            }
        }

        public const int CURRENT_VERSION = 1;
        public int version = 0;
        public Texture2D[] atlasTextures;
        public Material[] atlasMaterials;
        public SpriteData[] spriteDataList;


        public SpriteAtlasData.SpriteData GetSpriteData(string spriteName)
        {
            if (!string.IsNullOrEmpty(spriteName))
            {
                for (int i = 0; i < spriteDataList.Length; i++)
                {
                    SpriteData spriteData = spriteDataList[i];
                    if (spriteData.name.Equals(spriteName))
                    {
                        return spriteData;
                    }
                }
            }
            return null;
        }
    }

   
}
