module Articles

type Article =
    { Id     : string
      Title  : string
      UserId : int
      Date   : string
      Text   : string }
    
type ArticleCriteria =
    | All
    | Favourite
    
type ArticleFind = ArticleCriteria -> Article[]

type ArticleSave = Article -> Article

type ArticleDelete = string -> bool