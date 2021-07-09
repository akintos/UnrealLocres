# UnrealLocres
UnrealEngine 4 `TextLocalizationResource` library and export/import tool built with C#.

Can read/write every locres version up to 3 (latest)

## Usage
`UnrealLocres` is a command line tool. You should use it in command line (cmd, powershell, etc.)

### Export
```
usage: UnrealLocres.exe export locres_file_path [-f {csv,pot}] [-o output_path]

positional arguments:
  locres_file_path        Input locres file path

optional arguments:
  -f, --format {csv,pot}  Output file format (csv, pot)
  -o                      Output file path (default: {locres_file_path}.{format})
```
Export locres file. Default output format is csv.
You should **never** change the key column. 

### Import
```
usage: UnrealLocres.exe import locres_file_path translation_file_path [-f {csv,pot}] [-o output_path]

positional arguments:
  locres_file_path        Input locres file path
  translation_file_path   Input translation file path

optional arguments:
  -f, --format {csv,pot}  Translation file format (csv, pot)
  -o                      Output locres file path (default: {locres_file_path}.new)
```
Import translation file into original locres file and create new translated locres file.

### Merge
```
usage: UnrealLocres.exe merge target_lucres_path source_lucres_path [-o output_path]

positional arguments:
  target_lucres_path      Merge target locres file path, the file you want to translate
  source_lucres_path      Merge source locres file path, the file that has additional lines

optional arguments:
  -o                      Output locres file path (default: {target_lucres_path}.new)
```
Merge two locres files into one, adding strings that are present in source but not in target file.

## LocresLib
### Sample usage

```cs
using LocresLib;

var locres = new LocresFile();

using (var file = File.OpenRead(inputPath))
{
    locres.Load(file);
}

foreach (var locresNamespace in locres)
{
    foreach (var stringEntry in locresNamespace)
    {
        string key = stringEntry.Key;
        string val = stringEntry.Value;

        // work with stringEntry
    }
}

using (var file = File.Create(outputPath))
{
    locres.Save(file, LocresVersion.Optimized);
}
```

### UE4 Source code
This library is based on original UE4 open source code

[TextKey.cpp](https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Source/Runtime/Core/Private/Internationalization/TextKey.cpp)

[TextKey.h](https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Source/Runtime/Core/Public/Internationalization/TextKey.h)

[TextLocalizationResourceVersion.h](https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Source/Runtime/Core/Public/Internationalization/TextLocalizationResourceVersion.h)

[TextLocalizationResource.h](https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Source/Runtime/Core/Public/Internationalization/TextLocalizationResource.h)

[TextLocalizationResource.cpp](https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Source/Runtime/Core/Private/Internationalization/TextLocalizationResource.cpp)

[Crc.h](https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Source/Runtime/Core/Public/Misc/Crc.h)

[Crc.cpp](https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Source/Runtime/Core/Private/Misc/Crc.cpp)

[CityHash.h](https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Source/Runtime/Core/Public/Hash/CityHash.h)

[CityHash.cpp](https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Source/Runtime/Core/Private/Hash/CityHash.cpp)
