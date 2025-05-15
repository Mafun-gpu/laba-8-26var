using System;
using System.Collections.Generic;
using System.Text;

namespace Laba_1
{
    public enum TokenCode
    {
        Keyword = 1,    // ключевое слово (do, while, and, or, val)
        Identifier = 2,    // идентификатор (var)
        Space = 3,    // пробел (не используется в парсере)
        AssignOp = 4,    // оператор присваивания '='
        LBracket = 5,    // '('
        StringLiteral = 6,  // строковый литерал
        Comma = 7,    // ','
        RBracket = 8,    // ')'
        Semicolon = 9,    // ';'
        Integer = 10,   // целое число
        Float = 11,   // вещественное число
        ListOf = 12,   // ключевое слово listOf (старый вариант)
        RelOp = 13,   // реляционный оператор: <, <=, >, >=, ==, !=
        AoOp = 14,   // арифметический оператор: +, -
        Error = 99    // недопустимый символ
    }

    public class Token
    {
        public TokenCode Code { get; set; }
        public string Type { get; set; }
        public string Lexeme { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }
        public int Line { get; set; }

        public override string ToString()
            => $"[{Line}:{StartPos}-{EndPos}] ({(int)Code}) {Type} : '{Lexeme}'";
    }

    public class Scanner
    {
        private string _text;
        private int _pos;
        private int _line;
        private int _linePos;
        private List<Token> _tokens;

        public Scanner()
        {
            _tokens = new List<Token>();
        }

        public List<Token> Scan(string text)
        {
            _text = text;
            _pos = 0;
            _line = 1;
            _linePos = 1;
            _tokens.Clear();

            while (!IsEnd())
            {
                // 1) пропускаем пробелы
                if (char.IsWhiteSpace(CurrentChar()))
                {
                    Advance();
                    continue;
                }

                char ch = CurrentChar();

                // 2) число (целое или вещественное, с опциональным '-')
                if (char.IsDigit(ch) || (ch == '-' && char.IsDigit(PeekNext())))
                {
                    ReadNumber();
                }
                // 3) идентификатор или ключевое слово (только латинские буквы)
                else if (IsLatinLetter(ch))
                {
                    ReadIdentifierOrKeyword();
                }
                // 4) всё прочее — символы операций и разделителей
                else
                {
                    switch (ch)
                    {
                        // реляционные операторы: <=, >=, ==, !=, <, >
                        case '<':
                        case '>':
                        case '!':
                        case '=':
                            {
                                // пытаемся снять двухсимвольные: <=, >=, !=, ==
                                string two = $"{ch}{PeekNext()}";
                                if (two == "<=" || two == ">=" || two == "!=" || two == "==")
                                {
                                    AddToken(TokenCode.RelOp, "реляционный оператор", two);
                                    Advance(); Advance();
                                }
                                else if (ch == '<' || ch == '>')
                                {
                                    AddToken(TokenCode.RelOp, "реляционный оператор", ch.ToString());
                                    Advance();
                                }
                                else if (ch == '=')
                                {
                                    // одиночный '=' — оператор присваивания
                                    AddToken(TokenCode.AssignOp, "оператор присваивания", "=");
                                    Advance();
                                }
                                else
                                {
                                    // '!' без '=' — некорректно
                                    AddToken(TokenCode.Error, "недопустимый символ", "!");
                                    Advance();
                                }
                            }
                            break;

                        // арифметические операторы '+' и '-'
                        case '+':
                            AddToken(TokenCode.AoOp, "арифметический оператор", "+");
                            Advance();
                            break;
                        case '-':
                            // если здесь, то PeekNext() не цифра (ReadNumber уже съел "-5")
                            AddToken(TokenCode.AoOp, "арифметический оператор", "-");
                            Advance();
                            break;

                        case '(':
                            AddToken(TokenCode.LBracket, "открывающая скобка", "(");
                            Advance();
                            break;
                        case ')':
                            AddToken(TokenCode.RBracket, "закрывающая скобка", ")");
                            Advance();
                            break;
                        case ',':
                            AddToken(TokenCode.Comma, "запятая", ",");
                            Advance();
                            break;
                        case ';':
                            AddToken(TokenCode.Semicolon, "конец оператора", ";");
                            Advance();
                            break;
                        case '"':
                            ReadStringLiteral();
                            break;
                        default:
                            // всё остальное — ошибка
                            AddToken(TokenCode.Error, "недопустимый символ", ch.ToString());
                            Advance();
                            break;
                    }
                }
            }

            return _tokens;
        }

