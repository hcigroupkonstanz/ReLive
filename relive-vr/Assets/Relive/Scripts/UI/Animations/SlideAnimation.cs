using UnityEngine;

namespace Relive.UI.Animations
{
    public class SlideAnimation : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject AnimationObject;
        public float AnimationTime;
        public bool AnimateWidth;
        public bool AnimateHeight;
        public int NewWidth;
        public int NewHeight;
        public int DefaultWidth;
        public int DefaultHeight;

        private Animation anim;

        public void Activate()
        {
            if (AnimationObject)
            {
                anim = GetComponent<Animation>();
                if (!anim)
                    anim = gameObject.AddComponent<Animation>();
                AnimationCurve modifyWidth, modifyHeight;
                AnimationClip animationClip = new AnimationClip();
                // set animation clip to be legacy
                animationClip.legacy = true;
                if (AnimateWidth)
                {
                    modifyWidth = AnimationCurve.Linear(0,DefaultWidth,AnimationTime,NewWidth);
                    animationClip.SetCurve("", typeof(RectTransform), "m_SizeDelta.x", modifyWidth);
                }
                if (AnimateHeight)
                {
                    modifyHeight = AnimationCurve.Linear(0,DefaultHeight,AnimationTime,NewHeight);
                    animationClip.SetCurve("", typeof(RectTransform), "m_SizeDelta.y", modifyHeight);
                }
                
                // new event created
                anim.AddClip(animationClip, "SlideIn");
                anim.Play("SlideIn");
            }
        }

        public void Deactivate()
        {
            if (AnimationObject)
            {

                anim = GetComponent<Animation>();
                if (!anim)
                    anim = gameObject.AddComponent<Animation>();
                AnimationCurve modifyWidth, modifyHeight;
                AnimationClip animationClip = new AnimationClip();
                // set animation clip to be legacy
                animationClip.legacy = true;
                if (AnimateWidth)
                {
                    modifyWidth = AnimationCurve.Linear(0,GetComponent<RectTransform>().rect.width,AnimationTime,DefaultWidth);
                    animationClip.SetCurve("", typeof(RectTransform), "m_SizeDelta.x", modifyWidth);
                }

                if (AnimateHeight)
                {
                    modifyHeight = AnimationCurve.Linear(0,GetComponent<RectTransform>().rect.height,AnimationTime,DefaultHeight);
                    animationClip.SetCurve("", typeof(RectTransform), "m_SizeDelta.y", modifyHeight);
                }
                
                // new event created
                anim.AddClip(animationClip, "SlideOut");
                anim.Play("SlideOut");
            }
        }
    }
}