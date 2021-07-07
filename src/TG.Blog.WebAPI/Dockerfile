FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY src/*/*.fsproj ./
RUN for file in $(ls *.fsproj); do \
        echo $file \
        && mkdir -p src/${file%.*}/ \
        && mv $file src/${file%.*}/; \
    done

COPY . ./

FROM build AS publish
RUN dotnet publish ./src/TG.Blog.WebAPI/TG.Blog.WebAPI.fsproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TG.Blog.WebAPI.dll"]
