Erledigt:
- implemented interfaces etc.
- improved spring embedder
- explicit interface implementation supported
- tested if explicit parameters work
-  => syntax für methoden unterstützt

Dringend:
- BUG: multiple selection fixen (shift drücken wird weird erkannt) & man kann nicht suchen und danach noch mehr sachen auswählen
- Methodenebene hinzufügen
- Vererbung von Methoden unterstützen
- Alle Modifier unterstützen
- Fokusklassen sollen am selben Ort bleiben
- anordnung verbessern

Wichtig:
- Einstellungen des Nutzers abspeichern

Extras:
- method overrides oder implementation references still need to be shown
-Fokussierte Klassen/Methoden hervorheben
Warnung einabuen, dass Dateien mit Sonderzeichen mit der richtigen Codierung abgespeichert gemeint

Für nach der Expo:
- BUG: Zoom fokoussiert nicht perfekt auf Mauszeiger
- Alle Klassen außer Fokusklassen am Anfang einklappen
- Typkonvertierung unterstützen
- Operatorenüberladung unterstützen
- Delegate unterstützen

Arbeitszeiten:
19.05.23:
10:00-15:00 Uhr: Springalgorithmus drüber geschaut, Mehrfachauswahl für Methodenebene angefangen (Y)
10:00-15:00 Uhr: interfaces/records/structs implementiert, spring algo verbessert, getestet ob explizite parameter und explizite interfaces funktionieren, modifier testen angefangen (V)

`Please edit this README to fit your project and keep it up-to-date with your concept.`

`All final deliverables (code, executable game, report, trailer and poster) have to be committed, tagged as final and pushed to your GitLab repository.`

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

### Abstract

`Code Explorinator is a Unity Plugin that helps you visually explore code. The of days spaghetti code are numbered! It uses the C# Roslyn compiler to analyze the code and display it as an interactive UML Diagram.`

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

### Project and Source Control

Read more about Git in the [Atlassian Git Tutorials](https://de.atlassian.com/git).

#### Avoiding Clutter with .gitignore
Gitignore files allow to exclude certain patterns from being versioned.
This is necessary to avoid unnecessary (and possibly harmful) cluttering of your repository.
Especially the automatically generated project and cache files of VisualStudio, Unity, or Unreal projects should be ignored.

You can find [a selection of *.gitignore* files publicly available on GitHub](https://github.com/github/gitignore).

##### Quick Check if .gitignore is working

Your *.gitignore* is not correctly set up, if
* your repository contains Folders such as `Library`, `DerivedDataCache` or `Saved`
* `cache` files, `visual studio` project files etc. are `shown as modified` before commiting with your git client

In this case, check your setup.
Be aware that *.gitignore* is the actual, required filename!

#### Versioning Binary Assets with Git LFS and .gitattributes
Gitattribute files define file types to be handled through the Git Large File Storage (Git LFS) System.
This system does not handle binary files, such as assets, images, meshes, etc. well.
Even minimal changes add the whole file to the projects history.
Git LFS identifies iterations of binary files using a hash in the repository, but stores the actual binary data transparently in a seperate data silo.

To let Git LFS track a certain file (e.g. recursively all *.jpg*), execute this command:

	> git lfs track *.jpg

This command creates the following entry in the *.gitattributes* file:

	*.jpg filter=lfs diff=lfs merge=lfs -text

Git LFS is installed on all Workstations in E37 and the GameLabs.
For your private computer, you can [download Git LFS here](https://git-lfs.github.com/).

#### Further Reading: 
* [Epic on Git for Unreal](https://wiki.unrealengine.com/Git_source_control_(Tutorial)#Workarounds_for_dealing_with_binary_files_on_your_Git_repository)
* [GitLFS](https://www.git-lfs.com)
* [Git](https://www.git-scm.com)

