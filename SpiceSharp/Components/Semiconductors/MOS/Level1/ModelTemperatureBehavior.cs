﻿using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Temperature behavior for a <see cref="Model"/>
    /// </summary>
    public class ModelTemperatureBehavior : ExportingBehavior, ITemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        protected ModelBaseParameters ModelParameters { get; private set; }

        /// <summary>
        /// Shared parameters
        /// </summary>
        public double Factor1 { get; private set; }
        public double VtNominal { get; private set; }
        public double EgFet1 { get; private set; }
        public double PbFactor1 { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public ModelTemperatureBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            
            // Get parameters
            ModelParameters = provider.GetParameterSet<ModelBaseParameters>();
        }
        
        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public void Temperature(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            // Perform model defaulting
            if (!ModelParameters.NominalTemperature.Given)
                ModelParameters.NominalTemperature.RawValue = simulation.RealState.NominalTemperature;
            Factor1 = ModelParameters.NominalTemperature / Circuit.ReferenceTemperature;
            VtNominal = ModelParameters.NominalTemperature * Circuit.KOverQ;
            var kt1 = Circuit.Boltzmann * ModelParameters.NominalTemperature;
            EgFet1 = 1.16 - 7.02e-4 * ModelParameters.NominalTemperature * ModelParameters.NominalTemperature / (ModelParameters.NominalTemperature + 1108);
            var arg1 = -EgFet1 / (kt1 + kt1) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            PbFactor1 = -2 * VtNominal * (1.5 * Math.Log(Factor1) + Circuit.Charge * arg1);

            if (ModelParameters.OxideThickness.Given && ModelParameters.OxideThickness > 0.0)
            {
                if (ModelParameters.SubstrateDoping.Given)
                {
                    if (ModelParameters.SubstrateDoping * 1e6 > 1.45e16)
                    {
                        if (!ModelParameters.Phi.Given)
                        {
                            ModelParameters.Phi.RawValue = 2 * VtNominal * Math.Log(ModelParameters.SubstrateDoping * 1e6 / 1.45e16);
                            ModelParameters.Phi.RawValue = Math.Max(.1, ModelParameters.Phi);
                        }

                        var fermis = ModelParameters.MosfetType * .5 * ModelParameters.Phi;
                        var wkfng = 3.2;
                        if (!ModelParameters.GateType.Given)
                            ModelParameters.GateType.RawValue = 1;
                        if (!ModelParameters.GateType.RawValue.Equals(0))
                        {
                            var fermig = ModelParameters.MosfetType * ModelParameters.GateType * .5 * EgFet1;
                            wkfng = 3.25 + .5 * EgFet1 - fermig;
                        }

                        var wkfngs = wkfng - (3.25 + .5 * EgFet1 + fermis);
                        if (!ModelParameters.Gamma.Given)
                        {
                            ModelParameters.Gamma.RawValue =
                                Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Circuit.Charge * ModelParameters.SubstrateDoping * 1e6) /
                                ModelParameters.OxideCapFactor;
                        }

                        if (!ModelParameters.Vt0.Given)
                        {
                            if (!ModelParameters.SurfaceStateDensity.Given)
                                ModelParameters.SurfaceStateDensity.RawValue = 0;
                            var vfb = wkfngs - ModelParameters.SurfaceStateDensity * 1e4 * Circuit.Charge / ModelParameters.OxideCapFactor;
                            ModelParameters.Vt0.RawValue = vfb + ModelParameters.MosfetType * (ModelParameters.Gamma * Math.Sqrt(ModelParameters.Phi) + ModelParameters.Phi);
                        }
                    }
                    else
                    {
                        ModelParameters.SubstrateDoping.RawValue = 0;
                        throw new CircuitException("{0}: Nsub < Ni".FormatString(Name));
                    }
                }
            }
        }
    }
}
