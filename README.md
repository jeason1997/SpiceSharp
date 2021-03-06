# <img src="https://spicesharp.github.io/SpiceSharp/api/images/logo_full.svg" width="45px" /> Spice#/SpiceSharp
Spice# is a Spice circuit simulator written in C#. The framework is made to be compatible with the original Berkeley Spice simulator, but bugs have been squashed and features can and will probably will be added.

## Documentation
You can find documentation at [https://spicesharp.github.io/SpiceSharp/](https://spicesharp.github.io/SpiceSharp/). There you can find a guide for **getting started**, as well as:
- Supported types of analysis.
- The general structure of Spice#.
- A tutorial on how to implement your own *custom* model equations (prerequisite knowledge needed).

## Quickstart

Simulating a circuit is relatively straightforward. For example:

```csharp
// Build the circuit
var ckt = new Circuit(
    new VoltageSource("V1", "in", "0", 0.0),
    new Resistor("R1", "in", "out", 1.0e3),
    new Resistor("R2", "out", "0", 2.0e3)
);

// Create a DC sweep and register to the event for exporting simulation data
var dc = new DC("dc", "V1", 0.0, 5.0, 0.001);
dc.ExportSimulationData += (sender, args) => Console.WriteLine(args.GetVoltage("out"));

// Run the simulation
dc.Run(ckt);
```

Most standard Spice-components are available, and building your own custom components is also possible!

## Installation
Spice# is available as a **NuGet Package**.

[![NuGet Badge](https://buildstats.info/nuget/spicesharp)](https://www.nuget.org/packages/SpiceSharp/) SpiceSharp

## Current build status

|    | Status |
|:---|-------:|
|AppVeyor CI (Windows)|[![Build status](https://ci.appveyor.com/api/projects/status/tg6q7y8m5725g8ou/branch/master?svg=true)](https://ci.appveyor.com/project/SpiceSharp/spicesharp/branch/master)|
|Travis CI (Linux/Mono)|[![Build Status](https://travis-ci.org/SpiceSharp/SpiceSharp.svg?branch=master)](https://travis-ci.org/SpiceSharp/SpiceSharp)|

## Aim of Spice#?

Spice# aims to be:
- A **Library** rather than a standalone piece of software like most simulators currently are.
- **Accessible** for both the amateur and advanced electronics enthusiast (and perhaps professional designer). In order to decrease the hurdle, a [Spice# parser](https://github.com/SpiceSharp/SpiceSharpParser) is also being developed. This also includes it being cross-platform (.NET and Mono).
- **Compatible** with the *original Spice 3f5* software (without the bugs). There's a reason why this has become the industry standard.
- **Customizable** with custom simulations, custom models, integration methods, solver, etc.
- **Performance**. Nobody wants a slow simulator.

## What Spice# is not

Having been implemented in the .NET framework does have some limitations, mainly involving *performance*.
- Unmanaged C++ code can often be optimized more.
- Spice# uses *Reflection* to try and give you a better experience. This also slightly hits performance. Don't worry though, it isn't used *during* simulation, only during setup.
