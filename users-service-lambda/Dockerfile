FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore ./users-service-lambda.csproj

RUN dotnet publish ./users-service-lambda.csproj -c Release -o /app

FROM public.ecr.aws/lambda/dotnet:8
WORKDIR /var/task

COPY --from=build /app ./

CMD ["AutenticacaoApi::users-service-lambda.Function::FunctionHandler"]
