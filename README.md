# GameLab Project Repository

|  General Info  | |
| ---|---|
| Working Title | `Code Explorinator` |
| Final Title | `Edit me!` |
| Student | `Yannik Stamm`, `yannik.stamm`@stud-mail.uni-wuerzburg.de, `s431656`; `Veronika Strümper`, `veronika.struemper`@stud-mail.uni-wuerzburg.de, `s427038` |
| Target Platform(s) | `Windows` `MacOS` `Linux` |
| Start Date | 21.10.2019 |
| Study Program | Games Engineering B.Sc.|
| Engine Version | Unity 2021.3.11f1 or Unreal 5.0.3 |

# Structur notes
## Communication between backend and frontend
1. Frontend asks backend for specific class via a unique class identifier, that has already been given by backend.

2. Interface between backend and frontend
2.1 General idea
   The frontend gives the backend a class and a radius. The backend then returns the resulting graph in form of dictionaries.
2.1 Datatstructure
   2.1.1 INamedTypeSymbol (von Roslyn)
2.2 

### Abstract

Code Explorinator is a Unity Plugin that helps you visually explore code. The days of spaghetti code are over! It uses the C# Roslyn compiler to analyze your code within a unity project and displays it as an interactive class diagram.

The project is displayed as an UML-like diagram through which you can navigate by scrolling and zooming. An adjustable graph size and a list with all types (classes, structs, interfaces, ...) declared in your code help you get an overview of the project's structure.

`--- 8< --- READ, THEN REPLACE WITH CUSTOM CONTENT BELOW HERE --- 8< ---`

## Repository Usage Guides

```
RepositoryRoot/
    ├── README.md           // This should reflect your project 
    │                       //  accurately, so always merge infor- 
    │                       //  mation from your concept paper 
    │                       //  with the readme
    ├── builds/             // Archives (.zip) of built executables of your projects
    │                       //  including (non-standard) dependencies
    ├── code/
    │   ├── engine/         // Place your project folder(s) here
    │   ├── my-game-1/      // No un-used folders, no "archived" folders
    │   ├── CMakeLists.txt  // e.g. if using CMake, this can be your project root
    │   └── ...
    ├── documentation/      // GL2/3 - Each project requires FULL documentation  
    │                       //   i.e. API Docs, Handbook, Dev Docs
    ├── poster/             // PDF of your Poster(s)
    ├── report/             // PDF
    └── trailer/            // .mp4 (final trailer, no raw material)
```
