using System;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;

namespace Sensors
{
    /// <summary>
    /// A base class to support sensor components for raycast-based sensors.
    /// </summary>
    public class RaySensorComponent : SensorComponent
    {
        [SerializeField, FormerlySerializedAs("sensorName")]
        string m_SensorName = "RaySensor";

        /// <summary>
        /// The name of the Sensor that this component wraps.
        /// Note that changing this at runtime does not affect how the Agent sorts the sensors.
        /// </summary>
        public string SensorName
        {
            get { return m_SensorName; }
            set { m_SensorName = value; }
        }

        [SerializeField, FormerlySerializedAs("detectableTags")]
        [Tooltip("List of tags in the scene to compare against.")]
        List<string> m_DetectableTags;

        /// <summary>
        /// List of tags in the scene to compare against.
        /// Note that this should not be changed at runtime.
        /// </summary>
        public List<string> DetectableTags
        {
            get { return m_DetectableTags; }
            set { m_DetectableTags = value; }
        }

        [SerializeField, FormerlySerializedAs("raysPerDirection")]
        [Range(0, 50)]
        [Tooltip("Number of rays to the left and right of center.")]
        int m_RaysPerDirection = 3;

        /// <summary>
        /// Number of rays to the left and right of center.
        /// Note that this should not be changed at runtime.
        /// </summary>
        public int RaysPerDirection
        {
            get { return m_RaysPerDirection; }
            // Note: can't change at runtime
            set { m_RaysPerDirection = value; }
        }

        [SerializeField, FormerlySerializedAs("maxRayDegrees")]
        [Range(0, 180)]
        [Tooltip("Cone size for rays. Using 90 degrees will cast rays to the left and right. " +
            "Greater than 90 degrees will go backwards.")]
        float m_MaxRayDegrees = 70;

        /// <summary>
        /// Cone size for rays. Using 90 degrees will cast rays to the left and right.
        /// Greater than 90 degrees will go backwards.
        /// </summary>
        public float MaxRayDegrees
        {
            get => m_MaxRayDegrees;
            set { m_MaxRayDegrees = value; UpdateSensor(); }
        }

        [SerializeField, FormerlySerializedAs("sphereCastRadius")]
        [Range(0f, 10f)]
        [Tooltip("Radius of sphere to cast. Set to zero for raycasts.")]
        float m_SphereCastRadius = 0.5f;

        /// <summary>
        /// Radius of sphere to cast. Set to zero for raycasts.
        /// </summary>
        public float SphereCastRadius
        {
            get => m_SphereCastRadius;
            set { m_SphereCastRadius = value; UpdateSensor(); }
        }

        [SerializeField, FormerlySerializedAs("rayLength")]
        [Range(1, 1000)]
        [Tooltip("Length of the rays to cast.")]
        float m_RayLength = 20f;

        /// <summary>
        /// Length of the rays to cast.
        /// </summary>
        public float RayLength
        {
            get => m_RayLength;
            set { m_RayLength = value; UpdateSensor(); }
        }

        // The value of the default layers.
        const int k_PhysicsDefaultLayers = -5;
        [SerializeField, FormerlySerializedAs("rayLayerMask")]
        [Tooltip("Controls which layers the rays can hit.")]
        LayerMask m_RayLayerMask = k_PhysicsDefaultLayers;

        /// <summary>
        /// Controls which layers the rays can hit.
        /// </summary>
        public LayerMask RayLayerMask
        {
            get => m_RayLayerMask;
            set { m_RayLayerMask = value; UpdateSensor(); }
        }

        [SerializeField, FormerlySerializedAs("observationStacks")]
        [Range(1, 50)]
        [Tooltip("Number of raycast results that will be stacked before being fed to the neural network.")]
        int m_ObservationStacks = 1;

        /// <summary>
        /// Whether to stack previous observations. Using 1 means no previous observations.
        /// Note that changing this after the sensor is created has no effect.
        /// </summary>
        public int ObservationStacks
        {
            get { return m_ObservationStacks; }
            set { m_ObservationStacks = value; }
        }
        
        [SerializeField, FormerlySerializedAs("startVerticalOffset")]
        [Range(-10f, 10f)]
        [Tooltip("Ray start is offset up or down by this amount.")]
        float m_StartVerticalOffset;

        /// <summary>
        /// Ray start is offset up or down by this amount.
        /// </summary>
        public float StartVerticalOffset
        {
            get => m_StartVerticalOffset;
            set { m_StartVerticalOffset = value; UpdateSensor(); }
        }

        [SerializeField, FormerlySerializedAs("endVerticalOffset")]
        [Range(-10f, 10f)]
        [Tooltip("Ray end is offset up or down by this amount.")]
        float m_EndVerticalOffset;

        /// <summary>
        /// Ray end is offset up or down by this amount.
        /// </summary>
        public float EndVerticalOffset
        {
            get => m_EndVerticalOffset;
            set { m_EndVerticalOffset = value; UpdateSensor(); }
        }

        /// <summary>
        /// Color to code a ray that hits another object.
        /// </summary>
        [SerializeField]
        [Header("Debug Gizmos", order = 999)]
        internal Color rayHitColor = Color.red;

        /// <summary>
        /// Color to code a ray that avoid or misses all other objects.
        /// </summary>
        [SerializeField]
        internal Color rayMissColor = Color.white;

        [NonSerialized]
        RaySensor m_RaySensor;

        /// <summary>
        /// Get the RaySensor that was created.
        /// </summary>
        public RaySensor RaySensor
        {
            get => m_RaySensor;
        }

