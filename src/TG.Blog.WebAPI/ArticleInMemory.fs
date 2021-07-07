module Articles.ArticleInMemory

open Articles
open System.Collections
open Microsoft.Extensions.DependencyInjection

let find (inMemory : Hashtable) criteria =
    match criteria with
    | All -> inMemory.Values |> Seq.cast<Article> |> Array.ofSeq
    | Favourite -> failwith "not implemented exception for: 'find'"
    
let save (inMemory : Hashtable) article =
    inMemory.Add(article.Id, article)
    article
    

type IServiceCollection with
  member this.AddArticleInMemory (inMemory : Hashtable) =
    this.AddSingleton<ArticleFind>(find inMemory) |> ignore
    this.AddSingleton<ArticleSave>(save inMemory) |> ignore
    this