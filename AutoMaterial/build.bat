dotnet build -c Release
set OutPath=%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Dev\AutoMaterial
if not exist "%OutPath%\" mkdir "%OutPath%"
copy Readme.md %OutPath%
cd AutoMaterial
copy bin\Release\AutoMaterial.dll %OutPath%
copy mod.yaml %OutPath%
copy mod_info.yaml %OutPath%
copy AutoMaterialConfig.json %OutPath%