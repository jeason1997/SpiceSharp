﻿using System;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Part of the trapezoidal integration method
    /// </summary>
    public partial class Trapezoidal
    {
        /// <summary>
        /// A state that can be derived by the trapezoidal integration method.
        /// </summary>
        /// <seealso cref="StateDerivative" />
        /// <seealso cref="ITruncatable" />
        protected class TrapezoidalStateDerivative : StateDerivative, ITruncatable
        {
            // Private variables
            private readonly int _index;
            private readonly Trapezoidal _method;
            private readonly History<IntegrationState> _states;

            /// <summary>
            /// Gets or sets the value of the state at the current timepoint.
            /// </summary>
            /// <value>
            /// The current value.
            /// </value>
            public override double Current
            {
                get => _states[0].State[_index];
                set => _states[0].State[_index] = value;
            }

            /// <summary>
            /// Gets the <see cref="System.Double"/> at the specified index.
            /// </summary>
            /// <value>
            /// The <see cref="System.Double"/>.
            /// </value>
            /// <param name="index">The index.</param>
            /// <returns></returns>
            public override double this[int index] => _states[index].State[_index];

            /// <summary>
            /// Gets the current derivative.
            /// </summary>
            /// <value>
            /// The derivative value.
            /// </value>
            public override double Derivative => _states[0].State[_index + 1];

            /// <summary>
            /// Initializes a new instance of the <see cref="TrapezoidalStateDerivative"/> class.
            /// </summary>
            /// <param name="method">The trapezoidal method.</param>
            public TrapezoidalStateDerivative(Trapezoidal method)
            {
                _method = method;
                _index = method.StateManager.AllocateState(1);
                _states = method.IntegrationStates;
            }

            /// <summary>
            /// Calculate contribution to the jacobian matrix (or Y-matrix).
            /// </summary>
            /// <param name="derivative">Derivative of the state variable with respect to the unknown variable.</param>
            /// <returns>
            /// A value that can be added to the element in the Y-matrix.
            /// </returns>
            /// <remarks>
            /// The value returned by this method means that the state variable depends on the derivative of an unknown variable (eg.
            /// the voltage across a capacitor). <paramref name="derivative" /> is the derivative of the state variable w.r.t. the
            /// unknown variable.
            /// </remarks>
            public override double Jacobian(double derivative) => derivative * _method.Slope;

            /// <summary>
            /// Calculate contribution to the rhs vector (right-hand side vector).
            /// </summary>
            /// <returns>
            /// A value that can be added to the element in the right-hand side vector.
            /// </returns>
            /// <remarks>
            /// The state variable is assumed to be linearly dependent of the unknown variables
            /// it is derived of. Ie. Q = dqdv * v (v is the unknown).
            /// </remarks>
            public override double RhsCurrent() =>
                _states[0].State[_index + 1] - _method.Slope * _states[0].State[_index];

            /// <summary>
            /// Calculates the derivative.
            /// </summary>
            /// <exception cref="SpiceSharp.CircuitException">Invalid order</exception>
            public override void Integrate()
            {
                var derivativeIndex = _index + 1;
                var current = _states[0].State;
                var previous = _states[1].State;
                var ag = _method.Coefficients;

                switch (_method.Order)
                {
                    case 1:
                        current[derivativeIndex] = ag[0] * current[_index] + ag[1] * previous[_index];
                        break;

                    case 2:
                        current[derivativeIndex] = -previous[derivativeIndex] * ag[1] +
                                                   ag[0] * (current[_index] - previous[_index]);
                        break;

                    default:
                        throw new CircuitException("Invalid order");
                }
            }

            /// <summary>
            /// Truncates the current timestep.
            /// </summary>
            /// <returns>
            /// The maximum timestep allowed by this state.
            /// </returns>
            public double Truncate()
            {
                var derivativeIndex = _index + 1;
                var current = _states[0].State;
                var previous = _states[1].State;

                var diff = new double[_method.MaxOrder + 2];
                var deltmp = new double[_states.Length];

                // Calculate the tolerance
                var volttol =
                    _method.AbsTol + _method.RelTol * Math.Max(Math.Abs(current[derivativeIndex]), Math.Abs(previous[derivativeIndex]));
                var chargetol = Math.Max(Math.Abs(current[_index]), Math.Abs(previous[_index]));
                chargetol = _method.RelTol * Math.Max(chargetol, _method.ChgTol) / _states[0].Delta;
                var tol = Math.Max(volttol, chargetol);

                // Now compute divided differences
                var j = 0;
                foreach (var state in _states)
                {
                    diff[j] = state.State[_index];
                    deltmp[j] = state.Delta;
                    j++;
                }

                j = _method.Order;
                while (true)
                {
                    for (var i = 0; i <= j; i++)
                        diff[i] = (diff[i] - diff[i + 1]) / deltmp[i];
                    if (--j < 0)
                        break;
                    for (var i = 0; i <= j; i++)
                        deltmp[i] = deltmp[i + 1] + _states[i].Delta;
                }

                // Calculate the new timestep
                double factor = double.NaN;
                switch (_method.Order)
                {
                    case 1: factor = 0.5;
                        break;
                    case 2: factor = 0.0833333333;
                        break;
                }

                var del = _method.TrTol * tol / Math.Max(_method.AbsTol, factor * Math.Abs(diff[0]));
                if (_method.Order == 2)
                    del = Math.Sqrt(del);
                return del;
            }
        }
    }
}
