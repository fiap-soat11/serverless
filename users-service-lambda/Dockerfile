FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore ./users-service-lambda.csproj

RUN dotnet publish ./users-service-lambda.csproj -c Release -o /app /p:PublishReadyToRun=false

FROM public.ecr.aws/lambda/dotnet:8
WORKDIR /var/task

COPY --from=build /app ./

CMD ["users-service-lambda::users_service_lambda.Function::FunctionHandler"]
