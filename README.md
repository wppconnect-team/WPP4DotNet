# WPP4DotNet

> WPP4DotNet is an open source project developed by the community through the lib
> [wppconnect-team/wa-js](https://github.com/wppconnect-team/wa-js) becoming a library compatible with .Net Framework and .Net Core and can be used with C#, F# and VB.NET.

## Our online channels

[![Discord](https://img.shields.io/discord/844351092758413353?color=blueviolet&label=Discord&logo=discord&style=flat)](https://discord.gg/JU5JGGKGNG)
[![Telegram Group](https://img.shields.io/badge/Telegram-Group-32AFED?logo=telegram)](https://t.me/wppconnect)
[![WhatsApp Group](https://img.shields.io/badge/WhatsApp-Group-25D366?logo=whatsapp)](https://chat.whatsapp.com/LJaQu6ZyNvnBPNAVRbX00K)
[![YouTube](https://img.shields.io/youtube/channel/subscribers/UCD7J9LG08PmGQrF5IS7Yv9A?label=YouTube)](https://www.youtube.com/c/wppconnect)

## Install

Quite simple installation use Nuget or command line:
```bash
Install-Package wpp4dotnet
```

### Start Service (C#)

```c#
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Service!");
            Wpp wpp = new Wpp();
            //Set to True if you want to hide the browser or just remove False.
            wpp.StartService(new ChromeWebApp(false));
            new Thread(new ThreadStart(wpp.Service));
        }
    }
    
    internal class Wpp
    {
        IWpp _wpp;
        internal void StartService(IWpp wpp)
        {
            _wpp = wpp;
            _wpp.StartSession();
        }
        internal async void Service()
        {
            //Implement the services that will run in the background.
        }
    }
```
### Start Service (VB.NET)

```vb.net
Friend Class Program
    Private Shared Sub Main(ByVal args As String())
        Console.WriteLine("Start Service!")
        Dim wpp As Wpp = New Wpp()
        'Set to True if you want to hide the browser or just remove False.
        wpp.StartService(New ChromeWebApp(False))
        New Thread(New ThreadStart(AddressOf wpp.Service))
    End Sub
End Class

Friend Class Wpp
    Private _wpp As IWpp
    Friend Sub StartService(ByVal wpp As IWpp)
        _wpp = wpp
        _wpp.StartSession()
    End Sub
    Friend Async Sub Service()
        'Implement the services that will run in the background.
    End Sub
End Class
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
