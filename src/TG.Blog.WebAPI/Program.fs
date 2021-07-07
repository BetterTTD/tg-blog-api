module Program

open System
open Giraffe

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

let webApi =
    choose [
        GET >=>
            choose [
                route  "/"           >=> text "index"
                route  "/ping"       >=> text "pong"
                route  "/error"      >=> (fun _ _ -> failwith "Something went wrong!")
            ]
        RequestErrors.notFound (text "Not Found") ]

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler)
       .UseStaticFiles()
       .UseResponseCaching()
       .UseGiraffe webApi

let configureServices (services : IServiceCollection) =
    services
        .AddResponseCaching()
        .AddGiraffe() |> ignore
    services.AddDataProtection() |> ignore

let configureLogging (loggerBuilder : ILoggingBuilder) =
    loggerBuilder.AddFilter(fun lvl -> lvl.Equals LogLevel.Error)
                 .AddConsole()
                 .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0