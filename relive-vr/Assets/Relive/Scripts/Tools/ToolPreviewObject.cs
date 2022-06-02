using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Relive.UI.Toolbox
{
    public class ToolPreviewObject : MonoBehaviour
    {
        public SpriteRenderer FrontSpriteRenderer;
        public SpriteRenderer BackSpriteRenderer;
        public SpriteRenderer RightSpriteRenderer;
        public SpriteRenderer LeftSpriteRenderer;
        public SpriteRenderer TopSpriteRenderer;
        public SpriteRenderer BottomSpriteRenderer;

        public void SetAllSprites(Sprite sprite)
        {
            FrontSpriteRenderer.sprite = sprite;
            BackSpriteRenderer.sprite = sprite;
            RightSpriteRenderer.sprite = sprite;
            LeftSpriteRenderer.sprite = sprite;
            TopSpriteRenderer.sprite = sprite;
            BottomSpriteRenderer.sprite = sprite;
        }

    }
}
