# MaterialImporter Documentation

## Overview
`MaterialImporter` is a utility class in Unity that ensures materials have their shaders correctly assigned at runtime. It's designed to fix potential shader assignment issues that might occur during the import process or when loading materials from different environments.

## Functionality
The class performs the following operations during the `Awake()` lifecycle method:

1. Loads materials from the Resources folder
2. Checks if each material has the correct shader assigned
3. Reassigns the appropriate shader if needed
4. Logs confirmation messages when fixes are applied

## Materials Managed
The class currently manages three materials:
- `ParticleMorphAdvancedMaterial` → Uses `Custom/ParticleMorphAdvanced` shader
- `ParticleMorphMaterial` → Uses `Custom/ParticleMorph` shader
- `RainbowAnimatedMaterial` → Uses `Custom/Rainbow` shader

## Usage
Attach this component to a GameObject in your scene that initializes early in the loading process. No additional configuration is required as the class automatically handles the material checks.

## Dependencies
- Requires the specified materials to be located in the `Resources/Materials/` folder
- Requires the custom shaders to be available in the project 