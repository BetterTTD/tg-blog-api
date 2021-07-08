module Program

open System
open FSharp.Control.Tasks
open Giraffe

open Articles
open ArticleRepository

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open MongoDB.Driver


let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

let articlesHandler : HttpHandler =
    fun next context ->
        let find = context.GetService<ArticleFind>()
        let articles = find ArticleCriteria.All
        json articles next context

let addArticleHandler : HttpHandler =
    fun next context ->
        task {
            let save = context.GetService<ArticleSave>()
            let! article = context.BindJsonAsync<Article>()
            let article = { article with Id = ShortGuid.fromGuid(Guid.NewGuid()) }
            return! json (save article) next context
        }

let updateArticleHandler id : HttpHandler =
    fun next context ->
        task {
            let save = context.GetService<ArticleSave>()
            let! article = context.BindJsonAsync<Article>()
            let article = { article with Id = id }
            return! json (save article) next context
        }

let deleteArticleHandler id : HttpHandler =
    fun next context ->
        task {
            let delete = context.GetService<ArticleDelete>()
            return! json (delete id) next context
        }

let webApi =
    choose [
        GET >=>
            choose [
                route  "/"          >=>  text "index"
                route  "/ping"      >=>  text "pong"
                route  "/articles"  >=>  articlesHandler
            ]
            
        POST >=>
            choose [
                route "/articles"   >=>  addArticleHandler
            ]
            
        PUT >=>
            choose [
                routef "/articles/%s" (fun id -> updateArticleHandler id)
            ]
            
        DELETE >=>
            choose [
                routef "/articles/%s" (fun id -> deleteArticleHandler id)
            ]
            
        RequestErrors.notFound (text "Not Found") ]

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler)
       .UseStaticFiles()
       .UseResponseCaching()
       .UseGiraffe webApi

let configureServices (services : IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let cfg = serviceProvider.GetService<IConfiguration>()
    let mongo = MongoClient (cfg.GetConnectionString "BlogDb")
    let db = mongo.GetDatabase "blog"
    
    
    services.AddArticleRepository(db.GetCollection<Article>("articles"))
            .AddResponseCaching()
            .AddGiraffe()
            .AddDataProtection() |> ignore

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