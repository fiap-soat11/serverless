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

## Colocando pra rodar tudo.
1. Instale "dotnet add package MySqlConnector";

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