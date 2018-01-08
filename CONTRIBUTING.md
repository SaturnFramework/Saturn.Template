# Contribution guideline

## Building the template

`build.cmd` / `build.sh`

## Testing template in development mode

### Testing directly

1. Change directory to `Content`
1. Trigger `build.cmd run` / `build.sh`

### Testing NuGet package

1. Build template from root directory
1. Uninstall currently installed template with `dotnet new -u Saturn.Template`
1. Install new version with `dotnet new -i <<repo-path>>/nupkg/Saturn.Template.<<version>>.nupkg`

## Known issues

* In case that dotnet -i fails with an 'Reference not set' error on Linux, try
  * Uninstall a previous version: `dotnet new -u Saturn.Template`
  * Install the new template with its path: `dotnet new -i ./nupkg/Saturn.Template.<<version>>.nupkg