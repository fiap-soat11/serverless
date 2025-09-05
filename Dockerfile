FROM mcr.microsoft.com/dotnet/sdk:6.0 AS sdk
WORKDIR /app

COPY ./ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=sdk /app/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "MinimalApi.dll"]