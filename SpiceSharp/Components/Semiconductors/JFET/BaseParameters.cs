﻿using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the temperature in degrees celsius.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        [ParameterName("temp"), ParameterInfo("Instance temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Circuit.CelsiusKelvin;
            set => Temperature.Value = value + Circuit.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature in Kelvin.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        public GivenParameter<double> Temperature { get; } = new GivenParameter<double>(300.15);

        /// <summary>
        /// Gets the area.
        /// </summary>
        /// <value>
        /// The area.
        /// </value>
        [ParameterName("area"), ParameterInfo("Area factor")]
        public GivenParameter<double> Area { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Gets the initial D-S voltage.
        /// </summary>
        /// <value>
        /// The initial D-S voltage.
        /// </value>
        [ParameterName("ic-vds"), ParameterInfo("Initial D-S voltage")]
        public GivenParameter<double> InitialVds { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the initial G-S voltage.
        /// </summary>
        /// <value>
        /// The initial G-S voltage.
        /// </value>
        [ParameterName("ic-vgs"), ParameterInfo("Initial G-S voltage")]
        public GivenParameter<double> InitialVgs { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is off.
        /// </summary>
        /// <value>
        ///   <c>true</c> if off; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Sets the initial conditions of the JFET.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <exception cref="ArgumentNullException">values</exception>
        /// <exception cref="BadParameterException">values</exception>
        [ParameterName("ic"), ParameterInfo("Initial VDS,VGS vector")]
        public void SetIc(double[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            switch (values.Length)
            {
                case 2:
                    InitialVgs.Value = values[1];
                    goto case 1;
                case 1:
                    InitialVds.Value = values[0];
                    break;
                default:
                    throw new BadParameterException(nameof(values));
            }
        }
    }
}
