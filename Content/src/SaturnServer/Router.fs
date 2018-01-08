module Router

open Saturn.Pipeline
open Saturn.Router
open Giraffe.HttpHandlers

let browser = pipeline {
    plug acceptHtml
    plug putSecureBrowserHeaders
    plug fetchSession
    set_header "x-pipeline-type" "Browser"
}

let api = pipeline {
    plug acceptJson
    set_header "x-pipeline-type" "Api"
}

let defaultView = scope {
    get "/" (renderHtml Index.layout)
    get "/index.html" (redirectTo false "/")
    get "/default.html" (redirectTo false "/")
}

let browserRouter = scope {
    error_handler (renderHtml NotFound.layout) //Use the default 404 webpage
    pipe_through browser //Use the default browser pipeline

    forward "" defaultView //Use the default view
}

//Other scopes may use different pipelines and error handlers

// let apiRouter = scope {
//     error_handler (text "Api 404")
//     pipe_through api
//     get "/" (text "hello world")
//     getf "/%s/%i" (fun (str, age) -> text (sprintf "hello world, %s, You're %i" str age))
// }

let router = scope {
    error_handler (text "Top Router 404")
    // forward "/api" apiRouter
    forward "" browserRouter
}