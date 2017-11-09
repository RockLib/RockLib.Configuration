nuget restore -SolutionDirectory ../  ../RockLib.Configuration.ObjectFactory/RockLib.Configuration.ObjectFactory.csproj

msbuild /p:Configuration=Release /t:Clean ..\RockLib.Configuration.ObjectFactory\RockLib.Configuration.ObjectFactory.csproj

msbuild /p:Configuration=Release /t:Rebuild ..\RockLib.Configuration.ObjectFactory\RockLib.Configuration.ObjectFactory.csproj

msbuild /t:pack /p:PackageOutputPath=..\builtPackages  /p:Configuration=Release ..\RockLib.Configuration.ObjectFactory\RockLib.Configuration.ObjectFactory.csproj
