# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props .
COPY src/Einvoice.Ui/Einvoice.Ui.csproj src/Einvoice.Ui/
RUN dotnet restore src/Einvoice.Ui/Einvoice.Ui.csproj

COPY src/ src/
RUN dotnet publish src/Einvoice.Ui/Einvoice.Ui.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Einvoice.Ui.dll"]
