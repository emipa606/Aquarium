# GitHub Copilot Instructions for RimWorld Mod: Aquarium (Continued)

## Mod Overview and Purpose
Aquarium (Continued) is a RimWorld mod that allows pawns to become aquarists, providing items and AI mechanisms to keep and care for pet tropical fish. The mod adds numerous fish species, improved visuals, and additional features to enhance the experience of fish-keeping within the game.

## Key Features and Systems
- **Additional Fish Species**: A variety of fish species have been added, including popular aquarium fish like Arowana, Koi, and Clownfish.
- **Visual and Functional Improvements**: Improved textures for fish meat, smoother movement, and visual stack size for pet food and fish meat.
- **Enhanced User Interface**: Selector to add fish only shows available fish, options for random fish and decorations.
- **Tank Decorations**: Selectable sand and decorations, "Nothing"-option for tank decor.
- **Beauty and Maintenance**: Tank beauty based on fish inside, food and cleanliness do not degrade with no fish.
- **Compatibility**: Supports various other mods and the Odyssey DLC.
- **Multiplayer Support**: Native support for multiplayer.

## Coding Patterns and Conventions
- **C# File Structure**: The mod's C# files are organized into various classes that handle different aspects of the mod, such as utilities (`AQUtility.cs`), compatibility with Vanilla Nutrient Paste Expanded (`Comp_Aquarium_VNPE.cs`), components for fish in bags (`CompAQFishInBag.cs`), and the main controller (`Controller.cs`).
- **XML Integration**: Definitions for jobs, joy, and resources are defined in XML files under the `Defs` directory.
- **Harmony Patching**: Harmony is used for patching vanilla RimWorld methods to integrate new features and behaviors.
- **Code Comments**: Extensive comments are provided within the code to explain the functionality of each method and class.

## XML Integration
- **JobDefs**: Defines job definitions for fish-related tasks.
- **JoyDefs**: Defines joy giver and kind definitions related to fish-keeping.
- **ThingDefs**: Defines various things like fish meat, gelatin, and spirulina, including their recipes and properties.

## Harmony Patching
Harmony is a popular modding tool in RimWorld that allows developers to modify vanilla code without altering the original RimWorld files. Key patches in the Aquarium mod include:
- **Comp_Aquarium_VNPE**: Patches to handle interactions with the Vanilla Nutrient Paste Expanded mod.
- **Controller**: Various patches to integrate new features and behaviors into the game.

## Suggestions for Copilot
To effectively use Copilot in developing the Aquarium mod, consider the following suggestions:
- **Focus on XML Definitions**: When working with XML files, Copilot can be very helpful for generating and maintaining the structure and content of these definitions.
- **Utilize Harmony Patches**: For modifying vanilla behavior, Copilot can assist in generating the necessary Harmony patches.
- **Code Reuse**: Copilot can help identify opportunities for code reuse within the mod, such as utility methods that can be applied across multiple classes.
- **Documentation and Comments**: Use Copilot to generate comments for new methods and classes to ensure the code remains maintainable and understandable.

By following these guidelines, developers can leverage Copilot to enhance their productivity and maintain a high standard of quality in the Aquarium mod.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

