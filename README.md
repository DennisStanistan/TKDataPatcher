# TKDataPatcher
Allows to add custom item slots, highly beta. made it back in late 2020 but never had the time to polish and release it to the public
Uses .NET Core 3.1


## Download
Download the latest version from [TekkenMods](https://tekkenmods.com/mod/2301/tkdatapatcher)

## Publishing
```bash
dotnet publish --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true --output ./publish
```