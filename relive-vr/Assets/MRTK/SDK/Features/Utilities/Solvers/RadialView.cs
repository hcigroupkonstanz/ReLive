// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Utilities.Solvers
{
    /// <summary>
    /// RadialViewPoser solver locks a tag-along type object within a view cone
    /// </summary>
    [AddComponentMenu("Scripts/MRTK/SDK/RadialView")]
    public class RadialView : MonoBehaviour
    {
        public Transform TransformTarget;
        public Vector3 GoalPosition;
        public Quaternion GoalRotation;
        private float lastUpdateTime;

        public bool CalculatePosition;

        [SerializeField]
        [Tooltip(
            "If 0, the position will update immediately.  Otherwise, the greater this attribute the slower the position updates")]
        private float moveLerpTime = 0.1f;

        /// <summary>
        /// If 0, the position will update immediately.  Otherwise, the greater this attribute the slower the position updates
        /// </summary>
        public float MoveLerpTime
        {
            get => moveLerpTime;
            set => moveLerpTime = value;
        }

        [SerializeField]
        [Tooltip(
            "If 0, the rotation will update immediately.  Otherwise, the greater this attribute the slower the rotation updates")]
        private float rotateLerpTime = 0.1f;

        /// <summary>
        /// If 0, the rotation will update immediately.  Otherwise, the greater this attribute the slower the rotation updates")]
        /// </summary>
        public float RotateLerpTime
        {
            get => rotateLerpTime;
            set => rotateLerpTime = value;
        }

        /// <summary>
        /// The timestamp the solvers will use to calculate with.
        /// </summary>
        public float DeltaTime { get; set; }

        [SerializeField] [Tooltip("Min distance from eye to position element around, i.e. the sphere radius")]
        private float minDistance = 1f;

        /// <summary>
        /// Min distance from eye to position element around, i.e. the sphere radius.
        /// </summary>
        public float MinDistance
        {
            get => minDistance;
            set => minDistance = value;
        }

        [SerializeField] [Tooltip("Max distance from eye to element")]
        private float maxDistance = 2f;

        /// <summary>
        /// Max distance from eye to element.
        /// </summary>
        public float MaxDistance
        {
            get => maxDistance;
            set => maxDistance = value;
        }

        [SerializeField] [Tooltip("The element will stay at least this far away from the center of view")]
        private float minViewDegrees = 0f;

        /// <summary>
        /// The element will stay at least this far away from the center of view.
        /// </summary>
        public float MinViewDegrees
        {
            get => minViewDegrees;
            set => minViewDegrees = value;
        }

        [SerializeField] [Tooltip("The element will stay at least this close to the center of view")]
        private float maxViewDegrees = 30f;

        /// <summary>
        /// The element will stay at least this close to the center of view.
        /// </summary>
        public float MaxViewDegrees
        {
            get => maxViewDegrees;
            set => maxViewDegrees = value;
        }

        [SerializeField]
        [Tooltip("Apply a different clamp to vertical FOV than horizontal. Vertical = Horizontal * aspectV")]
        private float aspectV = 1f;

        /// <summary>
        /// Apply a different clamp to vertical FOV than horizontal. Vertical = Horizontal * AspectV.
        /// </summary>
        public float AspectV
        {
            get => aspectV;
            set => aspectV = value;
        }

        [SerializeField] [Tooltip("Option to ignore angle clamping")]
        private bool ignoreAngleClamp = false;

        /// <summary>
        /// Option to ignore angle clamping.
        /// </summary>
        public bool IgnoreAngleClamp
        {
            get => ignoreAngleClamp;
            set => ignoreAngleClamp = value;
        }

        [SerializeField] [Tooltip("Option to ignore distance clamping")]
        private bool ignoreDistanceClamp = false;

        /// <summary>
        /// Option to ignore distance clamping.
        /// </summary>
        public bool IgnoreDistanceClamp
        {
            get => ignoreDistanceClamp;
            set => ignoreDistanceClamp = value;
        }

        [SerializeField] [Tooltip("Ignore vertical movement and lock the Y position of the object")]
        private bool useFixedVerticalPosition = false;

        /// <summary>
        /// Ignore vertical movement and lock the Y position of the object.
        /// </summary>
        public bool UseFixedVerticalPosition
        {
            get => useFixedVerticalPosition;
            set => useFixedVerticalPosition = value;
        }

        [SerializeField] [Tooltip("Offset amount of the vertical position")]
        private float fixedVerticalPosition = -0.4f;

        /// <summary>
        /// Offset amount of the vertical position.
        /// </summary>
        public float FixedVerticalPosition
        {
            get => fixedVerticalPosition;
            set => fixedVerticalPosition = value;
        }

        [SerializeField]
        [Tooltip("If true, element will orient to ReferenceDirection, otherwise it will orient to ref position.")]
        private bool orientToReferenceDirection = false;

        /// <summary>
        /// If true, element will orient to ReferenceDirection, otherwise it will orient to ref position.
        /// </summary>
        public bool OrientToReferenceDirection
        {
            get => orientToReferenceDirection;
            set => orientToReferenceDirection = value;
        }

        /// <summary>
        /// Position to the view direction, or the movement direction, or the direction of the view cone.
        /// </summary>
        private Vector3 SolverReferenceDirection => TransformTarget != null ? TransformTarget.forward : Vector3.forward;

        /// <summary>
        /// The up direction to use for orientation.
        /// </summary>
        /// <remarks>Cone may roll with head, or not.</remarks>
        private Vector3 UpReference
        {
            get
            {
                Vector3 upReference = Vector3.up;

                upReference = TransformTarget != null ? TransformTarget.up : Vector3.up;

                return upReference;
            }
        }

        private Vector3 ReferencePoint => TransformTarget != null ? TransformTarget.position : Vector3.zero;

        private void Awake()
        {
            DeltaTime = Time.deltaTime;
        }

        public void Update()
        {
            DeltaTime = Time.realtimeSinceStartup - lastUpdateTime;
            lastUpdateTime = Time.realtimeSinceStartup;
            if (CalculatePosition)
            {
                Vector3 goalPosition = transform.position;

                if (ignoreAngleClamp)
                {
                    if (ignoreDistanceClamp)
                    {
                        goalPosition = transform.position;
                    }
                    else
                    {
                        GetDesiredOrientation_DistanceOnly(ref goalPosition);
                    }
                }
                else
                {
                    GetDesiredOrientation(ref goalPosition);
                }

                // Element orientation
                Vector3 refDirUp = UpReference;
                Quaternion goalRotation;

                if (orientToReferenceDirection)
                {
                    goalRotation = Quaternion.LookRotation(SolverReferenceDirection, refDirUp);
                }
                else
                {
                    goalRotation = Quaternion.LookRotation(goalPosition - ReferencePoint, refDirUp);
                }

                if (UseFixedVerticalPosition)
                {
                    goalPosition.y = ReferencePoint.y + FixedVerticalPosition;
                }

                GoalPosition = goalPosition;
                GoalRotation = goalRotation;

                UpdateTransformToGoal();
            }
        }

        /// <summary>
        /// Optimized version of GetDesiredOrientation.
        /// </summary>
        private void GetDesiredOrientation_DistanceOnly(ref Vector3 desiredPos)
        {
            // TODO: There should be a different solver for distance constraint.
            // Determine reference locations and directions
            Vector3 refPoint = ReferencePoint;
            Vector3 elementPoint = transform.position;
            Vector3 elementDelta = elementPoint - refPoint;
            float elementDist = elementDelta.magnitude;
            Vector3 elementDir = elementDist > 0 ? elementDelta / elementDist : Vector3.one;

            // Clamp distance too
            float clampedDistance = Mathf.Clamp(elementDist, minDistance, maxDistance);

            if (!clampedDistance.Equals(elementDist))
            {
                desiredPos = refPoint + clampedDistance * elementDir;
            }
        }

        private void GetDesiredOrientation(ref Vector3 desiredPos)
        {
            // Determine reference locations and directions
            Vector3 direction = SolverReferenceDirection;
            Vector3 upDirection = UpReference;
            Vector3 referencePoint = ReferencePoint;
            Vector3 elementPoint = transform.position;
            Vector3 elementDelta = elementPoint - referencePoint;
            float elementDist = elementDelta.magnitude;
            Vector3 elementDir = elementDist > 0 ? elementDelta / elementDist : Vector3.one;

            // Generate basis: First get axis perpendicular to reference direction pointing toward element
            Vector3 perpendicularDirection = (elementDir - direction);
            perpendicularDirection -= direction * Vector3.Dot(perpendicularDirection, direction);
            perpendicularDirection.Normalize();

            // Calculate the clamping angles, accounting for aspect (need the angle relative to view plane)
            float heightToViewAngle = Vector3.Angle(perpendicularDirection, upDirection);
            float verticalAspectScale =
                Mathf.Lerp(aspectV, 1f, Mathf.Abs(Mathf.Sin(heightToViewAngle * Mathf.Deg2Rad)));

            // Calculate the current angle
            float currentAngle = Vector3.Angle(elementDir, direction);
            float currentAngleClamped = Mathf.Clamp(currentAngle, minViewDegrees * verticalAspectScale,
                maxViewDegrees * verticalAspectScale);

            // Clamp distance too, if desired
            float clampedDistance =
                ignoreDistanceClamp ? elementDist : Mathf.Clamp(elementDist, minDistance, maxDistance);

            // If the angle was clamped, do some special update stuff
            if (currentAngle != currentAngleClamped)
            {
                float angRad = currentAngleClamped * Mathf.Deg2Rad;

                // Calculate new position
                desiredPos = referencePoint + clampedDistance *
                    (direction * Mathf.Cos(angRad) + perpendicularDirection * Mathf.Sin(angRad));
            }
            else if (!clampedDistance.Equals(elementDist))
            {
                // Only need to apply distance
                desiredPos = referencePoint + clampedDistance * elementDir;
            }
        }

        /// <summary>
        /// Lerps Vector3 source to goal.
        /// </summary>
        /// <remarks>
        /// Handles lerpTime of 0.
        /// </remarks>
        public static Vector3 SmoothTo(Vector3 source, Vector3 goal, float deltaTime, float lerpTime)
        {
            return Vector3.Lerp(source, goal, lerpTime.Equals(0.0f) ? 1f : deltaTime / lerpTime);
        }

        /// <summary>
        /// Slerps Quaternion source to goal, handles lerpTime of 0
        /// </summary>
        public static Quaternion SmoothTo(Quaternion source, Quaternion goal, float deltaTime, float lerpTime)
        {
            // Debug.Log(deltaTime / lerpTime);
            return Quaternion.Slerp(source, goal, lerpTime.Equals(0.0f) ? 1f : deltaTime / lerpTime);
        }

        /// <summary>
        /// Updates all object orientations to the goal orientation for this solver, with smoothing accounted for (smoothing may be off)
        /// </summary>
        protected void UpdateTransformToGoal()
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            pos = SmoothTo(pos, GoalPosition, DeltaTime, moveLerpTime);
            rot = SmoothTo(rot, GoalRotation, DeltaTime, rotateLerpTime);

            transform.position = pos;
            transform.rotation = rot;
        }
    }
}