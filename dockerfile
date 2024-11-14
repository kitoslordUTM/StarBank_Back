# Usa la imagen oficial de .NET SDK para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia el archivo de proyecto (.csproj) y restaura dependencias
COPY WebApplication2/*.csproj ./
RUN dotnet restore

# Copia el resto de los archivos del proyecto y compila la aplicación
COPY WebApplication2/. ./
RUN dotnet publish -c Release -o out

# Usa la imagen oficial de .NET runtime para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expone el puerto en el que la aplicación estará escuchando
EXPOSE 80

# Comando para iniciar la aplicación
ENTRYPOINT ["dotnet", "WebApplication2.dll"]
