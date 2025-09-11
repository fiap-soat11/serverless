# Etapa de build
FROM public.ecr.aws/lambda/dotnet:8 AS build
WORKDIR /src

# Copia csproj e restaura pacotes
COPY AutenticacaoApi.csproj .
RUN dotnet restore ./AutenticacaoApi.csproj

# Copia todo o código e publica
COPY . .
RUN dotnet publish ./AutenticacaoApi.csproj -c Release -o /app

# Etapa final (imagem lambda)
FROM public.ecr.aws/lambda/dotnet:8
WORKDIR /var/task

COPY --from=build /app ./

# Comando de entrada da Lambda (handler namespace::classe::método)
CMD ["AutenticacaoApi::AutenticacaoApi.Function::FunctionHandler"]
