nuget restore -SolutionDirectory ../  ../RockLib.Configuration/RockLib.Configuration.csproj

msbuild /p:Configuration=Release /t:Clean ..\RockLib.Configuration\RockLib.Configuration.csproj

msbuild /p:Configuration=Release /t:Rebuild ..\RockLib.Configuration\RockLib.Configuration.csproj

msbuild /t:pack /p:PackageOutputPath=..\builtPackages  /p:Configuration=Release ..\RockLib.Configuration\RockLib.Configuration.csproj
