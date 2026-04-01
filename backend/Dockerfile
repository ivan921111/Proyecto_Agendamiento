FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./Scheduling.Api/Scheduling.Api.csproj ./Scheduling.Api/
RUN dotnet restore ./Scheduling.Api/Scheduling.Api.csproj

COPY . .
RUN dotnet publish ./Scheduling.Api/Scheduling.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "Scheduling.Api.dll"]