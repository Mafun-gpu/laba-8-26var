using System;
using System.Collections.Generic;
using System.Text;

namespace Laba_1
{
    /// <summary>
    /// Узел дерева разбора для трассировки или визуализации.
    /// </summary>
    public class ParseNode
    {
        public string Name { get; }
        public List<ParseNode> Children { get; } = new List<ParseNode>();

        public ParseNode(string name)
        {
            Name = name;
        }

        public void AddChild(ParseNode child)
        {
            Children.Add(child);
        }

        /// <summary>
        /// Рекурсивно строит текстовое представление дерева с отступами.
        /// </summary>
        public string ToTreeString(int indent = 0)
        {
            var sb = new StringBuilder();
            sb.Append(new string(' ', indent * 2))
              .AppendLine(Name);
            foreach (var child in Children)
                sb.Append(child.ToTreeString(indent + 1));
            return sb.ToString();
        }
    }

    /// <summary>
    /// Рекурсивный спуск-парсер для грамматики варианта 26:
    ///   <While>     → do <Stmt> while <Cond> ;
    ///   <Stmt>      → var as <ArithExpr>
    ///   <Cond>      → <LogExpr> { or <LogExpr> }
    ///   <LogExpr>   → <RelExpr> { and <RelExpr> }
    ///   <RelExpr>   → <Operand> [ rel <Operand> ]
    ///   <Operand>   → var | const
    ///   <ArithExpr> → <Operand> { ao <Operand> }
    /// rel ∈ {<, <=, >, >=, ==, !=}, ao ∈ {+, -}, as = “=”
    /// </summary>
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;
        private ParseNode _root;

        /// <summary>Список сообщений об ошибках.</summary>
        public List<string> Errors { get; } = new List<string>();

        // Наборы лексем для rel- и ao-операторов
        private static readonly HashSet<string> RelOps = new HashSet<string> { "<", "<=", ">", ">=", "==", "!=" };
        private static readonly HashSet<string> AoOps = new HashSet<string> { "+", "-" };

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;
        }

        /// <summary>Входная точка: парсит конструкцию <While>.</summary>
        public ParseNode ParseWhile()
        {
            _root = ParseWhileNode();
            return _root;
        }

        /// <summary>Текстовый вывод дерева разбора (для RichTextBox).</summary>
        public string GetTraceText()
        {
            return _root?.ToTreeString() ?? string.Empty;
        }

        // -------------------- Методы для нетерминалов --------------------

        private ParseNode ParseWhileNode()
        {
            var node = new ParseNode("While");

            ExpectKeyword("do", node);
            node.AddChild(ParseStmt());
            ExpectKeyword("while", node);
            node.AddChild(ParseCond());
            Expect(TokenCode.Semicolon, ";", node);

            return node;
        }

        private ParseNode ParseStmt()
        {
            var node = new ParseNode("Stmt");

            // var
            if (Current != null && Current.Code == TokenCode.Identifier)
            {
                node.AddChild(new ParseNode(Current.Lexeme));
                Advance();
            }
            else
            {
                AddError($"[{Position(Current)}] Ожидался идентификатор");
            }

            // =
            Expect(TokenCode.AssignOp, "=", node);

            // <ArithExpr>
            node.AddChild(ParseArithExpr());

            return node;
        }

        private ParseNode ParseCond()
        {
            var node = new ParseNode("Cond");

            // <LogExpr>
            node.AddChild(ParseLogExpr());

            // { or <LogExpr> }
            while (IsKeyword("or"))
            {
                node.AddChild(new ParseNode("or"));
                Advance();
                node.AddChild(ParseLogExpr());
            }

            return node;
        }

        private ParseNode ParseLogExpr()
        {
            var node = new ParseNode("LogExpr");

            // <RelExpr>
            node.AddChild(ParseRelExpr());

            // { and <RelExpr> }
            while (IsKeyword("and"))
            {
                node.AddChild(new ParseNode("and"));
                Advance();
                node.AddChild(ParseRelExpr());
            }

            return node;
        }

        private ParseNode ParseRelExpr()
        {
            var node = new ParseNode("RelExpr");

            // <Operand>
            node.AddChild(ParseOperand());

            // [ rel <Operand> ]
            if (Current != null && RelOps.Contains(Current.Lexeme))
            {
                node.AddChild(new ParseNode(Current.Lexeme));
                Advance();
                node.AddChild(ParseOperand());
            }

            return node;
        }

        private ParseNode ParseOperand()
        {
            var node = new ParseNode("Operand");

            if (Current != null && Current.Code == TokenCode.Identifier)
            {
                node.AddChild(new ParseNode(Current.Lexeme));
                Advance();
            }
            else if (Current != null &&
                     (Current.Code == TokenCode.Integer || Current.Code == TokenCode.Float))
            {
                node.AddChild(new ParseNode(Current.Lexeme));
                Advance();
            }
            else
            {
                AddError($"[{Position(Current)}] Ожидался операнд (var или const)");
            }

            return node;
        }

        private ParseNode ParseArithExpr()
        {
            var node = new ParseNode("ArithExpr");

            // <Operand>
            node.AddChild(ParseOperand());

            // { ao <Operand> }
            while (Current != null && AoOps.Contains(Current.Lexeme))
            {
                node.AddChild(new ParseNode(Current.Lexeme));
                Advance();
                node.AddChild(ParseOperand());
            }

            return node;
        }

        // -------------------- Вспомогательные методы --------------------

        /// <summary>Текущий токен или null, если конец.</summary>
        private Token Current => _pos < _tokens.Count ? _tokens[_pos] : null;

        /// <summary>Сдвиг на следующий токен.</summary>
        private void Advance()
        {
            if (_pos < _tokens.Count) _pos++;
        }

        /// <summary>Проверяет и «съедает» ключевое слово.</summary>
        private bool IsKeyword(string kw)
        {
            return Current != null
                && Current.Code == TokenCode.Keyword
                && Current.Lexeme == kw;
        }

        /// <summary>Ожидает конкретное ключевое слово, иначе — пишет ошибку.</summary>
        private void ExpectKeyword(string kw, ParseNode parent)
        {
            if (IsKeyword(kw))
            {
                parent.AddChild(new ParseNode(kw));
                Advance();
            }
            else
            {
                AddError($"[{Position(Current)}] Ожидалось ключевое слово \"{kw}\"");
            }
        }

        /// <summary>Ожидает терминал заданного кода и лексемы, иначе — пишет ошибку.</summary>
        private void Expect(TokenCode code, string lexeme, ParseNode parent)
        {
            if (Current != null
                && Current.Code == code
                && Current.Lexeme == lexeme)
            {
                parent.AddChild(new ParseNode(lexeme));
                Advance();
            }
            else
            {
                AddError($"[{Position(Current)}] Ожидался \"{lexeme}\"");
            }
        }

        /// <summary>Возвращает позицию токена в формате "строка:столбец".</summary>
        private string Position(Token t)
        {
            return t != null
                ? $"{t.Line}:{t.StartPos}"
                : "EOF";
        }

        /// <summary>Добавляет сообщение об ошибке в список.</summary>
        private void AddError(string message)
        {
            Errors.Add(message);
        }
    }
}