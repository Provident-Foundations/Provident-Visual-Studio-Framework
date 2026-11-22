![Provident Foundations banner](../../Core/Provident%20Foundations%20Framework/Provident.Foundations/PFBanner.png)

# Provident Foundations Visual Studio Framework

A library for working with Visual Studio solution, project, source and XAML information in .NET applications. It provides models and analysis services used by the Provident Foundations developer tools and UI libraries; it is not itself a Visual Studio extension or IDE.

## What's included

- Solution and project models and loaders, with MSBuild and Roslyn dependencies.
- C# class-layout and naming analysis, plus configurable code-review rules.
- XAML file, resource and style analysis and validation.
- Developer-tool and icon helpers.

The code-review analyzer implements a limited set of automated checks; categories without rules still require manual review. Its coverage descriptions identify those limitations.

## Requirements and use

The project targets **.NET 11** (`net11.0`) and currently specifies x64 as its platform target. Reference the project from a compatible application. Loading projects depends on MSBuild and may require the appropriate SDKs and build environment for the projects being inspected. The library references the core Provident Foundations project.
