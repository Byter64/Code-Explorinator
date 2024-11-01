# GameLab Project Repository

|  General Info  | |
| ---|---|
| Working Title | `Code Explorinator` |
| Final Title | `Code Explorinator` |
| Student | `Yannik Stamm`, `yannik.stamm`@stud-mail.uni-wuerzburg.de, `s431656`; `Veronika Strümper`, `veronika.struemper`@stud-mail.uni-wuerzburg.de, `s427038` |
| Target Platform(s) | `Windows` |
| Start Date | 17.10.2022 |
| Study Program | Games Engineering B.Sc.|
| Engine Version | Unity 2022.2.1f1|

# Start
The Quick Start Guide can be found in the wiki

## Abstract

Code Explorinator is a Unity Plugin that helps you visually explore code. The days of spaghetti code are over! It uses the C# Roslyn compiler to analyze your code within a unity project and displays it as an interactive class diagram.

The project is displayed as an UML-like diagram through which you can navigate by scrolling and zooming. An adjustable graph size and a list with all types (classes, structs, interfaces, ...) declared in your code help you get an overview of the project's structure.

## Structural notes
### Communication between backend and frontend
1. Frontend asks backend for specific class via a unique class identifier, that has already been given by backend.

2. Interface between backend and frontend
2.1 General idea
   The frontend gives the backend a class and a radius. The backend then returns the resulting graph in form of dictionaries.
2.1 Datatstructure
   2.1.1 INamedTypeSymbol (von Roslyn)
2.2 
