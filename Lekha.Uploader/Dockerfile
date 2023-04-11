#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Lekha.Uploader/Lekha.Uploader.csproj", "Lekha.Uploader/"]
COPY ["Lekha.Infrastructure/Lekha.Infrastructure.csproj", "Lekha.Infrastructure/"]
COPY ["Lekha.Models/Lekha.Models.csproj", "Lekha.Models/"]
RUN dotnet restore "Lekha.Uploader/Lekha.Uploader.csproj"
COPY . .
WORKDIR "/src/Lekha.Uploader"
RUN dotnet build "Lekha.Uploader.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lekha.Uploader.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lekha.Uploader.dll"]