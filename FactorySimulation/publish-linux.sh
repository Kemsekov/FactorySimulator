dotnet publish -c Release -r linux-x64 --self-contained -p:PublishTrimmed=True -p:PublishSingleFile=True -p:TrimMode=Link -p:PublishReadyToRun=False -o publish/linux-x64