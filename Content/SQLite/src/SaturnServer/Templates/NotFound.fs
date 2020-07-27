module NotFound

open Giraffe.GiraffeViewEngine

let layout =
    html [_class "has-navbar-fixed-top"] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [encodedText "SaturnServer - Error #404"]
        ]
        body [] [
           h1 [] [rawText "ERROR #404"]
           a [_href "/" ] [rawText "Go back to home page"]
        ]
    ]