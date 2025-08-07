# GitHub Copilot Instructions for Aquarium (Continued) Mod Development

## Mod Overview and Purpose

**Aquarium (Continued)** is a mod for RimWorld that enhances the player experience by introducing aquarist (fish keeping) mechanics, allowing pawns to maintain aquariums with tropical fish. The mod improves the aesthetic and interactive elements of the game, providing mood boosts, recreation, and meditation opportunities for pawns through the care and observation of aquarium fish.

## Key Features and Systems

- **New Fish Species**: The mod includes a wide variety of fish species, ranging from Arowana to Celestial Pearl Danio, each bringing diversity and beauty to the aquariums.
  
- **Aquarium Management**: 
  - Visual indicators for stack sizes of pet food and fish meat.
  - Enhanced textures for fish meat for a more realistic appearance.
  - Improved fish animations that simulate realistic swimming behavior.
  - User-friendly interface for adding fish, with options to add random fish or fill the tank completely.
  
- **Customizable Tanks**:
  - Multiple tank sizes (4x4, 4x2, 2x4) to suit player needs, with varying fish holding capacity.
  - Customizable sand and decoration options, increasing both visual appeal and tank beauty based on contents.

- **Quality of Life Improvements**:
  - Fish tanks' beauty scales with type and amount of fish.
  - Degradation mechanics are paused when no fish are present.
  - Multiplayer support enhancements and consistent optimization.
  
- **Integration and Compatibility**:
  - Compatibility with various mods, including Vanilla Nutrient Paste Expanded and others that extend feeding mechanics.
  - Multiplayer support ensuring seamless interactions.

- **Localization**: Translations available for German and French languages.

## Coding Patterns and Conventions

- **Class Structure**: The mod uses an object-oriented design with classes like `CompAquarium`, `Comp_Aquarium_VNPE`, and specialized job drivers (`JobDriver_AQCleaning`, `JobDriver_AQFeeding`).
- **Code Comments**: Consistent use of comments for clarity in complex methods and logic.
- **Naming Conventions**: Follows C# standards with PascalCase for classes and methods, camelCase for private fields, and descriptive names conveying function purpose.

## XML Integration

- All fish, tanks, and decor objects are defined in XML to facilitate easy additions and modifications. Utilize RimWorld’s Def systems for defining ThingDefs and ThingComps (e.g., `CompProperties_AQFishInBag`).

## Harmony Patching

- Harmony is used to extend or alter base game functionalities to support unique mod features like aquarium feeding and behavior logic.
- Patches should be managed within classes like `Controller.cs` and utilize best practices (prefix/postfix) for minimal performance impact.

## Suggestions for Copilot

- **Generate Fish Interaction Logic**: Use Copilot to assist in creating advanced interaction logic for fish feeding, movement, and beauty calculations.
- **Expansion and Localization**: Aid in writing additional translation keys or expansion features.
- **Algorithm Optimization**: Recommend more efficient algorithms when handling large lists of fish data.
- **Safety Checks and Error Handling**: Assist in implementing robust error handling to prevent crashes during unexpected interactions.

These directives aim to maintain code consistency while leveraging GitHub Copilot for innovative solutions and efficient code generation.
