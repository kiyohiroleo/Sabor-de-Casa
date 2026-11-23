FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["SaborDeCasa.Api.csproj", "./"]

RUN dotnet restore "SaborDeCasa.Api.csproj"

COPY . .

RUN dotnet publish "SaborDeCasa.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "SaborDeCasa.Api.dll"]