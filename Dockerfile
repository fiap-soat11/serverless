FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore ./AutenticacaoApi.csproj

RUN dotnet publish ./AutenticacaoApi.csproj -c Release -o /app

FROM public.ecr.aws/lambda/dotnet:8
WORKDIR /var/task

COPY --from=build /app ./

CMD ["AutenticacaoApi::AutenticacaoApi.Function::FunctionHandler"]
