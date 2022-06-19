# WPP4DotNet

> WPP4DotNet is an open source project developed by the community through the lib
> [wppconnect-team/wa-js](https://github.com/wppconnect-team/wa-js) becoming a library compatible with .Net Framework and .Net Core and can be used with C#, F# and VB.NET.

## Install

Quite simple installation use Nuget or command line (**will be published on NuGet soon**)
```bash
Install-Package wpp4dotnet
```

### Start Service (C#)

```c#
    private IWpp _wpp;
    public void StartService(IWpp wpp, string session = "")
    {
        _wpp = wpp;
        _wpp.StartSession(session, true);
        thr = new Thread(Service);
        thr.IsBackground = true;
        thr.Start();
    }
    public async void Service()
    {
      //Implement the services that will run in the background.
    }
```
### Start Service (VB.NET)

```vb.net
    Private _wpp As IWpp

    Public Sub StartService(ByVal wpp As IWpp, ByVal Optional session As String = "")
        _wpp = wpp
        _wpp.StartSession(session, True)
        thr = New Thread(AddressOf Service)
        thr.IsBackground = True
        thr.Start()
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
