#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 50090

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Lekha.DataParser/Lekha.DataParser.csproj", "Lekha.DataParser/"]
RUN dotnet restore "Lekha.DataParser/Lekha.DataParser.csproj"
COPY . .
WORKDIR "/src/Lekha.DataParser"
RUN dotnet build "Lekha.DataParser.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Lekha.DataParser.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Lekha.DataParser.dll"]