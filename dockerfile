# Usa la imagen oficial de .NET SDK para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todos los archivos al contenedor
COPY . .

# Cambia al directorio del proyecto para restaurar dependencias
WORKDIR /src/WebApplication2
RUN dotnet restore

# Publica la aplicación en modo Release
RUN dotnet publish -c Release -o /app/publish

# Usa la imagen oficial de .NET runtime para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expone el puerto en el que la aplicación estará escuchando
EXPOSE 80

# Comando para iniciar la aplicación
ENTRYPOINT ["dotnet", "WebApplication2.dll"]
