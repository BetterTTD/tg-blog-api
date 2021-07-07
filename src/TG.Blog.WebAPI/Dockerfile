FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/TG.Blog.WebAPI/TG.Blog.WebAPI.fsproj", "TG.Blog.WebAPI/"]
RUN dotnet restore "src/TG.Blog.WebAPI/TG.Blog.WebAPI.fsproj"
COPY . .
WORKDIR "/src/TG.Blog.WebAPI"
RUN dotnet build "TG.Blog.WebAPI.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TG.Blog.WebAPI.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TG.Blog.WebAPI.dll"]
