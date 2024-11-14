# Usa la imagen oficial de .NET SDK para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia el archivo de proyecto (.csproj) en el directorio de trabajo
COPY WebApplication2/WebApplication2.csproj ./WebApplication2/

# Cambia al directorio del proyecto
WORKDIR /app/WebApplication2

# Restaura las dependencias del proyecto
RUN dotnet restore

# Copia todos los archivos del proyecto
COPY WebApplication2/. ./

# Publica la aplicación en modo Release
RUN dotnet publish -c Release -o /app/out

# Usa la imagen oficial de .NET runtime para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expone el puerto en el que la aplicación estará escuchando
EXPOSE 80

# Comando para iniciar la aplicación
ENTRYPOINT ["dotnet", "WebApplication2.dll"]
