using System;
using System.Collections.Generic;
using System.Linq;
using Agents;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Observers
{
    public class Egocentric : IObserver
    {
        public void Observe(VectorSensor sensor, Transform transform)
        {
            // Debug.Log($"{name} CollectObs at step {GetComponentInParent<Statistician>().Time}");
        
            // RayPerceptionSensor structure:
            // 0 - n_tags: one-hot encoding of what was hit
            // n_tags: whether *something* was hit
            // n_tags + 1: normalized distance

            var agent = transform.GetComponent<AgentBasic>();
            var goal = agent.goal;

        
            Vector3 position = transform.localPosition;
            Quaternion rotation = transform.localRotation;
            Vector3 velocity = transform.GetComponent<Rigidbody>().velocity;
            Vector3 goalPosition = goal.localPosition;
            
            // Position: 2
            sensor.AddObservation(position.x / 10f);
            sensor.AddObservation(position.z / 10f);
        
            // Rotation: 1
            sensor.AddObservation(rotation.eulerAngles.y / 360f);
            
            // Relative position: 2
            var relPosition = Quaternion.Inverse(rotation) * (goalPosition - position);
            // var relPosition = goalPosition - position;
            sensor.AddObservation(relPosition.x / 20f);
            sensor.AddObservation(relPosition.z / 20f);

            // Debug.Log(relPosition);
            

            // Velocity: 2, up to ~5
            sensor.AddObservation(velocity.x / 5f);
            sensor.AddObservation(velocity.z / 5f);
            
            sensor.AddObservation(agent.CollectedGoal);
            sensor.AddObservation(agent.mass); // 8

        }
        public int Size => 9;

        public IEnumerable<string> ObserveAgents(BufferSensorComponent sensor, Transform transform, bool useAcceleration)
        {
            LayerMask layerMask = 1 << LayerMask.NameToLayer("Agent");
            var nearbyColliders =
                Physics.OverlapSphere(transform.position, Params.SightRadius, layerMask)
                    .Where(c => c.CompareTag("Agent") && c.transform != transform) // Get only agents
                    .Where(c => MLUtils.Visible(transform, c.transform, Params.MinCosine)) // Cone of vision
                    .OrderBy(c => Vector3.Distance(c.transform.localPosition, transform.localPosition))
                    .ToList(); // Might be redundant?
            
            var names = nearbyColliders
                .Select(c => c.transform.name)
                .Take(Params.SightAgents);
            
            
            var nearbyObjects= nearbyColliders
                .Select(c => GetColliderInfo(transform, c, useAcceleration))
                .Take(Params.SightAgents);
            
            // Debug.Log($"Visible objects: {nearbyObjects.Count()}");
            foreach (var agentInfo in nearbyObjects)
            {
                // Debug.Log(String.Join(",", agentInfo));
                sensor.AppendObservation(agentInfo);
            }

            return names;
        }

        public static float[] GetColliderInfo(Transform baseTransform, Collider collider, bool useAcceleration)
        {
            
            var rigidbody = collider.GetComponent<Rigidbody>();
            var transform = collider.transform;

            var agent = collider.GetComponent<AgentBasic>();

            var pos = transform.localPosition;
            var velocity = rigidbody.velocity;

            var rotation = baseTransform.localRotation;
            pos = Quaternion.Inverse(rotation) * (pos - baseTransform.localPosition);
            velocity = Quaternion.Inverse(rotation) * velocity;

            float[] obs;

            if (useAcceleration)
            {
                var acceleration = agent == null
                    ? Vector3.zero
                    : Quaternion.Inverse(rotation) * (velocity - agent.PreviousVelocity);

                obs = new[] {pos.x, pos.z, velocity.x, velocity.z, agent.mass, acceleration.x, acceleration.z};
            }
            else
            {
                obs = new[] {pos.x, pos.z, velocity.x, velocity.z, agent.mass};
            }

            // if (baseTransform.name == "Person")
            // {
            //     Debug.Log($"{baseTransform.name} sees {transform.name} at {pos} with velocity {velocity} and mass {agent.mass}");
            // }

            return obs;
        }
    }
}