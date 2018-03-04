# Saturn template

A dotnet CLI template for [Saturn](https://github.com/Krzysztof-Cieslak/Saturn) projects.


## Prerequisites

* [dotnet SDK 2.0.0](https://www.microsoft.com/net/core) together with dotnet CLI

## Using the template

* Install or update the template: `dotnet new -i Saturn.Template`
* Create a new project from the template: `dotnet new Saturn --name my-saturn-project --language F#`
  * Note: due to [NuGet behavior](https://github.com/SaturnFramework/Saturn.Template/issues/7) you have to provide a different name than working directory (e.g. `my-saturn-project`)
  * Note: `-lang F#` or `--language F#` is also currently required, due to [CLI issue](https://github.com/SAFE-Stack/SAFE-template/issues/28)


## How to contribute

*Imposter syndrome disclaimer*: I want your help. No really, I do.

There might be a little voice inside that tells you you're not ready; that you need to do one more tutorial, or learn another framework, or write a few more blog posts before you can help me with this project.

I assure you, that's not the case.

This project has some clear Contribution Guidelines and expectations that you can [read here](https://github.com/Krzysztof-Cieslak/Saturn.Template/blob/master/CONTRIBUTING.md).

The contribution guidelines outline the process that you'll need to follow to get a patch merged. By making expectations and process explicit, I hope it will make it easier for you to contribute.

And you don't just have to write code. You can help out by writing documentation, tests, or even by giving feedback about this work. (And yes, that includes giving feedback about the contribution guidelines.)

Thank you for contributing!


## Contributing and copyright

The project is hosted on [GitHub](https://github.com/Krzysztof-Cieslak/Saturn.Template) where you can [report issues](https://github.com/Krzysztof-Cieslak/Saturn.Template/issues), fork
the project and submit pull requests.

The library is available under [MIT license](https://github.com/Krzysztof-Cieslak/Saturn.Template/blob/master/LICENSE.md), which allows modification and redistribution for both commercial and non-commercial purposes.
