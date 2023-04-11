#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Lekha.TaskGroup.Api/Lekha.TaskGroup.Api.csproj", "Lekha.TaskGroup.Api/"]
RUN dotnet restore "Lekha.TaskGroup.Api/Lekha.TaskGroup.Api.csproj"
COPY . .
WORKDIR "/src/Lekha.TaskGroup.Api"
RUN dotnet build "Lekha.TaskGroup.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lekha.TaskGroup.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lekha.TaskGroup.Api.dll"]