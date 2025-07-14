# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia apenas o .csproj e restaura dependências (cache eficiente)
COPY ["Lanches.csproj", "."]
RUN dotnet restore "Lanches.csproj"

# 2. Copia o resto e publica
COPY . .
RUN dotnet publish "Lanches.csproj" -c Release -o /app --no-restore

# Estágio de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# 3. Instala dependências para PostgreSQL
RUN apt-get update && \
    apt-get install -y libgdiplus && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# 4. Configurações essenciais
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# 5. Copia os arquivos publicados
COPY --from=build /app .

# 6. Entrypoint otimizado
ENTRYPOINT ["dotnet", "Lanches.dll"]
