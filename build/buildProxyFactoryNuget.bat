nuget restore -SolutionDirectory ../  ../RockLib.Configuration.ProxyFactory/RockLib.Configuration.ProxyFactory.csproj

msbuild /p:Configuration=Release /t:Clean ..\RockLib.Configuration.ProxyFactory\RockLib.Configuration.ProxyFactory.csproj

msbuild /p:Configuration=Release /t:Rebuild ..\RockLib.Configuration.ProxyFactory\RockLib.Configuration.ProxyFactory.csproj

msbuild /t:pack /p:PackageOutputPath=..\builtPackages  /p:Configuration=Release ..\RockLib.Configuration.ProxyFactory\RockLib.Configuration.ProxyFactory.csproj