        /// <summary>
        /// Returns the <see cref="RayCastType"/> for the associated raycast sensor.
        /// </summary>
        /// <returns></returns>
        public RayCastCType GetCastType()
        {
            return RayCastCType.Cast3D;
        }


        
        /// <summary>
        /// Returns the amount that the ray start is offset up or down by.
        /// </summary>
        /// <returns></returns>
        public virtual float GetStartVerticalOffset()
        {
            return StartVerticalOffset;
        }

        /// <summary>
        /// Returns the amount that the ray end is offset up or down by.
        /// </summary>
        /// <returns></returns>
        public virtual float GetEndVerticalOffset()
        {
            return EndVerticalOffset;
        }

        /// <summary>
        /// Returns an initialized raycast sensor.
        /// </summary>
        /// <returns></returns>
        public override ISensor[] CreateSensors()
        {
            var rayPerceptionInput = GetRayPerceptionInput();

            m_RaySensor = new RaySensor(m_SensorName, rayPerceptionInput);

            if (ObservationStacks != 1)
            {
                var stackingSensor = new StackingSensor(m_RaySensor, ObservationStacks);
                return new ISensor[] { stackingSensor };
            }

            return new ISensor[] { m_RaySensor };
        }

        /// <summary>
        /// Returns the specific ray angles given the number of rays per direction and the
        /// cone size for the rays.
        /// </summary>
        /// <param name="raysPerDirection">Number of rays to the left and right of center.</param>
        /// <param name="maxRayDegrees">
        /// Cone size for rays. Using 90 degrees will cast rays to the left and right.
        /// Greater than 90 degrees will go backwards.
        /// </param>
        /// <returns></returns>
        internal static float[] GetRayAngles(int raysPerDirection, float maxRayDegrees)
        {
            // Example:
            // { 90 - 5*delta, 90 - 4*delta, ..., 90, 90 + delta, ..., 90 + 5*delta }
            var anglesOut = new float[2 * raysPerDirection + 1];
            var delta = maxRayDegrees / raysPerDirection;
            
            for (var i =  0; i < 2*raysPerDirection + 1; i++)
            {
                // 90 - (num_rays) * delta + i * delta
                anglesOut[i] = 90 - (raysPerDirection - i) * delta;
            }

            return anglesOut;

        } 

        /// <summary>
        /// Get the RayPerceptionInput that is used by the <see cref="RayPerceptionSensor"/>.
        /// </summary>
        /// <returns></returns>
        public RayInput GetRayPerceptionInput()
        {
            var rayAngles = GetRayAngles(RaysPerDirection, MaxRayDegrees);

            var rayPerceptionInput = new RayInput();
            rayPerceptionInput.RayLength = RayLength;
            rayPerceptionInput.DetectableTags = DetectableTags;
            rayPerceptionInput.Angles = rayAngles;
            rayPerceptionInput.StartOffset = GetStartVerticalOffset();
            rayPerceptionInput.EndOffset = GetEndVerticalOffset();
            rayPerceptionInput.CastRadius = SphereCastRadius;
            rayPerceptionInput.Transform = transform;
            rayPerceptionInput.CastType = GetCastType();
            rayPerceptionInput.LayerMask = RayLayerMask;

            return rayPerceptionInput;
        }

        internal void UpdateSensor()
        {
            if (m_RaySensor != null)
            {
                var rayInput = GetRayPerceptionInput();
                m_RaySensor.SetRayPerceptionInput(rayInput);
            }
        }

        internal int SensorObservationAge()
        {
            if (m_RaySensor != null)
            {
                return Time.frameCount - m_RaySensor.DebugLastFrameCount;
            }

            return 0;
        }

        void OnDrawGizmosSelected()
        {
            if (m_RaySensor?.RayPerceptionOutput?.RayOutputs != null)
            {
                // If we have cached debug info from the sensor, draw that.
                // Draw "old" observations in a lighter color.
                // Since the agent may not step every frame, this helps de-emphasize "stale" hit information.
                var alpha = Mathf.Pow(.5f, SensorObservationAge());

                foreach (var rayInfo in m_RaySensor.RayPerceptionOutput.RayOutputs)
                {
                    DrawRaycastGizmos(rayInfo, alpha);
                }
            }
            else
            {
                var rayInput = GetRayPerceptionInput();
                // We don't actually need the tags here, since they don't affect the display of the rays.
                // Additionally, the user might be in the middle of typing the tag name when this is called,
                // and there's no way to turn off the "Tag ... is not defined" error logs.
                // So just don't use any tags here.
                rayInput.DetectableTags = null;
                for (var rayIndex = 0; rayIndex < rayInput.Angles.Count; rayIndex++)
                {
                    var rayOutput = RaySensor.PerceiveSingleRay(rayInput, rayIndex);
                    DrawRaycastGizmos(rayOutput);
                }
            }
        }

        /// <summary>
        /// Draw the debug information from the sensor (if available).
        /// </summary>
        void DrawRaycastGizmos(RayCOutput.RayOutput rayOutput, float alpha = 1.0f)
        {
            var startPositionWorld = rayOutput.StartPositionWorld;
            var endPositionWorld = rayOutput.EndPositionWorld;
            var rayDirection = endPositionWorld - startPositionWorld;
            rayDirection *= rayOutput.HitFraction;

            // hit fraction ^2 will shift "far" hits closer to the hit color
            var lerpT = rayOutput.HitFraction * rayOutput.HitFraction;
            var color = Color.Lerp(rayHitColor, rayMissColor, lerpT);
            color.a *= alpha;
            Gizmos.color = color;
            Gizmos.DrawRay(startPositionWorld, rayDirection);

            // Draw the hit point as a sphere. If using rays to cast (0 radius), use a small sphere.
            if (rayOutput.HasHit)
            {
                var hitRadius = Mathf.Max(rayOutput.ScaledCastRadius, .05f);
                Gizmos.DrawWireSphere(startPositionWorld + rayDirection, hitRadius);
            }
        }
    }
}
