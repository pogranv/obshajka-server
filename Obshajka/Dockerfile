FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

ENV ASPNETCORE_ENVIRONMENT=Development

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Obshajka/Obshajka.csproj", "Obshajka/"]
RUN dotnet restore "Obshajka/Obshajka.csproj"
COPY . .
WORKDIR "/src/Obshajka"
RUN dotnet build "Obshajka.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Obshajka.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Obshajka.dll"]
