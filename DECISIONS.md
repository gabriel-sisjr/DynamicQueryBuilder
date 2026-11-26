# Decisões do projeto

Registro curto das decisões que orientam o cronograma do `ROADMAP.md` (seção 4). Cada entrada diz o que foi decidido, por quê e o que muda no plano. Decisões técnicas posteriores (F2-01) entram neste mesmo arquivo.

Tomadas em 26/11/2026 pelo mantenedor.

## D1. O que a biblioteca é

**Decisão:** opção B. A biblioteca faz queries dinâmicas validadas contra o schema real: introspecção do banco, definição de query serializável, validação e SQL parametrizado. O builder fluente passa a ser a interface programática desse mesmo modelo.

**Por quê:** é o que o projeto já tentava fazer e é o único espaço que nenhuma biblioteca estabelecida ocupa. Como builder fluente genérico (opção A), o projeto concorreria com o SqlKata partindo de trás. A opção C reduziria o esforço, mas dependeria de uma biblioteca com uma única versão estável desde 2022.

**Consequência:** o cronograma segue como está, incluindo a Fase 5 (definição em JSON).

## D2. Nome e ID do pacote

**Decisão:** opção A. O pacote, o assembly e o namespace raiz recebem um nome novo, alinhado à D1. O nome é escolhido no F1-02, entre os candidatos do Apêndice E ou outro livre no nuget.org na data.

**Por quê:** o ID `DynamicQueryBuilder` no nuget.org pertence à Oplog e não pode ser publicado. Manter o nome com um prefixo seria menos trabalho, mas o pacote continuaria confundível com o deles.

**Consequência:** nada foi publicado ainda, então renomear não quebra ninguém. Até o F1-02, a documentação não cita nome novo nenhum.

## D3. Licença

**Decisão:** opção A, com MIT.

**Por quê:** o objetivo é chegar ao mainstream. A licença permissiva remove a maior barreira de adoção: entre os 1.000 pacotes mais usados do nuget.org fora os da Microsoft, 45% são MIT. A monetização fica para depois, por patrocínio ou módulos pagos.

**Consequência:**

- O F1-03 aplica a decisão: texto integral da MIT em `LICENSE`, selos coerentes no README e licença declarada no pacote.
- Saem o aviso de AGPL e o `COMMERCIAL_LICENSE.md`.
- Não há CLA.

## D4. Gerar SQL ou também executar

**Decisão:** opção A. O núcleo só gera `{ Sql, Parâmetros }`.

**Por quê:** assim o núcleo fica sem dependência de driver. A introspecção fica em pacotes por banco, que recebem a conexão de quem chama. A biblioteca não compete com Dapper e EF Core na execução.

**Consequência:** a execução vira pacote opcional (F6-06). Os itens F2-02, F3-02 e F3-09 seguem esta fronteira.

## D5. Bancos e frameworks

**Decisão:** opção C. É a opção A (PostgreSQL e MySQL até o beta, SQL Server antes da 1.0, `net10.0` e `net8.0`) mais `netstandard2.0` no núcleo.

**Por quê:** alcance máximo para o núcleo, que não depende de driver (D4).

**Consequências:**

- `netstandard2.0` só é viável depois do F2-02, quando o núcleo vira um projeto separado. O F1-05 continua com `net10.0` e `net8.0`.
- O alvo `netstandard2.0` precisa entrar no núcleo novo antes do `0.9.0-rc`, quando a API congela (F6-07). Ele restringe recursos de linguagem e exige polyfills.
- O item sai da lista "Depois da 1.0" (seção 5.3). O custo não está estimado no cronograma.
- `netstandard2.0` vale só para o núcleo. Os pacotes de provedor seguem os alvos dos drivers (F3-02); o Npgsql 11, por exemplo, só tem alvo `net10.0`.
