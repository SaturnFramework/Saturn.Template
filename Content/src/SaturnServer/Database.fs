module Database

open Dapper
open System.Data.Common
open System.Collections.Generic
open Giraffe.Tasks

let inline (=>) k v = k, box v

let execute (connection:#DbConnection) (sql:string) (data:_) =
    task {
        try
            let! res = connection.ExecuteAsync(sql, data)
            return Ok res
        with
        | ex -> return Error ex
    }

let query (connection:#DbConnection) (sql:string) (parameters:IDictionary<string, obj> option) =
    task {
        try
            let! res =
                match parameters with
                | Some p -> connection.QueryAsync<'T>(sql, p)
                | None    -> connection.QueryAsync<'T>(sql)
            return Ok res
        with
        | ex -> return Error ex
    }

let querySingle (connection:#DbConnection) (sql:string) (parameters:IDictionary<string, obj> option) =
    task {
        try
            let! res =
                match parameters with
                | Some p -> connection.QuerySingleAsync<'T>(sql, p)
                | None    -> connection.QuerySingleAsync<'T>(sql)
            return Ok res
        with
        | ex -> return Error ex
    }
