FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore ./users-service-login.csproj

RUN dotnet publish ./users-service-login.csproj -c Release -o /app /p:PublishReadyToRun=false

FROM public.ecr.aws/lambda/dotnet:8
WORKDIR /var/task

COPY --from=build /app ./

CMD ["users-service-login::users_service_login.Function::FunctionHandler"]

