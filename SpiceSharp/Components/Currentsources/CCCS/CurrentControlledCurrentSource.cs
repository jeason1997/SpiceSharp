﻿using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled current source.
    /// </summary>
    [Pin(0, "F+"), Pin(1, "F-"), Connected(0, 0)]
    public class CurrentControlledCurrentSource : Component
    {
        static CurrentControlledCurrentSource()
        {
            RegisterBehaviorFactory(typeof(CurrentControlledCurrentSource), new BehaviorFactoryDictionary
            {
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("control"), ParameterInfo("Name of the controlling source")]
        public string ControllingName { get; set; }

        /// <summary>
        /// Constants
        /// </summary>
		[ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CurrentControlledCurrentSourcePinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        public CurrentControlledCurrentSource(string name) 
            : base(name, CurrentControlledCurrentSourcePinCount)
        {
            Priority = ComponentPriority - 1;
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="voltageSource">The name of the voltage source</param>
        /// <param name="gain">The current gain</param>
        public CurrentControlledCurrentSource(string name, string pos, string neg, string voltageSource, double gain)
            : this(name)
        {
            ParameterSets.Get<BaseParameters>().Coefficient.Value = gain;
            Connect(pos, neg);
            ControllingName = voltageSource;
        }

        /// <summary>
        /// Build the data provider
        /// </summary>
        /// <returns></returns>
        protected override SetupDataProvider BuildSetupDataProvider(ParameterPool parameters, BehaviorPool behaviors)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (behaviors == null)
                throw new ArgumentNullException(nameof(behaviors));
            var provider = base.BuildSetupDataProvider(parameters, behaviors);

            // Add behaviors and parameters of the controlling voltage source
            provider.Add("control", behaviors[ControllingName]);
            provider.Add("control", parameters[ControllingName]);
            return provider;
        }
    }
}
