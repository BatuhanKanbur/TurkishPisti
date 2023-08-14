using System;
using System.Collections;
using Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Structures
{
    public static class BKTween
    {
        private static MonoBehaviour _managerBehaviour;

        public static void SetPositionAndRotation(this Transform transform, Transform targetPosition,Transform targetAngles, float duration = 1,bool isLocal=false, Action callBack = null)
        {
            Move(transform,targetPosition,duration,isLocal,callBack);
            Rotate(transform,targetAngles,duration,isLocal);
        }
        public static void SetPositionAndRotation(this Transform transform, Transform targetPosition,Vector3 targetAngles, float duration = 1,bool isLocal=false, Action callBack = null)
        {
            Move(transform,targetPosition,duration,isLocal,callBack);
            Rotate(transform,targetAngles,duration,isLocal);
        }
        public static void SetPositionAndRotation(this Transform transform, Vector3 targetPosition,Vector3 targetAngles, float duration = 1,bool isLocal=false, Action callBack = null)
        {
            Move(transform,targetPosition,duration,isLocal,callBack);
            Rotate(transform,targetAngles,duration,isLocal);
        }
        public static void Move(this Transform transform, Transform targetPosition, float duration = 1,bool isLocal=false,Action callBack = null)
        {
            _managerBehaviour = transform.GetComponent<MonoBehaviour>();
            _managerBehaviour.StartCoroutine(MoveLerp(transform, targetPosition.position, duration,isLocal, callBack));
        }
        public static void Move(this Transform transform, Vector3 targetPosition, float duration = 1,bool isLocal=false,Action callBack = null)
        {
            _managerBehaviour = transform.GetComponent<MonoBehaviour>();
            _managerBehaviour.StartCoroutine(MoveLerp(transform, targetPosition, duration,isLocal, callBack));
        }
        public static void Rotate(this Transform transform, Vector3 targetAngles, float duration = 1,bool isLocal=false,Action callBack = null)
        {
            _managerBehaviour = transform.GetComponent<MonoBehaviour>();
            _managerBehaviour.StartCoroutine(RotateLerp(transform, targetAngles, duration,isLocal, callBack));
        }
        public static void Rotate(this Transform transform, Transform targetAngles, float duration = 1,bool isLocal=false,Action callBack = null)
        {
            _managerBehaviour = transform.GetComponent<MonoBehaviour>();
            _managerBehaviour.StartCoroutine(RotateLerp(transform, targetAngles.localEulerAngles, duration,isLocal, callBack));
        }
        private static IEnumerator MoveLerp(Transform transform, Vector3 targetPosition, float duration,bool isLocal,Action callBack)
        {
            float elapsedTime = 0f;
            Vector3 startingPosition = isLocal ? transform.localPosition : transform.position;
            while (elapsedTime < duration)
            {
                if(isLocal)
                    transform.localPosition = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / duration);
                else
                    transform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (isLocal)
                transform.localPosition = targetPosition;
            else
                transform.position = targetPosition;
            callBack?.Invoke();
        }
        private static IEnumerator RotateLerp(Transform transform, Vector3 targetAngles, float duration,bool isLocal,Action callBack)
        {
            float elapsedTime = 0f;
            Vector3 startingAngles = isLocal ? transform.localEulerAngles : transform.eulerAngles;
            while (elapsedTime < duration)
            {
                if(isLocal)
                    transform.localEulerAngles = Vector3.Lerp(startingAngles, targetAngles, elapsedTime / duration);
                else
                    transform.eulerAngles = Vector3.Lerp(startingAngles, targetAngles, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (isLocal)
                transform.localEulerAngles = targetAngles;
            else
                transform.eulerAngles = targetAngles;
            callBack?.Invoke();
        }    
    }
}
