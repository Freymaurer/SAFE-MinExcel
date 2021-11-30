# SAFE-MinExcel

This is meant as a minimal template to set up an Office Excel web add-in. 

Tested for: 
- Excel online
- Excel 356 on windows
- Excel 2019 on windows



## Install pre-requisites

You'll need to install the following pre-requisites in order to build SAFE applications

* [.NET Core SDK](https://www.microsoft.com/net/download) 5.0 or higher
* [Node LTS](https://nodejs.org/en/download/)

1. Download repo.
2. Run install.cmd as admin **or** run `dotnet run setup`.
3. To start the app run `dotnet run officedebug` in the root directory.
4. Webside can be accessed under 'https://localhost:3000'
5. Start Excel and go to `Insert` > `My Add-ins` > click on the small arrow and it should show **Minimal-SAFE**.

## SAFE Stack Documentation

If you want to know more about the full Azure Stack and all of it's components (including Azure) visit the official [SAFE documentation](https://safe-stack.github.io/docs/).

You will find more documentation about the used F# components at the following places:

* [Saturn](https://saturnframework.org/)
* [Fable](https://fable.io/docs/)
* [Elmish](https://elmish.github.io/elmish/)
