dotnet build -c Release
set OutPath=C:\Users\Administrator\Documents\Klei\OxygenNotIncluded\mods\Dev\AutoMaterial
copy Readme.md %OutPath%
cd AutoMaterial
copy bin\Release\AutoMaterial.dll %OutPath%
copy mod.yaml %OutPath%
copy mod_info.yaml %OutPath%
copy AutoMaterialConfig.json %OutPath%