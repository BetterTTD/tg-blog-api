module ArticleRepository

open Articles
open MongoDB.Driver
open Microsoft.Extensions.DependencyInjection

let find (collection : IMongoCollection<Article>) (criteria : ArticleCriteria) =
    match criteria with
    | All        ->  collection.Find(Builders.Filter.Empty).ToEnumerable() |> Seq.toArray
    | Favourite  ->  collection.Find(Builders.Filter.Empty).ToEnumerable() |> Seq.toArray

let save (collection : IMongoCollection<Article>) (article : Article) =
    let articles = collection.Find(fun x -> x.Id = article.Id).ToEnumerable()
    let filter =
        Builders<Article>.Filter.Eq((fun x -> x.Id), article.Id)
    let updater =
        Builders<Article>.Update
            .Set((fun x -> x.Title),  article.Title)
            .Set((fun x -> x.Text),   article.Text)
            .Set((fun x -> x.Date),   article.Date)
            .Set((fun x -> x.UserId), article.UserId)
            
    match Seq.isEmpty articles with
    | true   ->  collection.InsertOne article
    | false  ->  collection.UpdateOne(filter, updater) |> ignore

    article
    
let delete (collection : IMongoCollection<Article>) (id : string) : bool =
    collection.DeleteOne(Builders<Article>.Filter.Eq((fun x -> x.Id), id)).DeletedCount > 0L

type IServiceCollection with
    member this.AddArticleRepository(collection : IMongoCollection<Article>) =
        this.AddSingleton<ArticleFind>(find collection)     |> ignore
        this.AddSingleton<ArticleSave>(save collection)     |> ignore
        this.AddSingleton<ArticleDelete>(delete collection) |> ignore