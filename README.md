# Рекурсивный спуск для варианта 26

## Грамматика

```
<While> → do <Stmt> while <Cond> ;
<Stmt> → var as <ArithExpr>
<Cond> → <LogExpr> { or <LogExpr> }
<LogExpr> → <RelExpr> { and <RelExpr> }
<RelExpr> → <Operand> [ rel <Operand> ]
<Operand> → var | const
<ArithExpr> → <Operand> { ao <Operand> }
```

rel ∈ { `<`, `<=`, `>`, `>=`, `==`, `!=` }
ao ∈ { `+`, `-` }
as = `=`
var — идентификатор (буквы)
const— число


## Язык
- **C#**, Windows Forms (.NET Framework)
- Лексер и парсер объединены в одном проекте `Laba_1`

## Классификация грамматики
- **Контекстно-свободная грамматика (КС)**  
- Подходит для разбора методом **LL(1)** (рекурсивный спуск без левой рекурсии)

## Схема вызова функций
```
ParseWhile()
├─ ExpectKeyword("do")
├─ ParseStmt()
│ ├─ Match Identifier
│ ├─ Expect AssignOp ("=")
│ └─ ParseArithExpr()
│ ├─ ParseOperand()
│ └─ { ao ParseOperand() }
├─ ExpectKeyword("while")
└─ ParseCond()
├─ ParseLogExpr()
│ ├─ ParseRelExpr()
│ │ ├─ ParseOperand()
│ │ └─ [ rel ParseOperand() ]
│ └─ { "and" ParseRelExpr() }
└─ { "or" ParseLogExpr() }
└─ Expect Semicolon (";")
```

## Тестовые примеры
### Корректные
```do x = 5 + 3 - 1 while a < b and b != 0;```

![image](https://github.com/user-attachments/assets/fa89adde-7f4c-42a0-b81b-b304a1222867)

```do result = a - b + 10 while value >= 100 or flag == 1;```

![image](https://github.com/user-attachments/assets/12634c7e-d2e4-4a32-b7bc-14673bbdc7b1)

### С ошибками
```while x < 10 do x = 1 +;      // Ожидалось "do", затем операнд после '+'```

![image](https://github.com/user-attachments/assets/d31dce90-7352-42d8-be79-7a30cde6e87e)

```do y = -5 * 2 while z ==;    // Ожидался операнд после rel-оператора```

![image](https://github.com/user-attachments/assets/051fa28b-2331-4f0c-9a8c-1b5c7b67b74c)

```do a = 1 + 2 while;          // Ожидалось условие после "while"```

![image](https://github.com/user-attachments/assets/f688e010-e767-495b-8d15-3f5c1eedf06e)
