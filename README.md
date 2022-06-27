# WPP4DotNet

> WPP4DotNet is an open source project developed by the community through the lib
> [wppconnect-team/wa-js](https://github.com/wppconnect-team/wa-js) becoming a library compatible with .Net Framework and .Net Core and can be used with C#, F# and VB.NET.

## Our online channels

[![Discord](https://img.shields.io/discord/844351092758413353?color=blueviolet&label=Discord&logo=discord&style=flat)](https://discord.gg/JU5JGGKGNG)
[![Telegram Group](https://img.shields.io/badge/Telegram-Group-32AFED?logo=telegram)](https://t.me/wppconnect)
[![WhatsApp Group](https://img.shields.io/badge/WhatsApp-Group-25D366?logo=whatsapp)](https://chat.whatsapp.com/C1ChjyShl5cA7KvmtecF3L)
[![YouTube](https://img.shields.io/youtube/channel/subscribers/UCD7J9LG08PmGQrF5IS7Yv9A?label=YouTube)](https://www.youtube.com/c/wppconnect)

## Install

Quite simple installation use Nuget or command line:
```bash
Install-Package wpp4dotnet
```

### Start Service (C#)

```c#
    private IWpp _wpp;
    private Thread _thr;
    
    public void StartService(IWpp wpp, string session = "")
    {
        _wpp = wpp;
        _wpp.StartSession(session, true);
        _thr = new Thread(new ThreadStart(Service));
    }
    
    public async void Service()
    {
      //Implement the services that will run in the background.
    }
```
### Start Service (VB.NET)

```vb.net
    Private _wpp As IWpp
    Private _thr As Thread

    Public Sub StartService(ByVal wpp As IWpp, ByVal Optional session As String = "")
        _wpp = wpp
        _wpp.StartSession(session, True)
        _thr = New Thread(New ThreadStart(Service))
    End Sub

    Public Async Sub Service()
      'Implement the services that will run in the background.
    End Sub
```
## Maintainers
Maintainers are needed, I cannot keep with all the updates by myself. If you are interested please open a Pull Request.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

Copyright 2021 WPPConnect Team

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
