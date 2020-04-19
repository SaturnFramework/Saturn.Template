### How to build application

1. Make sure you have installed version of .Net SDK defined in `global.json`
2. Run `dotnet tool restore` to restore all necessary tools
3. Run `dotnet saturn migration` to create sqlite database
3. Run `dotnet fake build -t Run` to start application in watch mode (automatic recompilation and restart at file save)

### How to use `dotnet saturn`

Templates comes with `Saturn.Cli` tool installed by default. It's a CLI tool that can be used for project scaffold and managing generated DB migrations

#### Commands

`dotnet saturn` supports following commands:

* `gen NAME NAMES COLUMN:TYPE COLUMN:TYPE COLUMN:TYPE ...` - creates model, database layer, views and controller returning HTML views
* `gen.json NAME NAMES COLUMN:TYPE COLUMN:TYPE COLUMN:TYPE ...` - creates model, database layer and JSON API controller
* `gen.model NAME NAMES COLUMN:TYPE COLUMN:TYPE COLUMN:TYPE ...` - creates model and database layer
* `migration` - runs all migration scripts for the database

#### Supported Types

Generator supports following types:

* `string`
* `int`
* `float`
* `double`
* `decimal`
* `guid`
* `datetime`
* `bool`
