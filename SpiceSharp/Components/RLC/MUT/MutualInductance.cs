﻿using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.MutualInductanceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A mutual inductance
    /// </summary>
    public class MutualInductance : Component
    {
        static MutualInductance()
        {
            RegisterBehaviorFactory(typeof(MutualInductance), new BehaviorFactoryDictionary
            {
                {typeof(TransientBehavior), e => new TransientBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("inductor1"), ParameterInfo("First coupled inductor")]
        public string InductorName1 { get; set; }
        [ParameterName("inductor2"), ParameterInfo("Second coupled inductor")]
        public string InductorName2 { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the mutual inductance</param>
        public MutualInductance(string name) : base(name, 0)
        {
            Priority = ComponentPriority - 1;
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="inductorName1">Inductor 1</param>
        /// <param name="inductorName2">Inductor 2</param>
        /// <param name="coupling">Mutual inductance</param>
        public MutualInductance(string name, string inductorName1, string inductorName2, double coupling)
            : base(name, 0)
        {
            Priority = ComponentPriority - 1;
            ParameterSets.Add(new BaseParameters(coupling));
            InductorName1 = inductorName1;
            InductorName2 = inductorName2;
        }

        /// <summary>
        /// Add inductances to the data provider for setting up behaviors
        /// </summary>
        /// <returns></returns>
        protected override SetupDataProvider BuildSetupDataProvider(ParameterPool parameters, BehaviorPool behaviors)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (behaviors == null)
                throw new ArgumentNullException(nameof(behaviors));

            // Base execution (will add entity behaviors and parameters for this mutual inductance)
            var data = base.BuildSetupDataProvider(parameters, behaviors);

            // Register inductor 1
            data.Add("inductor1", parameters[InductorName1]);
            data.Add("inductor1", behaviors[InductorName1]);

            // Register inductor 2
            data.Add("inductor2", parameters[InductorName2]);
            data.Add("inductor2", behaviors[InductorName2]);

            return data;
        }
    }
}
