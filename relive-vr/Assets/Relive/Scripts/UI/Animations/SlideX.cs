using System.Collections;
using UnityEngine;

namespace Relive.UI.Animations
{
    public class SlideX : MonoBehaviour
    {
        public GameObject AnimationObject;
        public float AnimationTime;
        public int XOffset;
        public int Spacing;

        private Coroutine deactivateRoutine;

        public void Activate()
        {
            // Create animation for GameObject
            //deactivate remaining coroutine if there are any
            if (deactivateRoutine != null)
                StopCoroutine(deactivateRoutine);

            if (AnimationObject)
            {
                AnimationObject.SetActive(true);
                RectTransform rectTransform = GetComponent<RectTransform>();

                int count = 0;
                float currentPos = 0;
                // Get a list of all child objects
                foreach (Transform child in AnimationObject.transform)
                {
                    child.gameObject.SetActive(true);
                    Animation anim = child.GetComponent<Animation>();
                    if (!anim)
                        anim = child.gameObject.AddComponent<Animation>();
                    AnimationClip animationClip;
                    currentPos = count * (child.GetComponent<RectTransform>().rect.width + Spacing);
                    AnimationCurve translateX =
                        AnimationCurve.Linear(0.0f, 0.0f, AnimationTime, XOffset + currentPos);
                    animationClip = new AnimationClip();
                    // set animation clip to be legacy
                    animationClip.legacy = true;
                    animationClip.SetCurve("", typeof(Transform), "localPosition.x", translateX);
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

            if (AnimationObject && AnimationObject.activeSelf)
            {
                deactivateRoutine = StartCoroutine(DeActivateAnimationObject(AnimationTime));
                int count = 1;
                // Get a list of all child objects
                foreach (Transform child in AnimationObject.transform)
                {
                    child.gameObject.SetActive(true);
                    Animation anim = child.GetComponent<Animation>();
                    if (!anim)
                        anim = child.gameObject.AddComponent<Animation>();
                    AnimationClip animationClip;
                    AnimationCurve translateX =
                        AnimationCurve.Linear(0.0f, child.GetComponent<RectTransform>().localPosition.x, AnimationTime,
                            0);
                    animationClip = new AnimationClip();
                    // set animation clip to be legacy
                    animationClip.legacy = true;
                    animationClip.SetCurve("", typeof(Transform), "localPosition.x", translateX);
                    anim.AddClip(animationClip, count.ToString());
                    anim.Play(count++.ToString());
                }
            }
        }
    }
}