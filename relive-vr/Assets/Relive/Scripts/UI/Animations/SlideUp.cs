using System.Collections;
using UnityEngine;

namespace Relive.UI.Animations
{
    // Start is called before the first frame update
    public class SlideUp : MonoBehaviour
    {
        public GameObject AnimationObject;
        public float AnimationTime;
        public int YOffset;
        public int Spacing;

        public void Activate()
        {
            // Create animation for GameObject

            if (AnimationObject)
            {
                AnimationObject.SetActive(true);
                RectTransform rectTransform = GetComponent<RectTransform>();

                int count = 1;
                float currentPos = 0;
                // Get a list of all child objects
                foreach (Transform child in AnimationObject.transform)
                {
                    Animation anim = child.GetComponent<Animation>();
                    if (!anim)
                        anim = child.gameObject.AddComponent<Animation>();
                    AnimationClip animationClip;
                    currentPos += rectTransform.rect.height + Spacing;
                    AnimationCurve translateY =
                        AnimationCurve.Linear(0.0f, 0.0f, AnimationTime, YOffset + currentPos);
                    animationClip = new AnimationClip();
                    // set animation clip to be legacy
                    animationClip.legacy = true;
                    animationClip.SetCurve("", typeof(Transform), "localPosition.y", translateY);
                    // new event created
                    anim.AddClip(animationClip, count.ToString());
                    anim.Play(count++.ToString());
                }
            }
        }

        public IEnumerator DeActivateAnimationObject(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            AnimationObject.SetActive(false);
        }

        public void Deactivate()
        {
            // Create animation for GameObject

            if (AnimationObject)
            {
                StartCoroutine(DeActivateAnimationObject(AnimationTime));
                int count = 1;
                // Get a list of all child objects
                foreach (Transform child in AnimationObject.transform)
                {
                    child.gameObject.SetActive(true);
                    Animation anim = child.GetComponent<Animation>();
                    if (!anim)
                        anim = child.gameObject.AddComponent<Animation>();
                    AnimationClip animationClip;
                    AnimationCurve translateY =
                        AnimationCurve.Linear(0.0f, child.GetComponent<RectTransform>().localPosition.y, AnimationTime, 0);
                    animationClip = new AnimationClip();
                    // set animation clip to be legacy
                    animationClip.legacy = true;
                    animationClip.SetCurve("", typeof(Transform), "localPosition.y", translateY);
                    anim.AddClip(animationClip, count.ToString());
                    anim.Play(count++.ToString());
                }
            }
        }
    }
}