﻿using System;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseAcceptBehavior" />
    public class AcceptBehavior : BaseAcceptBehavior
    {
        private BaseParameters _bp;

        // Necessary behaviors and parameters
        private TransientBehavior _tran;
        private double _oldSlope1, _oldSlope2;
        private bool _wasBreak = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public AcceptBehavior(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();

            // Get transient
            _tran = provider.GetBehavior<TransientBehavior>();
        }

        /// <summary>
        /// Called when a new timepoint is being tested.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Probe(TimeSimulation simulation)
        {
            var breakpoint = _wasBreak;
            if (simulation.Method is IBreakpoints method)
                breakpoint |= method.Break;

            _tran.Signals.Probe(simulation.Method.Time, breakpoint);
        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            if (simulation.Method is IBreakpoints method)
            {
                // The integration method supports breakpoints, let's see if we need to add one
                if (_wasBreak || method.Break)
                {
                    // Calculate the slope reaching this accepted solution
                    double slope1, slope2;
                    if (method.Time.Equals(0.0))
                    {
                        // The first timepoint is assumed to have a slope of 0
                        slope1 = 0.0;
                        slope2 = 0.0;
                    }
                    else
                    {
                        var signals = _tran.Signals;
                        var delta = signals.GetTime(0) - signals.GetTime(1);
                        slope1 = (signals.GetValue(0, 0) - signals.GetValue(1, 0)) / delta;
                        slope2 = (signals.GetValue(0, 1) - signals.GetValue(1, 1)) / delta;
                    }

                    // If the previous point was a breakpoint, let's decide if we need another in the future
                    if (_wasBreak)
                    {
                        var tol1 = _bp.RelativeTolerance * Math.Max(Math.Abs(slope1), Math.Abs(_oldSlope1)) +
                                  _bp.AbsoluteTolerance;
                        var tol2 = _bp.RelativeTolerance * Math.Max(Math.Abs(slope2), Math.Abs(_oldSlope2)) +
                                   _bp.AbsoluteTolerance;
                        if (Math.Abs(slope1 - _oldSlope1) > tol1 || Math.Abs(slope2 - _oldSlope2) > tol2)
                            method.Breakpoints.SetBreakpoint(_tran.Signals.GetTime(1) + _bp.Delay);
                    }

                    // Track for the next time
                    _oldSlope1 = slope1;
                    _oldSlope2 = slope2;
                    _wasBreak = method.Break;
                }
            }

            _tran.Signals.AcceptProbedValues();
        }
    }
}
