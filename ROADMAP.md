# DynamicQueryBuilder: diagnóstico e cronograma de trabalho até a 1.0

Documento interno de planejamento, escrito em 24/11/2026.

- **Base da análise:** `develop@b5165e6`, que é o `fe3a9de` analisado mais o bump de versão do PR #7, mesclado em 24/11/2026. Nada foi implementado; este documento só descreve e ordena o trabalho.
- **Como foi feito:** quatro auditorias independentes (código .NET, segurança e licença, testes/CI/docs, mercado), com os achados mais graves conferidos direto nos arquivos. Foram executados build, testes, cobertura e provas de conceito contra a DLL compilada.
- **O que não foi possível:** não havia banco de dados disponível. O SQL de catálogo foi analisado por leitura; onde a conclusão depende do comportamento do servidor, o texto diz "inferido".
- **Publicação:** desde o F0-06, o `docfx.json` publica só `index.md`, `toc.yml` e `docs/**`. Este arquivo fica no repositório e não vai para o GitHub Pages.
- **Progresso:** Fase 0 concluída em 26/11/2026 (PR #8). Decisões D1 a D5 tomadas e registradas em `DECISIONS.md`.

## 1. Resumo

O projeto é um protótipo de dois dias (3 e 4 de março de 2025): cerca de 1.000 linhas em `src/`, 65 testes passando, nunca publicado, sem estrelas, parado há 20 meses. A ideia original (ler o schema do banco e usá-lo para montar queries dinâmicas) é boa e é justamente a parte que o mercado não atende bem. O código atual ainda não entrega essa ideia.

O caminho até o mainstream não é polimento. Quatro coisas impedem qualquer adoção hoje:

1. **O SQL é montado por concatenação.** Valores, colunas, operadores e condições de `JOIN` entram como texto. Para uma biblioteca de queries dinâmicas, isso é injeção de SQL por desenho, e o README anuncia o contrário.
2. **As duas metades não se encaixam.** O leitor de metadados e o builder usam formatos diferentes, e a única validação existente confere a tabela do `FROM`. Colunas e tabelas de `JOIN` nunca são validadas.
3. **O nome do pacote é de outra empresa.** O ID `DynamicQueryBuilder` no NuGet pertence à Oplog. O comando de instalação do README instala a biblioteca deles.
4. **Licença e plataforma.** O arquivo `LICENSE` não é um texto de licença válido, e os dois frameworks-alvo (.NET 8 e 9) saíram de suporte em 10/11/2026.

Como builder fluente genérico, o projeto está atrás do SqlKata (MIT, 29,4 milhões de downloads). O espaço que nenhuma biblioteca estabelecida ocupa é a combinação de schema real, definição de query serializável, validação e SQL parametrizado. É para lá que o cronograma aponta.

**O plano:** 9 fases, 94 itens, cerca de **90 dias de trabalho focado** (faixa realista: 70 a 135). Três marcos:

| Marco               | Fases | Esforço acumulado | O que você tem na mão                                                                           |
| ------------------- | ----- | ----------------- | ----------------------------------------------------------------------------------------------- |
| Seguro para mostrar | 0 a 2 | 35 dias           | `0.1.0-alpha` só do núcleo: parametrizado, dois dialetos, sem drivers; CI e release funcionando |
| Com diferencial     | 3 a 5 | 68 dias           | `0.4.0-beta`: pacotes por banco, validação contra o schema real e queries em JSON               |
| 1.0                 | 6 a 8 | 90 dias           | API congelada, SQL Server, documentação de produto                                              |

Antes da Fase 1 você precisa tomar cinco decisões (seção 4). A Fase 0 não depende de nenhuma e pode começar já. Atualização de 26/11/2026: as cinco foram tomadas (`DECISIONS.md`) e a Fase 0 está concluída.

## 2. Ponto de partida

| Aspecto      | Estado em 24/11/2026                                                                                               |
| ------------ | ------------------------------------------------------------------------------------------------------------------ |
| Histórico    | 9 commits na `develop`, todos de 3 e 4/3/2025; 6 PRs mesclados pelo autor em até 10 minutos, sem revisão           |
| PR #7        | Mesclado em 24/11/2026. Levou só o bump `0.0.0.3 → 0.0.0.6`; a `feature/query-builder` já tinha entrado pelo PR #5 |
| Código       | Cerca de 1.000 linhas em `src/`; builder de `SELECT` e leitor de metadados para PostgreSQL e MySQL                 |
| Build        | Passa com 0 avisos, mas só porque um `<NoWarn>` esconde comentários XML malformados                                |
| Testes       | 65 passando em net8.0 e net9.0; 79,4% das linhas e 64,3% dos ramos; a metade de metadados tem 0%                   |
| Frameworks   | `net8.0` e `net9.0`, ambos fora de suporte desde 10/11/2026                                                        |
| Publicação   | Nenhuma tag, nenhum release, nenhum pacote no NuGet                                                                |
| CI           | Um workflow, que só gera documentação e a publica, inclusive a partir de pull requests                             |
| Site de docs | No ar em `gabriel-sisjr.github.io/DynamicQueryBuilder`, sem link a partir do README                                |
| Comunidade   | 0 estrelas, 0 forks; perfil de comunidade do GitHub em 28%                                                         |
| Branches     | `develop` é a padrão; `main` está parada no primeiro commit                                                        |

## 3. Diagnóstico

Cada achado termina com o item do cronograma que o resolve. Caminhos de `.cs` são relativos a `src/DynamicQueryBuilder/`, salvo indicação.

### 3.1 Segurança

- **Todo valor é concatenado no SQL e não existe saída para parâmetros.** `FilterBy` monta `$"{column} {@operator} {value}"` (`Builders/QueryBuilderFilters.cs:14,31,44-45`) e `GenerateSql` devolve só uma `string` (`Interfaces/Builders/IQueryBuilder.cs:177`). Na prova de conceito, um valor de filtro `'x' OR 1=1 --` comentou o filtro de tenant e o `LIMIT` adicionados depois. Os testes consagram o contrato em que o chamador entrega o literal já entre aspas (`tests/.../QueryBuilderTests.cs:91,146,318`). (F2-03)
- **Identificadores e fragmentos passam direto.** Colunas, tabela e condição de `JOIN`, `GROUP BY`, `ORDER BY` e o operador em texto livre vão para o SQL sem checagem. A lista de colunas entregue ao `Setup` é guardada e nunca lida; a única validação é a tabela do `FROM` (`Builders/QueryBuilder.cs:87`). (F2-06, F2-07, F3-07)
- **`AND` e `OR` sem agrupamento.** Os filtros são emitidos em sequência, sem parênteses (`QueryBuilder.cs:102-105`). O resultado `TenantId = 42 AND Name = 'bob' OR Name = 'alice'` devolve linhas de todos os tenants sem nenhum caractere especial. Parametrizar valores não corrige isso. (F2-05)
- **Builder com estado registrado como `Scoped`** (`DynamicQueryBuilderModule.cs:36`). Dois componentes na mesma requisição compartilham a instância, e o filtro de um vaza para a query do outro. Com 20.000 chamadas paralelas, filtros se perdem e o `GenerateSql` chegou a lançar `NullReferenceException`. (F2-09, F2-15)
- **A senha aparece no `ToString()`.** `DynamicQueryBuilderSettings` é um record posicional (`Models/Helpers/DynamicQueryBuilderSettings.cs:10`), então qualquer log estruturado imprime a string de conexão inteira. (F3-09)
- **Sem limites de recurso.** Não há `LIMIT` padrão nem máximo, limite negativo é aceito e não há teto de joins. (F2-14)
- **Credencial commitada e exemplo sem autenticação.** `examples/ClientWebAPI/ClientWebAPI/appsettings.json:9` traz uma string de conexão com o superusuário `postgres`, um IP de rede interna e o nome de um banco de produto interno. O controller do exemplo expõe o schema inteiro sem autenticação (`Controllers/DynamicQueryBuilderController.cs:7-15`). (F0-04, F3-11)
- **O README promete o que o código não tem.** "Type-safe", "SQL injection protection" e "Built-in validation mechanisms" (`README.md:13,40,41`; `index.md:19,50,51`) convivem com um roadmap, no mesmo arquivo, que lista esses pontos como pendentes (`README.md:127-131`). (F0-02)

### 3.2 Corretude do SQL gerado

- **Nome de tabela reescrito.** `FromTable` aplica `ToUpper()` sensível à cultura e emite sem aspas (`QueryBuilder.cs:24`). Em cultura turca, `items` não casa com `ITEMS`. No PostgreSQL, uma tabela criada com caixa mista fica inalcançável (inferido). Não existe `schema.tabela` nem alias no `FROM`. (F2-06)
- **Joins inválidos.** `SELF JOIN` não é SQL (`QueryBuilder.cs:45`), `CROSS JOIN` sempre sai com `ON` (`:49`) e `FULL JOIN` é emitido para MySQL, que não o tem. Os testes travam os dois primeiros (`QueryBuilderTests.cs:384,460`). (F2-07)
- **Entradas vazias ou nulas.** Saem `WHERE AGE = ` com o lado direito vazio, `IN ()`, `LIMIT -5` e `ORDER BY ID 7`; `FromTable(null)` lança `NullReferenceException`. (F2-11)
- **Dois geradores que já divergem.** `GenerateSql` e `GenerateFormattedSql` duplicam todas as cláusulas. `SetLimit(0)` é omitido em um (`QueryBuilder.cs:114`) e emitido no outro (`:164`). O formatado usa `Environment.NewLine`, então a saída muda entre Windows e Linux. (F2-08)
- **O driver não influencia o SQL.** `DatabaseDriver` só é lido pelo leitor de metadados. `ILIKE` sai para MySQL e a paginação é `LIMIT` fixo. (F2-04)
- **API morta ou armadilha.** `ArithmeticOperators`, `SetOperators` e `LogicalOperators.NOT` não têm uso; `StringOperators.CONCAT` é aceito e sempre lança exceção; `FilterClause` é público sem que nenhum método o use. (F2-12)

### 3.3 Desenho da API

- **Três formas de construir o builder** (`new QueryBuilder()`, `QueryBuilder.Create()` e injeção), e a etapa obrigatória de `Setup` pode ser contornada. `IQueryBuilder` não é resolvível pelo contêiner. (F2-09)
- **Nomes fora da convenção .NET.** Enums em `SCREAMING_CASE` e no plural. `LogicalOperators` e `OrderDirection` chegam ao SQL por `ToString()` (`QueryBuilder.cs:70,105`), então renomear um membro muda o SQL emitido. Os métodos (`Columns`, `FromTable`, `FilterBy`, `SetLimit`, `GenerateSql`) fogem do padrão do ecossistema. (F2-10)
- **Tudo é `string`.** Tabela, coluna, operador e valor. Existem 12 sobrecargas quase idênticas de `FilterBy` e `OrFilterBy`. (F2-10)
- **Visibilidade trocada.** `DriversFactory` e o SQL de catálogo são públicos; o leitor `GetDatabaseMetaData` é interno e sem fábrica, portanto só funciona via injeção. Namespaces como `Models.Enums.Helpers.Databases`. (F2-09)
- **Registro em DI frágil.** As configurações são capturadas em closure, sem padrão Options, sem validação e sem `TryAdd`; só um banco por aplicação. A dependência das abstrações de DI chega por transitividade do Npgsql. (F2-15)

### 3.4 Metadados

- **Formatos incompatíveis.** O leitor devolve schema → tabela → colunas (`Interfaces/Helpers/IGetDatabaseMetaData.cs:15`); o `Setup` espera tabela → nomes (`Interfaces/Builders/IQueryBuilderSetup.cs:21`). Com as chaves em minúsculas que o PostgreSQL devolve, `GenerateSql` lança "Undefined or Invalid table". (F3-01, F3-07)
- **A consulta de catálogo do MySQL deve falhar.** Ela seleciona `TABLE_SCHEMA`, `TABLE_NAME` e `COLUMN_NAME` sem qualificar, num join com `ON` entre duas views que têm essas colunas (`Drivers/DriversFactory.cs:66-77`). Inferido: não foi executada em MySQL; a mesma consulta falha por ambiguidade em tabelas de mesmo formato no SQLite. (F3-05)
- **Chave primária errada no PostgreSQL.** O campo recebe `column_default`, que é a expressão padrão da coluna (`DriversFactory.cs:46`). Chaves estrangeiras compostas geram produto cartesiano, enums e arrays saem como `USER-DEFINED` e `ARRAY`, e não há ordenação (inferido pela leitura). (F3-04)
- **MySQL devolve dados enganosos.** `COLUMN_KEY` traz `PRI`, `UNI` ou `MUL`, nunca nulo; o join traz chaves primárias e únicas junto com as estrangeiras; não há filtro pelo banco da conexão, então saem todos os bancos visíveis ao login. (F3-05)
- **Lacunas de execução.** Sem `CancellationToken`, descarte síncrono da conexão, sem filtro de schema ou tabela, sem cache (o catálogo inteiro é lido a cada requisição) e sem exceção própria. (F3-06)
- **Modelo inadequado.** Dicionário mutável aninhado; `DatabaseTablesMetaData` descreve uma coluna; `PrimaryKey` é `string`; faltam nulabilidade e ordem; chave estrangeira composta não é representável. (F3-01)

### 3.5 Dependências e empacotamento

- **ID do pacote ocupado.** Sem `<PackageId>`, o projeto empacota como `DynamicQueryBuilder`, que no nuget.org é da Oplog (91.583 downloads, última versão 1.1.0 em 10/2/2025). (F1-02)
- **Frameworks fora de suporte.** .NET 8 e .NET 9 encerraram em 10/11/2026. Em suporte hoje: .NET 10 (LTS) e .NET 11, ambos até 14/11/2028. O Npgsql 11 só tem alvo `net10.0`. (F1-05)
- **Drivers no núcleo.** O pacote depende de `Npgsql` e `MySql.Data`, o que traz cerca de 30 pacotes transitivos e obriga quem só usa PostgreSQL a baixar a pilha do MySQL. O `MySql.Data` impede declarar compatibilidade com AOT e trimming. (F2-02, F3-02)
- **`MySql.Data` é GPL.** A licença é `GPL-2.0-only` com exceção para software livre. A exceção não cobre um cliente de código fechado, então uma licença comercial sua não alcançaria o código da Oracle. `MySqlConnector` é MIT e tem cerca de três vezes os downloads anuais. Não é parecer jurídico. (F3-03)
- **`.csproj` com defeitos.** Versão de quatro partes fixada à mão (a mesma `0.0.0.6` já identifica dois builds diferentes); o segundo `<NoWarn>` substitui o primeiro e o que sobra esconde os comentários XML malformados de `ComparisonOperators.cs`; `DocumentationFile` fixo em `bin\Debug`, de modo que o build Release escreve na pasta Debug; nenhum metadado de pacote (a descrição gerada é "Package Description"); sem pacote de símbolos. (F0-10, F1-04, F1-07, F1-08)
- **Sem portões de qualidade.** Com análise completa ligada aparecem 52 avisos; a maior parte é de nomenclatura e some com o item F2-10. (F1-06)

### 3.6 Testes

- **A suíte consagra defeitos.** Há testes esperando `WHERE AGE  30` (com o comentário "which is odd, but covers the path", `QueryBuilderTests.cs:575-585`), `OR  = NULL` (`:587-598`), `SELF JOIN`, `CROSS JOIN ... ON` e os dois comportamentos de `LIMIT 0`. Corrigir o builder quebra esses testes.
- **Nenhum teste executa SQL.** A metade de metadados e o registro em DI têm 0% de cobertura (Apêndice C).
- **`DriversFactoryTests` tem 100% de cobertura e pouco sinal.** Seis testes verificam que um literal contém pedaços de si mesmo. Em `ShouldThrow<T>("texto")`, o texto é a mensagem de falha do teste, não a mensagem esperada da exceção.
- **Tipos de teste ausentes:** SQL esperado por dialeto, payloads de injeção, integração com banco real, concorrência, superfície pública, compilação das amostras da documentação e benchmarks.
- **Higiene:** `Moq` é referenciado e nunca usado; convivem dois estilos de asserção.

Resolvido em F2-16 e F3-10.

### 3.7 CI, versão e release

- **O único workflow não compila nem testa.** Ele instala o DocFX e gera o site. O check verde de um PR não diz nada sobre o código. (F0-07)
- **Pull requests publicam no site de produção.** O job `deploy` não tem condição de evento. Aconteceu com o PR #7: o site foi regenerado a partir de `refs/pull/7/merge`, e o link "Edit this page" ficou apontando para `blob/7/merge/...` (404) até o merge disparar um novo deploy a partir da `develop`. Sete dos 15 deploys feitos até aquele momento vieram de commits que não estão em branch nenhuma. (F0-05)
- **Build de docs sem controle.** DocFX sem versão fixa (2.81.0 no CI, 2.78.3 local); `"pdf": true` baixa 114 MB de Chrome a cada execução; 13 avisos ignorados; o glob `**/*.{md,yml}` publica `README.html` e `COMMERCIAL_LICENSE.html`. (F0-05, F0-06)
- **Três fontes de versão, nenhuma ligada.** `<Version>` manual; `.gitversion.yml`, que é a configuração padrão do GitVersion e não é chamado por nada; `standard-version`, sem script e descontinuado; mais uma pasta `changelog/` vazia. Zero tags e zero releases. (F1-08)
- **Convenção de commit não chega à branch padrão.** Os commits locais passam no commitlint, mas 7 dos 9 commits da `develop` falham, porque o squash usa o título do PR. (F1-10)
- **`main` abandonada.** Está no primeiro commit, com `Class1.cs`; o workflow tem `TODO: Change to main in the future`. (F1-01)
- **Sem proteção de branch.** O único ruleset está desativado e, como está escrito, bloquearia todo merge (exige aprovação de outra pessoa, CodeQL e assinatura). (F0-08)
- **Sem automação de segurança.** Não há Dependabot, CodeQL, secret scanning, relato privado de vulnerabilidade nem `SECURITY.md`. (F0-09)
- **Ferramental Node pesado.** 409 pacotes npm para validar mensagem de commit, com 12 vulnerabilidades conhecidas (não distribuídas com a biblioteca); o pre-commit roda `dotnet test` a cada commit, inclusive de documentação. (F0-11, F1-10)

### 3.8 Documentação e exemplos

- **Onze pontos em que a documentação não bate com o código** (Apêndice B). No README, duas amostras não compilam, as de query usam um `queryBuilder` nunca declarado e a etapa obrigatória de `Setup` não aparece em lugar nenhum. (F0-02, F7-01)
- **Três cópias mantidas à mão.** `README.md`, `index.md` e `docs/getting-started.md` repetem o mesmo conteúdo; o PR #6 atualizou uma e deixou as outras. (F7-04)
- **Site no ar e invisível.** Nada no repositório aponta para ele; a página de introdução é só um título; a página inicial tem cinco links mortos. (F7-06)
- **Exemplos que não demonstram nada.** O `ConsoleClient` é o template "Hello, World!". O `ClientWebAPI` não compila a partir de um clone limpo: referencia o pacote `0.0.0.6`, que só existe numa pasta local ignorada, e o NuGet resolve para o pacote da Oplog. (F0-12, F3-11)
- **Scripts de build com efeito colateral.** `build/*.sh` e `build/*.ps1` registram uma fonte no NuGet global da máquina, não chegam ao fim e imprimem sucesso mesmo após falha. (F0-12)

### 3.9 Licença

O modelo pretendido é AGPL-3.0 mais licença comercial, mas os arquivos não o expressam:

- `LICENSE` é um aviso de 9 linhas, não o texto da AGPL. Por isso o GitHub classifica a licença como "Other".
- Uma frase acrescentada diz que uso comercial obriga a abrir o código. A AGPL não diz isso, e restrições adicionais podem ser removidas por quem recebe o software.
- `COMMERCIAL_LICENSE.md` não tem termos, só um convite para contato.
- O README mostra um selo MIT ao lado de um selo AGPL.
- O pacote não declara licença e não há CLA para contribuições externas.

Não é parecer jurídico. A decisão é a D3; a correção é o item F1-03.

### 3.10 Higiene do repositório

- Sem `Directory.Build.props`, gerenciamento central de pacotes, `global.json` ou manifesto de ferramentas. (F1-04)
- As regras de nomenclatura do `.editorconfig` citam estilos que não existem e não têm efeito; três arquivos estão em CRLF apesar da regra de LF; o `.gitignore` é um modelo antigo que ignoraria uma futura pasta `sql/`. (F1-12)
- Sem `CONTRIBUTING`, código de conduta, templates, descrição ou tópicos no repositório. (F1-11)
- `feature/update-docs` é idêntica à `develop`. Há dois stashes: um guarda 8 linhas de documentação de uma sobrecarga `AddDynamicQueryBuilder(settings => { ... })` que não existe no código; o outro guarda 7.896 arquivos de `node_modules`. (F0-01, F0-13)

### 3.11 Mercado

- **Montar SQL fluente, parametrizado e multi-dialeto é commodity.** SqlKata, linq2db, RepoDb e outros já fazem (Apêndice D).
- **Validação em tempo de execução contra o schema real:** nenhuma biblioteca estabelecida encontrada.
- **JSON para SQL seguro, sem classes de entidade:** mal atendido. O que existe é novo e pequeno (`SqlKata.JsonQuery`, `Querio`), ambos lançados entre julho e agosto de 2026.
- **Chamadas de ferramenta por agentes de IA:** a Microsoft e a OWASP recomendam exatamente o padrão em que o modelo fornece a intenção e um motor monta a query de forma determinística. A Microsoft entrega isso como servidor, não como biblioteca.
- **Licença:** não foi encontrada nenhuma biblioteca .NET que tenha chegado ao mainstream lançando já sob AGPL mais comercial. Todos os casos restritivos conhecidos mudaram de licença depois de anos de adoção permissiva.

## 4. Decisões que são suas

O cronograma da seção 5 assume a opção recomendada em cada uma. Onde outra escolha muda o plano, está indicado.

**Decididas em 26/11/2026** (detalhes em `DECISIONS.md`): D1 = B, D2 = A (o nome é escolhido no F1-02), D3 = A com MIT, D4 = A e D5 = C. A D5 difere da recomendação: o `netstandard2.0` no núcleo sai de "Depois da 1.0" (seção 5.3) e entra depois do F2-02, antes do `0.9.0-rc`, sem estimativa no cronograma.

### D1. O que a biblioteca é

| Opção                                                                 | Consequência                                                                                                                                                                                                                                                           |
| --------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A. Builder fluente genérico                                           | Concorrência direta com o SqlKata, partindo de trás. Sai a Fase 5; crescem as Fases 4 e 6 (mais dialetos, `INSERT`/`UPDATE`/`DELETE`)                                                                                                                                  |
| **B. Queries dinâmicas validadas contra o schema real (recomendada)** | Introspecção, definição serializável, validação e SQL parametrizado. O builder fluente vira a interface programática do mesmo modelo                                                                                                                                   |
| C. A opção B construída sobre os compiladores do SqlKata              | Corta F2-02 a F2-04, boa parte da Fase 4 e o F6-01, e acrescenta uma camada de adaptação: da ordem de 15 a 20 dias a menos. Em troca, depende de uma biblioteca com uma única versão estável desde 2022, alvo só `net8.0` e que declara não aceitar novos compiladores |

A opção B é a que aproveita o que o projeto já tentou fazer e a única com espaço livre. O risco é a janela: três projetos se moveram para perto desse espaço nos últimos cinco meses.

### D2. Nome e ID do pacote

O ID atual não pode ser publicado. Além disso, o assembly se chamaria `DynamicQueryBuilder.dll`, igual ao da Oplog.

| Opção                                         | Consequência                                                                                                                                                         |
| --------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **A. Nome novo, alinhado à D1 (recomendada)** | Candidatos livres hoje no nuget.org: `SchemaSql`, `KnownSql`, `StrictSql`, `GuardedSql`, `CatalogSql` (Apêndice E). Renomeia pacote, assembly e namespace de uma vez |
| B. Manter o nome com um prefixo seu reservado | Menos trabalho de marca; o nome continua confundível com o da Oplog e com meia dúzia de pacotes "DynamicQuery"                                                       |

Disponibilidade conferida por consulta ao índice do NuGet; não é garantia de aceite e não houve checagem de marca registrada. Como nada foi publicado ainda, renomear na Fase 1 (F1-02) não quebra ninguém.

### D3. Licença

| Opção                                                               | Consequência                                                                                                                                                                                                                                   |
| ------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **A. MIT ou Apache-2.0 (recomendada para o objetivo "mainstream")** | Remove a maior barreira de adoção. Num levantamento feito para esta análise sobre os 1.000 pacotes mais usados do nuget.org, fora os da Microsoft, 45% são MIT e 29% Apache-2.0. Monetização fica para depois, por patrocínio ou módulos pagos |
| B. AGPL-3.0 mais comercial, feita direito                           | Texto integral da AGPL, termos comerciais redigidos por advogado e CLA. Mantém a venda de licenças; AGPL é proibida por política em muitas empresas e fecha a porta da .NET Foundation                                                         |
| C. Núcleo permissivo e complementos pagos                           | Combina adoção com receita, mas só faz sentido quando houver complementos                                                                                                                                                                      |

Em qualquer opção: sai o selo contraditório, entra o texto integral e a licença é declarada no pacote.

### D4. Gerar SQL ou também executar

| Opção                                                       | Consequência                                                                                                                                       |
| ----------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| **A. O núcleo só gera `{ Sql, Parâmetros }` (recomendada)** | Núcleo sem dependência de driver. A introspecção fica em pacotes por banco e recebe a conexão de quem chama. Execução vira pacote opcional (F6-06) |
| B. A biblioteca abre conexões e executa                     | Mais simples de demonstrar; mantém drivers no caminho principal e compete com Dapper e EF Core                                                     |

### D5. Bancos e frameworks

| Opção                                                                                             | Consequência                                                                                                                                                                                                |
| ------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **A. PostgreSQL e MySQL até o beta; SQL Server antes da 1.0; `net10.0` e `net8.0` (recomendada)** | SQL Server é o banco mais comum no ecossistema .NET e força a camada de dialeto a ser real (paginação e aspas diferentes). `net8.0` está fora de suporte, mas ainda é o alvo de muita aplicação em produção |
| B. Só PostgreSQL e MySQL na 1.0                                                                   | Seis dias a menos (sai o F6-01); a camada de dialeto é validada só com dois bancos parecidos                                                                                                                |
| C. Acrescentar `netstandard2.0` ao núcleo                                                         | Alcance máximo, ao custo de recursos de linguagem e polyfills. Só é viável depois do F2-02                                                                                                                  |

## 5. Cronograma

### 5.1 Visão geral

| Fase | Objetivo                                  | Depende de     | Esforço     | Entrega                                  |
| ---- | ----------------------------------------- | -------------- | ----------- | ---------------------------------------- |
| 0    | Estancar e proteger (✅ concluída)         | nada           | 6 dias      | Repositório honesto e com CI de verdade  |
| 1    | Fundação do repositório e do pacote       | D2, D3, D5     | 8 dias      | Pipeline de release ensaiado             |
| 2    | Núcleo novo: modelo, parâmetros, dialetos | D1, D4, Fase 1 | 21 dias     | `0.1.0-alpha` (só o núcleo)              |
| 3    | Schema: introspecção e validação          | Fase 2         | 14 dias     | `0.2.0-alpha` (com os pacotes por banco) |
| 4    | Cobertura de SQL                          | Fase 2         | 9 dias      | `0.3.0-beta`                             |
| 5    | Definição de query serializável           | Fases 3 e 4    | 10 dias     | `0.4.0-beta`                             |
| 6    | Maturidade de biblioteca                  | Fase 5         | 11 dias     | `0.9.0-rc`                               |
| 7    | Documentação de produto                   | Fase 6         | 8 dias      | Site completo                            |
| 8    | 1.0 e adoção                              | Fase 7         | 3 dias      | `1.0.0`                                  |
|      | **Total**                                 |                | **90 dias** |                                          |

As Fases 3 e 4 podem andar em paralelo. A documentação de cada mudança entra na mesma fase em que a mudança é feita; a Fase 7 é a reescrita do material de produto, não a primeira vez que a documentação é tocada.

### 5.2 Como ler as tabelas

- **Tipo** segue a convenção de commits do projeto (`feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `perf`), com o escopo entre parênteses.
- **Esforço:** P é até 2 horas; M é cerca de 1 dia; G é cerca de 3 dias; GG é 1 semana ou mais.
- Os totais somam P = 0,25, M = 1, G = 3 e GG = 6 dias, arredondados por fase. São ordens de grandeza para quem conhece o código, não compromissos.
- Cada linha foi escrita para virar uma issue.

### Fase 0: estancar e proteger

> **Status: ✅ concluída em 26/11/2026.** As mudanças de arquivo estão no PR #8. As de configuração foram aplicadas direto no GitHub. Os detalhes de cada item estão em "Status da Fase 0", logo após a tabela.

Nenhum item depende de decisão. Tudo aqui reduz risco imediato ou cria a rede de segurança do resto.

| ID    | Tipo            | Item                                                                                                                                                                                                   | Esf. | Pronto quando                                           |
| ----- | --------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---- | ------------------------------------------------------- |
| F0-01 | chore(repo)     | Apagar `feature/update-docs` no remoto e as branches locais já mescladas (`feature/query-builder`, `feature/update-docs`); ligar a exclusão automática de branch                                       | P    | Só `develop` e `main` no remoto                         |
| F0-02 | fix(docs)       | README e `index.md` honestos: tirar as alegações de proteção contra injeção, validação, type-safety e desempenho; aviso de status pré-alpha; corrigir ou remover as amostras que não compilam          | P    | Nenhuma página afirma o que o código não faz            |
| F0-03 | fix(docs)       | Tirar `dotnet add package DynamicQueryBuilder` e `Install-Package` do README e do getting-started                                                                                                      | P    | Ninguém é mandado para o pacote da Oplog                |
| F0-04 | fix(examples)   | Tirar a string de conexão do `appsettings.json` (user-secrets ou variável de ambiente, com placeholder); trocar a senha se já foi real                                                                 | P    | Nenhum segredo versionado                               |
| F0-05 | fix(ci)         | `docfx.yml`: `deploy` só em `push` na branch de publicação; `contents: read` global e `write` só no job de deploy; grupo de concorrência; versão do DocFX fixada em manifesto; Actions fixadas por SHA | P    | Um PR não altera o site                                 |
| F0-06 | fix(docs)       | `docfx.json`: lista explícita de arquivos no lugar de `**/*.{md,yml}`; desligar o PDF; título do site                                                                                                  | P    | Só `index.md` e `docs/**` são publicados                |
| F0-07 | chore(ci)       | Workflow de CI: restore, build e testes em todo PR e push, incluindo os exemplos                                                                                                                       | M    | Um PR que quebra o build fica vermelho                  |
| F0-08 | chore(repo)     | Ruleset novo na branch padrão: PR obrigatório, check de CI obrigatório, squash, sem force-push                                                                                                         | P    | Push direto na branch padrão é recusado                 |
| F0-09 | chore(security) | Secret scanning com push protection, relato privado de vulnerabilidade, Dependabot (NuGet, npm, Actions), CodeQL e `SECURITY.md`                                                                       | M    | Aba Security sem itens desativados                      |
| F0-10 | fix(build)      | `.csproj`: remover os dois `<NoWarn>`, corrigir os comentários XML de `ComparisonOperators.cs`, um único `GenerateDocumentationFile` sem caminho fixo                                                  | P    | Build limpo sem supressões                              |
| F0-11 | chore(deps)     | `npm audit fix`; atualizar os pacotes de teste                                                                                                                                                         | P    | `npm audit` e `dotnet list package --vulnerable` limpos |
| F0-12 | fix(examples)   | Exemplos por `ProjectReference`, dentro da solução; apagar `build/*.sh` e `build/*.ps1`                                                                                                                | M    | Os exemplos compilam a partir de um clone limpo         |
| F0-13 | chore(repo)     | Descartar os dois stashes locais, depois de confirmar que as 8 linhas de docs do primeiro não interessam                                                                                               | P    | `git stash list` vazio                                  |
| F0-14 | docs(adr)       | Registrar as decisões D1 a D5 em um arquivo curto de decisões                                                                                                                                          | P    | As cinco decisões estão escritas                        |

Total: 6 dias.

#### Status da Fase 0

Todos os 14 itens estão feitos. Alguns critérios só ficam observáveis depois do merge do PR #8 na `develop`; isso está indicado no item.

| ID    | Status | Observação                                                                                                                                                                                                                |
| ----- | ------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| F0-01 | ✅     | `feature/update-docs` apagada no remoto (`ad35dba`); `feature/query-builder` e `feature/update-docs` apagadas localmente; exclusão automática ligada. `gh-pages` continua no remoto porque é a fonte do GitHub Pages |
| F0-02 | ✅     | Inclui `docs/getting-started.md`, `docs/introduction.md` e os comentários XML. O selo MIT continua até o F1-03                                                                                                         |
| F0-03 | ✅     | Nenhuma página manda instalar o pacote da Oplog                                                                                                                                                                         |
| F0-04 | ✅     | Placeholder vazio, com user-secrets ou variável de ambiente e falha na subida. Segundo o mantenedor, a senha era fictícia, então não houve rotação. O valor antigo continua no histórico do git            |
| F0-05 | ✅     | Comprovado no PR #8: o job `deploy` foi pulado e `gh-pages` continuou em `9ae3147`. O DocFX está fixado em 2.81.0. A correção só vale depois do merge: até lá, a `docfx.yml` antiga da `develop` continua publicando a partir de PRs. Em 26/11, os PRs #9 a #15 do Dependabot publicaram no site; eles mexem só no `package-lock.json`, então o texto é o da `develop`, mas os links "Edit this page" apontam para `blob/15/merge` (404) até o primeiro deploy depois do merge |
| F0-06 | ✅     | O site passa a ter só `index`, `toc`, `docs/**` e a API a partir do primeiro deploy depois do merge                                                                                                                   |
| F0-07 | ✅     | Workflow `CI`, job `Build and test`: verde no PR #8, com 0 avisos de build                                                                                                                                                 |
| F0-08 | ✅     | Ruleset "Protect default branch" (`28177642`) ativo em `~DEFAULT_BRANCH`: PR obrigatório com 0 aprovações, só squash, check `Build and test` do GitHub Actions, sem force-push nem exclusão e sem bypass. O ruleset antigo "Deploy to Develop" continua desativado e não se aplica a nenhuma branch |
| F0-09 | ✅     | Ligados: secret scanning com push protection, relato privado de vulnerabilidade, alertas e atualizações de segurança do Dependabot. `dependabot.yml`, CodeQL e `SECURITY.md` valem a partir do merge. Padrões genéricos e checagem de validade do secret scanning não estão disponíveis neste plano |
| F0-10 | ✅     | Build Debug e Release com 0 avisos, sem supressões                                                                                                                                                                     |
| F0-11 | ✅     | `npm audit`: de 12 vulnerabilidades para 0. `dotnet list package --vulnerable` limpo, inclusive transitivos                                                                                                                         |
| F0-12 | ✅     | Os exemplos compilam a partir de um clone limpo, com só nuget.org como fonte                                                                                                                                                     |
| F0-13 | ✅     | `git stash list` vazio. Stashes descartados: `0afad23` (docs da sobrecarga inexistente) e `d052f1f` (`node_modules`). Dá para recuperar com `git stash apply <sha>` até o próximo `git gc`                                    |
| F0-14 | ✅     | `DECISIONS.md`                                                                                                                                                                                                            |

Apareceram durante a fase e ficam para fases seguintes:

- Sem `global.json` (F1-04), o CI usa o SDK mais novo do runner.
- O compartilhamento SMB local derruba builds paralelos com travas de arquivo. A contorno é `DOTNET_SYSTEM_IO_DISABLEFILELOCKING=1` ou `-m:1`.
- O NuGet do Dependabot gera títulos "Bump" com maiúscula, que reprovam no commitlint (F1-10).

### Fase 1: fundação do repositório e do pacote

Depende de D2, D3 e D5. Ao final, publicar é apertar um botão, mas nada é publicado ainda: o núcleo atual não deve ir para o NuGet.

| ID    | Tipo           | Item                                                                                                                                                                           | Esf. | Depende      | Pronto quando                                                |
| ----- | -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---- | ------------ | ------------------------------------------------------------ |
| F1-01 | chore(repo)    | Definir o modelo de branches e alinhar branch padrão, workflow e versionamento a ele                                                                                           | P    |              | `main` deixa de estar no primeiro commit ou deixa de existir |
| F1-02 | chore(pack)    | Renomear pacote, assembly e namespace raiz; `<PackageId>` explícito; pedir reserva de prefixo; 2FA na conta do nuget.org                                                       | M    | D2           | `dotnet pack` gera o ID novo                                 |
| F1-03 | chore(license) | Aplicar a D3: texto integral em `LICENSE`, selos coerentes, licença declarada no pacote; CLA se a escolha for dual                                                             | P    | D3           | O GitHub reconhece a licença                                 |
| F1-04 | chore(build)   | `Directory.Build.props`, gerenciamento central de pacotes, `global.json`, `LangVersion` fixo, manifesto de ferramentas                                                         | M    |              | Nenhuma propriedade repetida entre projetos                  |
| F1-05 | chore(build)   | Frameworks em suporte e drivers atualizados                                                                                                                                    | P    | D5           | Testes rodam em runtime suportado                            |
| F1-06 | chore(build)   | Avisos como erro e análise recomendada; resolver os avisos que não são de nomenclatura                                                                                         | M    | F1-04        | Build falha com aviso novo                                   |
| F1-07 | chore(pack)    | Metadados do pacote, README e ícone embutidos, pacote de símbolos, build determinístico no CI                                                                                  | P    | F1-02        | O pacote gerado passa na validação do NuGet sem avisos       |
| F1-08 | chore(release) | Uma única fonte de versão, derivada de tag; SemVer a partir de `0.1.0-alpha`; remover `<Version>` fixo, `.gitversion.yml` sem uso e `standard-version`; `CHANGELOG.md` na raiz | M    | F1-01        | A versão do pacote vem da tag                                |
| F1-09 | chore(ci)      | Workflow de release por tag: pack, validação, Trusted Publishing por OIDC, GitHub Release; rodar um ensaio sem publicar                                                        | M    | F1-07, F1-08 | O ensaio gera o pacote e as notas                            |
| F1-10 | chore(repo)    | Enxugar o ferramental Node: validar o título do PR no CI e fazer squash com ele; tirar `dotnet test` do pre-commit; remover `commitizen` e `standard-version`                  | P    | F0-07        | Os commits da branch padrão passam na convenção              |
| F1-11 | docs(repo)     | `CONTRIBUTING`, código de conduta, templates de issue e PR, `CODEOWNERS`; descrição, tópicos e homepage do repositório                                                         | M    |              | Perfil de comunidade do GitHub completo                      |
| F1-12 | chore(style)   | Corrigir as regras do `.editorconfig`, criar `.gitattributes`, regenerar o `.gitignore`, checar formatação no CI                                                               | P    | F0-07        | `dotnet format --verify-no-changes` no CI                    |
| F1-13 | chore(ci)      | Cobertura no CI com limiar mínimo e relatório no PR                                                                                                                            | P    | F0-07        | Queda de cobertura reprova o PR                              |

Total: 8 dias.

### Fase 2: núcleo novo

Depende de D1 e D4. É a fase que troca o protótipo por uma biblioteca: quase todo o código e todos os testes atuais são substituídos. Trate como implementação nova, não como refatoração.

O `0.1.0-alpha` publica só o núcleo. O leitor de metadados, os drivers e a classe de configuração que guarda a string de conexão ficam fora do pacote até a Fase 3, porque ainda carregam os defeitos da seção 3.4 e a dependência GPL.

| ID    | Tipo           | Item                                                                                                                                                                                                                           | Esf. | Depende       | Pronto quando                                                                                          |
| ----- | -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---- | ------------- | ------------------------------------------------------------------------------------------------------ |
| F2-01 | docs(adr)      | Três decisões técnicas registradas: representação do SQL (árvore compilada por dialeto), estado do builder (imutável), fronteira entre núcleo e provedores                                                                     | M    | D1, D4        | As três estão escritas com as alternativas descartadas                                                 |
| F2-02 | feat(core)     | Modelo de query em árvore (seleção, origem, joins, predicados, agrupamento, ordenação, paginação), em um projeto de núcleo novo e sem referência a drivers; o leitor de metadados atual fica em projeto à parte, não publicado | G    | F2-01         | O modelo representa tudo que o builder atual emite e o núcleo não referencia `Npgsql` nem `MySql.Data` |
| F2-03 | feat(core)     | Compilação para `{ Sql, Parâmetros }`: todo valor vira parâmetro; valores tipados; fim do literal pré-citado                                                                                                                   | G    | F2-02         | Nenhum valor aparece no texto do SQL                                                                   |
| F2-04 | feat(dialect)  | Camada de dialeto (aspas de identificador, marcadores de parâmetro, paginação, `LIKE`/`ILIKE`, joins suportados) com PostgreSQL e MySQL                                                                                        | G    | F2-02         | A mesma query gera SQL correto nos dois bancos                                                         |
| F2-05 | feat(core)     | Predicados agrupados (`AND`, `OR`, `NOT` com parênteses) e predicado obrigatório que filtros do usuário não alargam                                                                                                            | M    | F2-02         | `(a OR b) AND c` é expressável; o caso do tenant tem teste                                             |
| F2-06 | fix(core)      | Identificadores nunca reescritos e sempre entre aspas; `schema.tabela`; alias de tabela e de coluna                                                                                                                            | M    | F2-04         | Tabela de caixa mista funciona no PostgreSQL                                                           |
| F2-07 | fix(core)      | Joins: remover `SELF_JOIN`, `CROSS JOIN` sem condição, `FULL JOIN` recusado no MySQL, condição estruturada (coluna, operador, coluna)                                                                                          | M    | F2-04         | Nenhum join aceita texto livre                                                                         |
| F2-08 | refactor(core) | Um único gerador com opção de formatação e quebra de linha fixa                                                                                                                                                                | P    | F2-03         | Saída idêntica em Windows e Linux                                                                      |
| F2-09 | refactor(api)  | Builder imutável com ponto de entrada único; implementação interna e selada; namespaces achatados                                                                                                                              | M    | F2-02         | Reusar um builder não altera queries anteriores                                                        |
| F2-10 | refactor(api)  | Enums em PascalCase singular com mapeamento explícito para palavra-chave; métodos alinhados ao ecossistema; um `Where(coluna, operador, valor)` no lugar das 12 sobrecargas                                                    | M    | F2-09         | Análise de nomenclatura sem avisos                                                                     |
| F2-11 | fix(core)      | Validação de argumentos em todo método público                                                                                                                                                                                 | P    | F2-09         | Entrada inválida falha na chamada, não no banco                                                        |
| F2-12 | chore(api)     | Remover a API morta                                                                                                                                                                                                            | P    | F2-10         | Todo tipo público tem uso                                                                              |
| F2-13 | feat(core)     | Saída de emergência para SQL cru, com nome que deixa o risco claro e ainda com parâmetros                                                                                                                                      | P    | F2-03         | Não existe outro caminho para texto livre                                                              |
| F2-14 | feat(core)     | Limites: `LIMIT` padrão e máximo, teto de joins, de filtros e de tamanho de `IN`                                                                                                                                               | M    | F2-03         | Query sem limite recebe o padrão                                                                       |
| F2-15 | refactor(di)   | Registro em DI: fábrica singleton sem estado, Options com validação na inicialização, `TryAdd`, serviços com chave para vários bancos, referência explícita às abstrações de DI                                                | M    | F2-09         | Configuração inválida falha na subida da aplicação                                                     |
| F2-16 | test(core)     | Suíte nova: SQL esperado por dialeto, payloads de injeção, argumentos inválidos, concorrência; uma só biblioteca de asserção; teste de fumaça que executa o SQL gerado em PostgreSQL e MySQL reais                             | G    | F2-03 a F2-11 | Os payloads da prova de conceito viram testes e o SQL gerado roda nos dois bancos                      |
| F2-17 | chore(release) | Publicar `0.1.0-alpha` só com o pacote do núcleo                                                                                                                                                                               | P    | F1-09, F2-16  | Núcleo no nuget.org com o ID novo, sem dependência de driver                                           |

Total: 21 dias.

### Fase 3: schema

É aqui que a ideia original passa a existir: o que o leitor de metadados produz é exatamente o que o builder usa para validar.

| ID    | Tipo              | Item                                                                                                                                                        | Esf. | Depende       | Pronto quando                                      |
| ----- | ----------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | ---- | ------------- | -------------------------------------------------- |
| F3-01 | refactor(schema)  | Modelo de schema imutável: tabela com schema, nome, colunas, chave primária e chaves estrangeiras; comparador de identificadores explícito                  | M    | F2-02         | Chave estrangeira composta é representável         |
| F3-02 | refactor(pack)    | Pacotes de provedor publicáveis, um para PostgreSQL e um para MySQL; o leitor recebe `DbDataSource` ou fábrica de conexão de quem chama                     | G    | F3-01         | Quem usa PostgreSQL não baixa nada do MySQL        |
| F3-03 | fix(mysql)        | Trocar `MySql.Data` por `MySqlConnector`                                                                                                                    | P    | F3-02         | Nenhuma dependência GPL                            |
| F3-04 | fix(pg)           | Reescrever o catálogo do PostgreSQL sobre `pg_catalog`: chave primária real, chaves estrangeiras compostas, tipos definidos pelo usuário, ordem das colunas | M    | F3-01         | Resultado confere com um schema de teste conhecido |
| F3-05 | fix(mysql)        | Reescrever o catálogo do MySQL: colunas qualificadas, chave primária booleana, só chaves estrangeiras de fato, restrito ao banco da conexão                 | M    | F3-01         | A consulta executa e confere com o schema de teste |
| F3-06 | feat(schema)      | Opções do leitor: filtro de schemas e tabelas, views, `CancellationToken`, descarte assíncrono, cache, exceção própria                                      | M    | F3-04, F3-05  | O leitor pode ser cancelado e restrito a uma lista |
| F3-07 | feat(core)        | Resolver todo identificador contra o schema: origem, joins, colunas, filtros, ordenação, agrupamento                                                        | M    | F3-01, F2-06  | Tabela ou coluna desconhecida gera exceção tipada  |
| F3-08 | feat(core)        | Conferir o tipo do valor contra o tipo da coluna                                                                                                            | M    | F3-07         | Texto em coluna numérica é recusado antes do banco |
| F3-09 | fix(security)     | Nenhuma configuração guarda senha imprimível                                                                                                                | P    | F3-02         | `ToString()` e logs não mostram segredo            |
| F3-10 | test(integration) | Testcontainers com PostgreSQL e MySQL: catálogo, execução das queries geradas e suíte de injeção contra banco real                                          | G    | F3-04 a F3-07 | O CI sobe os dois bancos e executa o SQL           |
| F3-11 | fix(examples)     | Exemplo de API com autenticação, verbo HTTP explícito, schema restrito e papel de banco somente leitura                                                     | M    | F3-07         | O exemplo roda com `docker compose`                |
| F3-12 | chore(release)    | Publicar `0.2.0-alpha`, com a primeira versão dos pacotes de provedor                                                                                       | P    | F3-10         | Três pacotes no nuget.org                          |

Total: 14 dias.

### Fase 4: cobertura de SQL

O que falta para o builder servir em relatórios reais. Pode andar em paralelo com a Fase 3.

| ID    | Tipo           | Item                                                                                    | Esf. | Pronto quando                                 |
| ----- | -------------- | --------------------------------------------------------------------------------------- | ---- | --------------------------------------------- |
| F4-01 | feat(core)     | `DISTINCT`                                                                              | P    | Teste por dialeto                             |
| F4-02 | feat(core)     | Agregações (`COUNT`, `SUM`, `AVG`, `MIN`, `MAX`) e alias de coluna                      | M    | Agregação sem texto livre                     |
| F4-03 | feat(core)     | `HAVING`                                                                                | P    | Usa a mesma árvore de predicados              |
| F4-04 | feat(core)     | `OFFSET` e paginação por dialeto                                                        | P    | Página 2 correta nos dois bancos              |
| F4-05 | feat(core)     | Ordenação com direção por coluna e posição de nulos                                     | P    | Direção inválida é recusada                   |
| F4-06 | feat(core)     | `LIKE` com `ESCAPE` e atalhos de contém, começa com e termina com, que escapam curingas | M    | `%` no valor do usuário não vira curinga      |
| F4-07 | feat(core)     | Subqueries: `IN (subquery)`, `EXISTS` e subquery na origem                              | G    | Parâmetros da subquery se fundem aos da query |
| F4-08 | feat(core)     | `UNION`, `INTERSECT`, `EXCEPT`                                                          | M    | Operação inexistente no dialeto é recusada    |
| F4-09 | feat(core)     | `IN` com lista tipada e regra definida para lista vazia                                 | P    | Lista vazia não gera `IN ()`                  |
| F4-10 | docs           | Uma página por recurso, com o SQL gerado em cada dialeto                                | M    | Toda cláusula tem exemplo                     |
| F4-11 | chore(release) | Publicar `0.3.0-beta`                                                                   | P    |                                               |

Total: 9 dias.

### Fase 5: definição de query serializável

O diferencial, se a D1 for a opção B. Uma query descrita em JSON é validada contra o schema e vira SQL parametrizado, sem classes de entidade.

| ID    | Tipo           | Item                                                                                                      | Esf. | Depende      | Pronto quando                                    |
| ----- | -------------- | --------------------------------------------------------------------------------------------------------- | ---- | ------------ | ------------------------------------------------ |
| F5-01 | feat(json)     | Formato JSON versionado da definição de query, com `System.Text.Json` e geração de código                 | G    | Fase 4       | O formato cobre tudo que o builder fluente cobre |
| F5-02 | feat(json)     | JSON para modelo, validação contra o schema e SQL parametrizado, com erros que apontam o campo            | M    | F5-01, F3-07 | Uma definição inválida diz qual campo e por quê  |
| F5-03 | feat(schema)   | Exportar o schema permitido (tabelas, colunas, tipos, relações) para uma tela ou um agente montar a query | M    | F3-06        | A saída alimenta o exemplo do F5-06              |
| F5-04 | feat(json)     | JSON Schema publicado do formato                                                                          | P    | F5-01        | Editores validam a definição                     |
| F5-05 | test(json)     | Testes de propriedade e fuzz sobre a entrada JSON                                                         | M    | F5-02        | Nenhuma entrada gera SQL fora do schema          |
| F5-06 | docs(examples) | Dois exemplos completos: construtor de relatórios (API e página) e ferramenta para agente de IA           | G    | F5-02, F5-03 | Os dois rodam com `docker compose`               |
| F5-07 | chore(release) | Publicar `0.4.0-beta`                                                                                     | P    | F5-05        |                                                  |

Total: 10 dias.

### Fase 6: maturidade de biblioteca

| ID    | Tipo           | Item                                                                                | Esf. | Depende | Pronto quando                               |
| ----- | -------------- | ----------------------------------------------------------------------------------- | ---- | ------- | ------------------------------------------- |
| F6-01 | feat(dialect)  | SQL Server: dialeto, leitor de catálogo e testes de integração                      | GG   | D5      | A suíte de integração passa nos três bancos |
| F6-02 | chore(api)     | Superfície pública rastreada e validação do pacote contra a última versão publicada | M    |         | Quebra de API reprova o PR                  |
| F6-03 | chore(aot)     | Declarar compatibilidade com AOT e trimming, com teste de publicação                | M    | F3-02   | Publicação AOT de um exemplo sem avisos     |
| F6-04 | perf           | Benchmarks de compilação comparados com o SqlKata, com página publicada             | M    |         | Números reproduzíveis no repositório        |
| F6-05 | perf           | Passe de alocações no compilador                                                    | M    | F6-04   | Melhoria medida pelo benchmark              |
| F6-06 | feat(exec)     | Pacote opcional de execução: extensões para `DbConnection` e Dapper                 | M    | D4      | Executar uma query compilada em uma linha   |
| F6-07 | chore(release) | Publicar `0.9.0-rc` e congelar a API                                                | P    | F6-02   | Nenhuma quebra planejada até a 1.0          |

Total: 11 dias.

### Fase 7: documentação de produto

| ID    | Tipo     | Item                                                                                                        | Esf. | Pronto quando                                        |
| ----- | -------- | ----------------------------------------------------------------------------------------------------------- | ---- | ---------------------------------------------------- |
| F7-01 | docs     | README novo: o que é, para quem, início rápido que compila, status e comparação honesta com as alternativas | M    | Um estranho roda o início rápido em 5 minutos        |
| F7-02 | docs     | Site: conceitos, guia por dialeto, definição em JSON, schema, injeção de dependência, erros                 | G    | Toda funcionalidade pública tem página               |
| F7-03 | docs     | Página de segurança: modelo de ameaça, o que é entrada confiável, papel mínimo no banco                     | M    | A página responde "posso passar entrada do usuário?" |
| F7-04 | docs(ci) | Trechos de código da documentação compilados e executados no CI; página inicial derivada do README          | M    | Amostra quebrada reprova o PR                        |
| F7-05 | docs     | Referência de API só da superfície pública, com comentários XML completos                                   | M    | Nenhum membro público sem descrição                  |
| F7-06 | docs     | Site: link a partir do README e da homepage do repositório, links mortos corrigidos, sitemap                | P    | O site aparece na página do repositório              |
| F7-07 | docs     | Guia de migração das versões 0.x para a 1.0                                                                 | P    |                                                      |

Total: 8 dias.

### Fase 8: 1.0 e adoção

| ID    | Tipo             | Item                                                                          | Esf. | Pronto quando                       |
| ----- | ---------------- | ----------------------------------------------------------------------------- | ---- | ----------------------------------- |
| F8-01 | chore(release)   | Conferir os critérios de 1.0 e publicar `1.0.0` com atestação de proveniência | P    | Lista do final desta seção cumprida |
| F8-02 | chore(security)  | OpenSSF Scorecard e selo Best Practices                                       | M    | Selos no README                     |
| F8-03 | docs             | Post de lançamento com comparação; submissão às listas awesome-dotnet         | M    | Publicado e submetido               |
| F8-04 | chore(community) | Rotina de triagem: prazo de primeira resposta, rótulos, uso do Discussions    | P    | Documentada no `CONTRIBUTING`       |
| F8-05 | docs             | Roadmap público em inglês e marcos no GitHub                                  | P    |                                     |
| F8-06 | docs             | Política de suporte de versões e cadência de releases                         | P    |                                     |

Total: 3 dias.

**Critérios de 1.0:**

- Todo valor é parâmetro e todo identificador é resolvido contra o schema e emitido entre aspas.
- Não existe API de texto livre fora da saída de emergência nomeada.
- A suíte de injeção roda no CI contra PostgreSQL, MySQL e SQL Server reais.
- A documentação não faz afirmação de segurança sem teste correspondente.
- O pacote tem ID próprio, licença declarada e nenhuma dependência GPL.
- Os releases saem pelo CI, com publicação confiável e proveniência.

### 5.3 Depois da 1.0

Fora do cronograma, em ordem provável de demanda: SQLite e Oracle; `INSERT`, `UPDATE`, `DELETE` e upsert; CTE; `CASE`; funções de janela; `netstandard2.0` no núcleo; analisador Roslyn que valida queries em tempo de compilação. Procedures armazenadas, citadas no roadmap atual do README, não combinam com o posicionamento da D1.

### 5.4 Conversão para calendário

Os 90 dias são de trabalho focado. A duração depende de quanto tempo por semana o projeto recebe, e foi falta de tempo que o parou da primeira vez.

| Dedicação           | Seguro para mostrar (35 dias) | Com diferencial (68 dias) | 1.0 (90 dias) | Faixa para a 1.0 |
| ------------------- | ----------------------------- | ------------------------- | ------------- | ---------------- |
| Tempo integral      | 7 semanas                     | 14 semanas                | 18 semanas    | 3,5 a 6 meses    |
| 20 horas por semana | 14 semanas                    | 27 semanas                | 36 semanas    | 7 a 12,5 meses   |
| 10 horas por semana | 28 semanas                    | 54 semanas                | 72 semanas    | 13 a 25 meses    |

### 5.5 Se o tempo for curto

- **Não cortar:** Fase 0 inteira, F2-03, F2-05, F2-06, F3-07 e F3-10. Sem eles a biblioteca continua insegura.
- **Primeiro a cortar:** F6-01 (SQL Server), F6-05, F6-06, F4-07 e F4-08. São 12 dias.
- **Atalho maior:** a opção C da D1, que troca o compilador próprio pelo do SqlKata.
- **Menor entrega útil:** Fases 0 a 3, cerca de 49 dias. É uma biblioteca segura e com validação de schema, sem JSON.

## 6. Riscos

- **Dedicação.** O projeto parou uma vez por falta de tempo e recurso. O plano foi ordenado para que cada fase termine em um estado publicável, de modo que uma nova pausa não deixe trabalho pela metade.
- **A janela do nicho está se fechando.** `SqlKata.JsonQuery`, `Querio` e `SqExpress` se moveram na direção da D1 entre julho e novembro de 2026, e a Microsoft entrega a versão servidor do mesmo padrão.
- **A Fase 2 é uma reescrita.** Quase nada do código atual sobrevive. Estimativas de reescrita costumam estourar; por isso a faixa superior é 50% acima do total.
- **Adoção não é garantida por qualidade.** A evidência sobre o que faz uma biblioteca .NET decolar é fraca. O que se sabe: manutenção ativa e documentação são as duas queixas mais comuns de quem escolhe pacotes.
- **Licença.** Manter AGPL mais comercial contradiz o objetivo de mainstream segundo todos os casos encontrados.
- **Escopo.** Mais dialetos e comandos de escrita antes da 1.0 atrasam o diferencial sem aumentar a chance de adoção.

## Apêndice A: cobertura de SQL hoje

| Recurso                                     | Hoje                                          | Onde entra    |
| ------------------------------------------- | --------------------------------------------- | ------------- |
| Lista de colunas e `*`                      | Presente                                      | F2-02         |
| `FROM` de uma tabela                        | Parcial: maiúsculas, sem schema, sem alias    | F2-06         |
| Alias de tabela e coluna                    | Só como texto livre                           | F2-06, F4-02  |
| `DISTINCT`                                  | Ausente                                       | F4-01         |
| Agregações                                  | Só como texto livre                           | F4-02         |
| `INNER`, `LEFT`, `RIGHT`, `FULL JOIN`       | Presente; `FULL` inválido no MySQL            | F2-07         |
| `CROSS JOIN`                                | Quebrado: sempre com `ON`                     | F2-07         |
| `SELF JOIN`                                 | Quebrado: palavra-chave inexistente           | F2-07         |
| Comparações `=`, `<>`, `>`, `<`, `>=`, `<=` | Presente, sem parâmetros                      | F2-03         |
| Operador em texto livre                     | Presente; vetor de injeção                    | F2-13         |
| `IN`, `NOT IN`                              | Parcial: lista vazia gera `IN ()`             | F4-09         |
| `IS NULL`, `IS NOT NULL`                    | Presente                                      | F2-02         |
| `BETWEEN`                                   | Presente                                      | F2-02         |
| `LIKE`, `ILIKE`                             | Parcial: `ILIKE` sai para MySQL; sem `ESCAPE` | F2-04, F4-06  |
| `AND`, `OR`                                 | Parcial: sem agrupamento                      | F2-05         |
| `NOT` e grupos                              | Ausente                                       | F2-05         |
| `EXISTS` e subqueries                       | Ausente                                       | F4-07         |
| `GROUP BY`                                  | Presente                                      | F2-02         |
| `HAVING`                                    | Ausente                                       | F4-03         |
| `ORDER BY`                                  | Presente; direção não validada                | F4-05         |
| `LIMIT`                                     | Presente; aceita negativo                     | F2-14         |
| `OFFSET`                                    | Ausente                                       | F4-04         |
| `UNION`, `INTERSECT`, `EXCEPT`              | Ausente                                       | F4-08         |
| Parâmetros                                  | Ausente                                       | F2-03         |
| Aspas em identificadores                    | Ausente                                       | F2-06         |
| Saída por dialeto                           | Ausente                                       | F2-04         |
| CTE, `CASE`, janelas, escrita               | Ausente                                       | Depois da 1.0 |

## Apêndice B: documentação que não bate com o código

| #   | Onde                                              | O que acontece                                                                                    |
| --- | ------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| 1   | `README.md:47,52`; `docs/getting-started.md:6,12` | Instala o `DynamicQueryBuilder` da Oplog                                                          |
| 2   | `README.md:61`                                    | Não compila: `AddDynamicQueryBuilder` recebe `DynamicQueryBuilderSettings`, não uma string        |
| 3   | `README.md:77`                                    | Não compila: `GetDatabaseTablesMetaDataAsync` não recebe argumentos                               |
| 4   | `README.md:87-110`; `getting-started.md:98-123`   | `queryBuilder` nunca é declarado; `Setup` nunca é mostrado                                        |
| 5   | As mesmas seis amostras                           | Os comentários `// Output:` só valem com chaves em maiúsculas; com `"Users"` lançam exceção       |
| 6   | `DynamicQueryBuilderModule.cs:23-27`              | O exemplo do comentário XML não compila: o record é posicional e a propriedade é `DatabaseDriver` |
| 7   | `ComparisonOperators.cs:12,20,27`                 | XML malformado; três membros saem sem descrição no site                                           |
| 8   | `getting-started.md:55` e `:67`                   | "Primary key status", mas o valor mostrado é a expressão padrão da coluna                         |
| 9   | `README.md:31-33`                                 | `CONCAT` lança exceção; operadores de conjunto e `NOT` não têm API                                |
| 10  | `README.md:116,148`                               | "documentation site" aponta para a pasta do repositório, não para o site                          |
| 11  | `README.md:121-122`                               | O `ConsoleClient` é "Hello, World!"; o `ClientWebAPI` só mostra metadados e não compila           |

## Apêndice C: cobertura de testes

Medida com coverlet; igual em net8.0 e net9.0.

| Classe                            | Linhas    | Ramos     |
| --------------------------------- | --------- | --------- |
| `QueryBuilder` (os dois arquivos) | 91,3%     | 75,3%     |
| `QueryBuilder.cs`                 | 97,5%     | 94,1%     |
| `QueryBuilderFilters.cs`          | 84,5%     | 54,3%     |
| `DriversFactory`                  | 100%      | 100%      |
| `GetDatabaseMetaData`             | 0%        | 0%        |
| `DynamicQueryBuilderModule`       | 0%        | n/a       |
| `DatabaseTablesMetaData`          | 0%        | n/a       |
| **Total**                         | **79,4%** | **64,3%** |

## Apêndice D: alternativas no mercado

Downloads totais no nuget.org e estrelas no GitHub, lidos em 24/11/2026.

| Biblioteca                          | Downloads | Estrelas                | Última versão estável       | Licença       | Parametriza valores               |
| ----------------------------------- | --------- | ----------------------- | --------------------------- | ------------- | --------------------------------- |
| SqlKata                             | 29,4 M    | 3.381                   | 4.0.1, fev/2025             | MIT           | Sim                               |
| Dapper.SqlBuilder                   | 29,6 M    | 18.417 (repo do Dapper) | 2.1.100, nov/2026           | Apache-2.0    | Só se quem chama escrever `@nome` |
| linq2db                             | 29,6 M    | 3.332                   | 6.6.0, nov/2026             | MIT           | Sim                               |
| RepoDb                              | 3,3 M     | 1.912                   | 1.17.1, nov/2026            | Apache-2.0    | Sim                               |
| InterpolatedSql                     | 1,7 M     | 279                     | 2.5.1, fev/2026             | MIT           | Sim                               |
| SqExpress                           | 32 mil    | 111                     | 1.4.0, out/2026             | MIT           | Sim                               |
| Gridify (filtra `IQueryable`)       | 3,7 M     | 1.174                   | 2.20.0, set/2026            | MIT           | Delegado ao provedor LINQ         |
| DynamicQueryBuilder da Oplog (LINQ) | 92 mil    | 43                      | 1.1.0, fev/2025             | MIT           | Delegado ao provedor LINQ         |
| Querio                              | 3 mil     | 4                       | Primeira versão em ago/2026 | MIT           | Sim                               |
| SqlKata.JsonQuery                   | 437       | não levantado           | Primeira versão em jul/2026 | não levantado | Sim                               |

## Apêndice E: nomes candidatos

Todos devolveram 404 no índice do nuget.org em 24/11/2026.

| ID           | Observação                                                                               |
| ------------ | ---------------------------------------------------------------------------------------- |
| `SchemaSql`  | Sem pacotes com esse prefixo; 10 repositórios no GitHub, nenhum .NET, todos sem estrelas |
| `KnownSql`   | Nenhum repositório no GitHub                                                             |
| `StrictSql`  | Um repositório em Go, sem estrelas                                                       |
| `GuardedSql` | Nada em nenhum dos dois                                                                  |
| `CatalogSql` | Nada no GitHub                                                                           |
| `Querent`    | Usável; 51 repositórios no GitHub com o nome                                             |
| `QueryLoom`  | Usável; 8 repositórios pequenos em outras linguagens                                     |

Evitar: `SchemaBound` e `VeriSql` (projetos existentes com o mesmo nome). Já ocupados: `SchemaGuard`, `SafeQuery`, `QueryForge`, `SqlForge`, `DynamicSql`, `SqlBuilder`, `QueryBuilder`.

## Apêndice F: fontes

- Suporte do .NET: <https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core> e <https://builds.dotnet.microsoft.com/dotnet/release-metadata/releases-index.json>
- Alvos recomendados para bibliotecas: <https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/cross-platform-targeting>
- Boas práticas de pacote: <https://learn.microsoft.com/en-us/nuget/create-packages/package-authoring-best-practices>
- Reserva de prefixo: <https://learn.microsoft.com/en-us/nuget/nuget-org/id-prefix-reservation>
- Trusted Publishing: <https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing>
- Validação de pacote: <https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/overview>
- Pacote da Oplog: <https://github.com/oplog/DynamicQueryBuilder>
- SqlKata: <https://github.com/sqlkata/querybuilder>
- Padrão recomendado para agentes: <https://devblogs.microsoft.com/azure-sql/sql-mcp-server-nl2sql/> e <https://genai.owasp.org/llmrisk/llm052025-improper-output-handling/>
- Texto da AGPL-3.0: <https://www.gnu.org/licenses/agpl-3.0.txt>
- Exceção de software livre da Oracle: <https://oss.oracle.com/licenses/universal-foss-exception/>
- MySqlConnector: <https://mysqlconnector.net/tutorials/migrating-from-connector-net/>
- Política de AGPL do Google: <https://opensource.google/documentation/reference/using/agpl-policy>
- Requisitos da .NET Foundation: <https://github.com/dotnet-foundation/projects>
- Critérios do awesome-dotnet: <https://github.com/quozd/awesome-dotnet/blob/master/CONTRIBUTING.md>
- OpenSSF Scorecard: <https://github.com/ossf/scorecard/blob/main/docs/checks.md>
- Pesquisa do ecossistema NuGet: <https://devblogs.microsoft.com/nuget/state-of-the-nuget-ecosystem/>
