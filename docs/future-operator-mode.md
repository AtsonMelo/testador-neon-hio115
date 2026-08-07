# Modo Operador futuro

## Separação de responsabilidades

- Testador: manutenção, diagnóstico e comissionamento supervisionado;
- Simulador: desenvolvimento e treinamento sem hardware;
- Operador: operação real de uma máquina.

[Bloqueado] O Modo Operador não faz parte desta sessão e não possui tela,
serviço, transporte ou comando implementado.

## Gate futuro mínimo

Uma milestone de Operador deverá definir separadamente:

- modelo operacional e autoridade do usuário;
- permissivos e intertravamentos reais;
- estratégia de segurança funcional;
- estados degradados e perda de comunicação;
- registro de auditoria e retenção;
- procedimentos elétricos e mecânicos;
- revisão de riscos e aprovação formal.

O Modo Operador não deve reutilizar implicitamente gates de teste ou parâmetros
de simulação.
