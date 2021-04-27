using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Observers
{
    public interface IObserver
    {
        public void Observe(VectorSensor sensor, Transform transform, Transform goal);

        public int Size
        {
            get;
        }
    }

    public enum ObserversEnum
    {
        Absolute,
        Relative,
        RotRelative
    }
    
    public static class Mapper
    {
        public static IObserver GetObserver(ObserversEnum obsType)
        {
            IObserver observer = obsType switch
            {
                ObserversEnum.Absolute => new Absolute(),
                ObserversEnum.Relative => new Relative(),
                ObserversEnum.RotRelative => new RotRelative(),
                _ => null
            };

            return observer;
        }
    }
}