module Program

open System.Reflection
open SimpleMigrations
open Microsoft.Data.Sqlite
open SimpleMigrations.DatabaseProvider
open SimpleMigrations.Console


[<EntryPoint>]
let main argv =
    let assembly = Assembly.GetExecutingAssembly()
    use db = new SqliteConnection "DataSource=src/SaturnServer.1/database.sqlite"
    let provider = SqliteDatabaseProvider(db)
    let migrator = SimpleMigrator(assembly, provider)
    let consoleRunner = ConsoleRunner(migrator)
    consoleRunner.Run(argv) |> ignore
    0