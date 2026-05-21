# Ideias futuras - Testador CLP HI

## Refatoracao assistida por Codex

- Usar Codex em ciclo separado para refatorar e limpar ainda mais a MainForm.cs.
- Manter a MainForm focada em composicao, eventos e services.
- Evitar misturar refatoracao com alteracoes visuais ou novas funcionalidades.

## Gerenciamento de programa no CLP

- Avaliar possibilidade de o testador detectar se o CLP ja esta rodando algum programa.
- Pesquisar forma segura de identificar programa carregado ou em execucao.
- Pesquisar forma segura de remover ou apagar programa existente quando aplicavel.
- Pesquisar forma segura de carregar ou instalar novo programa no CLP.

## Cuidados tecnicos

- Tratar como pesquisa futura antes de implementar.
- Validar dependencias com HIstudio, protocolo HI e documentacao do fabricante.
- Evitar qualquer acao destrutiva sem confirmacao explicita.
- Testar primeiro em bancada, nunca direto em ambiente operacional.
