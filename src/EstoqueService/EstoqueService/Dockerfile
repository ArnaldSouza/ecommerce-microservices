FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EstoqueService/EstoqueService/EstoqueService.csproj", "EstoqueService/"]
RUN dotnet restore "EstoqueService/EstoqueService. csproj"
COPY src/EstoqueService/EstoqueService/ EstoqueService/
WORKDIR "/src/EstoqueService"
RUN dotnet build "EstoqueService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EstoqueService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 

# instalar dotnet-ef tool para migrações
RUN dotnet tool install --global dotnet-ef --version 8.*
ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["dotnet", "EstoqueService.dll"]