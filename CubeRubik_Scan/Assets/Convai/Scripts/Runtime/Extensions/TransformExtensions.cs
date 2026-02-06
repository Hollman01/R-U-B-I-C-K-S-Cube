using UnityEngine;

namespace Convai.Scripts.Extensions
{
    public static class TransformExtensions
    {
        public static bool LookingAtTarget(this Transform viewer, Transform target, float coneAngle)
        {
            if (viewer == null || target == null)
            {
                return false;
            }

            Vector3 directionToTarget = (target.position - viewer.position).normalized;
            directionToTarget.y = 0;
            directionToTarget.Normalize();

            Vector3 viewerForward = viewer.forward;
            viewerForward.y = 0;
            viewerForward.Normalize();

            float viewerToTargetDot = Vector3.Dot(viewerForward, directionToTarget);
            float angleThreshold = Mathf.Cos(coneAngle * 0.5f * Mathf.Deg2Rad);
            return viewerToTargetDot > angleThreshold;
        }

        public static float GetDotProduct(this Transform viewer, Transform target)
        {
            Vector3 directionToTarget = (target.position - viewer.position).normalized;
            directionToTarget.y = 0;
            directionToTarget.Normalize();

            Vector3 viewerForward = viewer.forward;
            viewerForward.y = 0;
            viewerForward.Normalize();

            return Vector3.Dot(viewerForward, directionToTarget);
        }
    }
}
