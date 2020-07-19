module Program

open System.Reflection
open SimpleMigrations
open Npgsql
open SimpleMigrations.DatabaseProvider
open SimpleMigrations.Console


[<EntryPoint>]
let main argv =
    let assembly = Assembly.GetExecutingAssembly()
    let connStr = "Host=localhost;Username=postgres;password=postgres;Database=postgres"
    use db = new NpgsqlConnection(connStr)
    let provider = PostgresqlDatabaseProvider(db)
    let migrator = SimpleMigrator(assembly, provider)
    let consoleRunner = ConsoleRunner(migrator)
    consoleRunner.Run(argv) |> ignore
    0