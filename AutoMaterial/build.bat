dotnet build -c Release
cd AutoMaterial
set OutPath=C:\Users\Administrator\Documents\Klei\OxygenNotIncluded\mods\Dev\AutoMaterial
copy bin\Release\AutoMaterial.dll %OutPath%
copy mod.yaml %OutPath%
copy mod_info.yaml %OutPath%
copy AutoMaterialConfig.json %OutPath%