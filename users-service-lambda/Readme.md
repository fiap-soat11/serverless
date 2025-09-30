## Brincando direto no Visual Studio:

1.
Para implantar sua função no AWS Lambda, clique com o botão direito no projeto no Solution Explorer e selecione *Publicar no AWS Lambda*.

2.
Para visualizar sua função implantada, abra a janela Function View clicando duas vezes no nome da função mostrado abaixo do nó AWS Lambda na árvore do AWS Explorer.

3.
Para realizar testes em sua função implantada, use a guia Test Invoke na janela Function View aberta.

4.
Para configurar fontes de eventos para sua função implantada, por exemplo, para que sua função seja invocada quando um objeto for criado em um bucket do Amazon S3, use a guia Event Sources na janela Function View aberta.

5.
Para atualizar a configuração de tempo de execução de sua função implantada, use a guia Configuração na janela Function View aberta.

6.
Para visualizar logs de execução de invocações de sua função, use a guia Logs na janela Function View aberta.

## Aqui estão alguns passos para começar a partir da linha de comando:

## Dicas
1. http://jwtbuilder.jamiekurtz.com/ para construir um jwt free.
2. https://www.jwt.io/ Decode jwt.

1. Rodar o comando no powerShell "dotnet publish -c Release -r linux-x64 --self-contained false";
2. "dotnet publish -c Release -r linux-x64 --self-contained false -p:PublishReadyToRun=false -o ./publish";
3. Depois "Compress-Archive -Path .\publish\* -DestinationPath function.zip";
4. Dentro do Visual Studio, abra o Developer PowerShell e rode "aws lambda update-function-code --function-name users_service_lambda --zip-file fileb://function.zip"; 

## Colocando pra rodar tudo.
1. Instale "dotnet add package MySqlConnector";

Instale o Amazon.Lambda.Tools Global Tools se ainda não estiver instalado.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

Se já estiver instalado, verifique se uma nova versão está disponível.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Executar testes unitários
```
    cd "users-service-lambda/test/users-service-lambda.Tests"
    dotnet test
```

Implantar função no AWS Lambda
```
    cd "users-service-lambda/src/users-service-lambda"
    dotnet lambda deploy-function
```

Também mais fácil e direto, clicar no nome da lambda, com o botão direito clicar em "Publish to AWS lambda", mais fácil.

Link pra baixar o toolkit da AWS, pra brincar.
https://marketplace.visualstudio.com/items?itemName=AmazonWebServices.AWSToolkitforVisualStudio2022