## Brincando direto no Visual Studio:

1.
Para implantar sua fun��o no AWS Lambda, clique com o bot�o direito no projeto no Solution Explorer e selecione *Publicar no AWS Lambda*.

2.
Para visualizar sua fun��o implantada, abra a janela Function View clicando duas vezes no nome da fun��o mostrado abaixo do n� AWS Lambda na �rvore do AWS Explorer.

3.
Para realizar testes em sua fun��o implantada, use a guia Test Invoke na janela Function View aberta.

4.
Para configurar fontes de eventos para sua fun��o implantada, por exemplo, para que sua fun��o seja invocada quando um objeto for criado em um bucket do Amazon S3, use a guia Event Sources na janela Function View aberta.

5.
Para atualizar a configura��o de tempo de execu��o de sua fun��o implantada, use a guia Configura��o na janela Function View aberta.

6.
Para visualizar logs de execu��o de invoca��es de sua fun��o, use a guia Logs na janela Function View aberta.

## Aqui est�o alguns passos para come�ar a partir da linha de comando:

## Dicas
1. http://jwtbuilder.jamiekurtz.com/ para construir um jwt free.
2. https://www.jwt.io/ Decode jwt.

1. Rodar o comando no powerShell "dotnet publish -c Release -r linux-x64 --self-contained false";
2. "dotnet publish -c Release -r linux-x64 --self-contained false -p:PublishReadyToRun=false -o ./publish";
3. Depois "Compress-Archive -Path .\publish\* -DestinationPath function.zip";
4. Dentro do Visual Studio, abra o Developer PowerShell e rode "aws lambda update-function-code --function-name users_service_lambda --zip-file fileb://function.zip"; 

## Database Configuration

The service supports multiple database types through a repository abstraction pattern. Configure the database type using the `DATABASE_TYPE` environment variable.

### Supported Databases:
- **DynamoDB** (default): Set `DATABASE_TYPE=DYNAMODB`
- **MySQL**: Set `DATABASE_TYPE=MYSQL`

### DynamoDB Configuration

1. Create a DynamoDB table with the following structure:
   - **Table Name**: Configure via `DYNAMODB_TABLE_NAME` environment variable (default: "Users")
   - **Partition Key**: `CPF` (String)
   - **Attributes**: `CPF` (String), `Nome` (String), `Email` (String), `Ativo` (Boolean)

2. Set environment variables in AWS Lambda:
   - `DATABASE_TYPE=DYNAMODB`
   - `DYNAMODB_TABLE_NAME=Users` (optional, defaults to "Users")

3. Ensure the Lambda execution role has DynamoDB permissions:
   ```json
   {
     "Version": "2012-10-17",
     "Statement": [
       {
         "Effect": "Allow",
         "Action": [
           "dynamodb:GetItem",
           "dynamodb:PutItem",
           "dynamodb:UpdateItem",
           "dynamodb:DeleteItem"
         ],
         "Resource": "arn:aws:dynamodb:*:*:table/Users"
       }
     ]
   }
   ```

### MySQL Configuration

1. Set environment variables in AWS Lambda:
   - `DATABASE_TYPE=MYSQL`
   - `RDS_CONNECTION_STRING=<your-connection-string>`

2. The MySQL table structure should be:
   ```sql
   CREATE TABLE Cliente (
       CPF VARCHAR(11) PRIMARY KEY,
       Nome VARCHAR(255) NOT NULL,
       Email VARCHAR(255) NOT NULL,
       Ativo BOOLEAN DEFAULT 1
   );
   ```

## Architecture

The project follows a clean architecture pattern with repository abstraction:

- **Domain/Entities**: Business entities (User)
- **Domain/Interfaces**: Repository interfaces (IUserRepository)
- **Infrastructure/Repositories**: Database implementations (DynamoDbUserRepository, MySqlUserRepository)
- **RepositoryFactory**: Factory pattern to create the appropriate repository based on configuration

To add a new database implementation:
1. Create a new repository class implementing `IUserRepository`
2. Add the database type to the `RepositoryFactory.CreateUserRepository()` method

## Colocando pra rodar tudo.
1. Instale "dotnet add package MySqlConnector" (only needed if using MySQL);

Instale o Amazon.Lambda.Tools Global Tools se ainda n�o estiver instalado.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

Se j� estiver instalado, verifique se uma nova vers�o est� dispon�vel.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Executar testes unit�rios
```
    cd "users-service-lambda/test/users-service-lambda.Tests"
    dotnet test
```

Implantar fun��o no AWS Lambda
```
    cd "users-service-lambda/src/users-service-lambda"
    dotnet lambda deploy-function
```

Tamb�m mais f�cil e direto, clicar no nome da lambda, com o bot�o direito clicar em "Publish to AWS lambda", mais f�cil.

Link pra baixar o toolkit da AWS, pra brincar.
https://marketplace.visualstudio.com/items?itemName=AmazonWebServices.AWSToolkitforVisualStudio2022