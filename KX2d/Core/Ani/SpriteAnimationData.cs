using UnityEngine;

namespace KX2d.Core.Sprite
{

    public class SpriteAnimationData : ScriptableObject
    {

        [System.Serializable]
        public class FrameData
        {
            public string SpriteName;
            public Rect AttackBound;
            public Rect HitBound;
        }

        [System.Serializable]
        public class ActionData
        {
            public string name;
            public float fps = 30;
            public FrameData[] FrameList = new FrameData[0];
        }

        public const int CURRENT_VERSION = 1;
        public int version = 0;
        public SpriteAtlasData SpriteAtlasData;
        public ActionData[] ActionList = new ActionData[0];

        public ActionData GetActionData(string actionName)
        {
            if (!string.IsNullOrEmpty(actionName))
            {
                for (int i = 0; i < ActionList.Length; i++)
                {
                    ActionData actionData = ActionList[i];
                    if (actionData.name.Equals(actionName))
                    {
                        return actionData;
                    }
                }
            }
            return null;
        }
    }
}
