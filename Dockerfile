# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia apenas o .csproj primeiro (para cache eficiente)
COPY ["*.csproj", "."]
RUN dotnet restore "Lanches.csproj"

# 2. Copia todo o resto
COPY . .

# 3. Publica com runtime específico
RUN dotnet publish "Lanches.csproj" -c Release -o /app \
    --runtime linux-x64 \
    --self-contained false \
    --no-restore

# Estágio de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# 4. Instala dependências para PostgreSQL e libgdiplus
RUN apt-get update && \
    apt-get install -y libgdiplus && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# 5. Configurações essenciais
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# 6. Copia os arquivos publicados
COPY --from=build /app .

# 7. Entrypoint otimizado
ENTRYPOINT ["dotnet", "Lanches.dll"]