        private void ReadIdentifierOrKeyword()
        {
            int startPos = _linePos;
            var sb = new StringBuilder();

            while (!IsEnd() && IsLatinLetter(CurrentChar()))
            {
                sb.Append(CurrentChar());
                Advance();
            }

            string lexeme = sb.ToString();
            // ключевые слова вашей грамматики
            if (lexeme == "val")
                AddToken(TokenCode.Keyword, "ключевое слово", lexeme, startPos, _linePos - 1, _line);
            else if (lexeme == "listOf")
                AddToken(TokenCode.ListOf, "ключевое слово", lexeme, startPos, _linePos - 1, _line);
            else if (lexeme == "do" || lexeme == "while" || lexeme == "and" || lexeme == "or")
                AddToken(TokenCode.Keyword, "ключевое слово", lexeme, startPos, _linePos - 1, _line);
            else
                AddToken(TokenCode.Identifier, "идентификатор", lexeme, startPos, _linePos - 1, _line);
        }

        private void ReadNumber()
        {
            int startPos = _linePos;
            var sb = new StringBuilder();
            bool isFloat = false;

            if (CurrentChar() == '-')
            {
                sb.Append(CurrentChar());
                Advance();
            }

            while (!IsEnd())
            {
                if (char.IsDigit(CurrentChar()))
                {
                    sb.Append(CurrentChar());
                    Advance();
                }
                else if (CurrentChar() == '.' && char.IsDigit(PeekNext()) && !isFloat)
                {
                    isFloat = true;
                    sb.Append(CurrentChar());
                    Advance();
                }
                else break;
            }

            string lexeme = sb.ToString();
            if (isFloat)
                AddToken(TokenCode.Float, "вещественное число", lexeme, startPos, _linePos - 1, _line);
            else
                AddToken(TokenCode.Integer, "целое число", lexeme, startPos, _linePos - 1, _line);
        }

        private void ReadStringLiteral()
        {
            int startPos = _linePos;
            Advance(); // пропускаем открывающую "
            var sb = new StringBuilder();
            bool closed = false;

            while (!IsEnd())
            {
                char ch = CurrentChar();
                if (ch == '"')
                {
                    closed = true;
                    Advance(); // пропускаем закрывающую "
                    break;
                }
                else
                {
                    sb.Append(ch);
                    Advance();
                }
            }

            if (closed)
                AddToken(TokenCode.StringLiteral, "строковый литерал", sb.ToString(), startPos, _linePos - 1, _line);
            else
                AddToken(TokenCode.Error, "незакрытая строка", sb.ToString(), startPos, _linePos - 1, _line);
        }

        // Помощники

        private bool IsLatinLetter(char ch)
            => (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');

        private bool IsEnd() => _pos >= _text.Length;
        private char CurrentChar() => IsEnd() ? '\0' : _text[_pos];
        private char PeekNext() => (_pos + 1 < _text.Length) ? _text[_pos + 1] : '\0';

        private void Advance()
        {
            if (CurrentChar() == '\n')
            {
                _line++;
                _linePos = 0;
            }
            _pos++;
            _linePos++;
        }

        private void AddToken(TokenCode code, string type, string lexeme,
                              int startPos, int endPos, int line)
        {
            _tokens.Add(new Token
            {
                Code = code,
                Type = type,
                Lexeme = lexeme,
                StartPos = startPos,
                EndPos = endPos,
                Line = line
            });
        }

        private void AddToken(TokenCode code, string type, string lexeme)
            => AddToken(code, type, lexeme, _linePos, _linePos, _line);
    }
}