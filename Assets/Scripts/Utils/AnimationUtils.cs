using System;
using System.Collections;
using UnityEngine;

public static class LeanTween
{
    public static AnimationHandler scale(GameObject target, Vector3 targetScale, float duration)
    {
        AnimationHandler handler = new AnimationHandler(target, duration);
        handler.StartScaleAnimation(targetScale);
        return handler;
    }
    
    public class AnimationHandler
    {
        private GameObject target;
        private float duration;
        private Action onCompleteCallback;
        
        public AnimationHandler(GameObject target, float duration)
        {
            this.target = target;
            this.duration = duration;
        }
        
        public AnimationHandler setOnComplete(Action callback)
        {
            onCompleteCallback = callback;
            return this;
        }
        
        public void StartScaleAnimation(Vector3 targetScale)
        {
            MonoBehaviour component = target.GetComponent<MonoBehaviour>();
            if (component != null)
            {
                component.StartCoroutine(ScaleCoroutine(targetScale));
            }
        }
        
        private IEnumerator ScaleCoroutine(Vector3 targetScale)
        {
            Vector3 startScale = target.transform.localScale;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            target.transform.localScale = targetScale;
            onCompleteCallback?.Invoke();
        }
    }
} 