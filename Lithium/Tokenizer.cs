#pragma warning disable 8618, 8629

using System.Text;

namespace Lithium;

class Tokenizer {
    private int index = -1, lineNum = 0;
    private readonly string code;

    public Tokenizer(string path)
    {
        using StreamReader sr = new StreamReader(path, Encoding.UTF8);
        code = sr.ReadToEnd();
    }

    private char? peek(int n = 1) {
        if(index + n < code.Length) {
            return code[index + n];
        }
        return null;
    }

    private char consume() {
        index++;
        return code[index];
    }

    public List<Token> tokenizeCode() {
        List<Token> tokens = [];
        string buf = "";
        while(peek() != null) {
            if(Char.IsLetter((char)peek())) {
                while(Char.IsLetterOrDigit((char)peek())) {
                    buf += consume();
                }
                switch(buf) {
                    case "exit":
                        tokens.Add(new Token(TokenTypes._exit, lineNum));
                        break;
                    case "print":
                        tokens.Add(new Token(TokenTypes._print, lineNum));
                        break;
                    case "if":
                        tokens.Add(new Token(TokenTypes._if, lineNum));
                        break;
                    case "elif":
                        tokens.Add(new Token(TokenTypes._elif, lineNum));
                        break;
                    case "else":
                        tokens.Add(new Token(TokenTypes._else, lineNum));
                        break;
                    case "for":
                        tokens.Add(new Token(TokenTypes._for, lineNum));
                        break;
                    case "while":
                        tokens.Add(new Token(TokenTypes._while, lineNum));
                        break;
                    case "func":
                        tokens.Add(new Token(TokenTypes._func, lineNum));
                        break;
                    case "return":
                        tokens.Add(new Token(TokenTypes._return, lineNum));
                        break;
                    case "int":
                        tokens.Add(new Token(TokenTypes._int, lineNum));
                        break;
                    case "bool":
                        tokens.Add(new Token(TokenTypes._bool, lineNum));
                        break;
                    case "char":
                        tokens.Add(new Token(TokenTypes._char, lineNum));
                        break;
                    case "void":
                        tokens.Add(new Token(TokenTypes._void, lineNum));
                        break;
                    default:
                        tokens.Add(new Token(TokenTypes.identifier, buf, lineNum));
                        break;

                        /*
                        tokens.Add( buf switch {
                        "exit" => new Token(TokenTypes._exit, lineNum),
                        "print" => new Token(TokenTypes._print, lineNum),
                        "if" => new Token(TokenTypes._if, lineNum),
                        "elif" => tokens.Add(new Token(TokenTypes._elif, lineNum),
                        "else" => tokens.Add(new Token(TokenTypes._else, lineNum),
                        "for" => tokens.Add(new Token(TokenTypes._for, lineNum),
                        "while" => tokens.Add(new Token(TokenTypes._while, lineNum),
                        "func" => tokens.Add(new Token(TokenTypes._func, lineNum),
                        "return" => tokens.Add(new Token(TokenTypes._return, lineNum),
                        "int" => tokens.Add(new Token(TokenTypes._int, lineNum),
                        "bool" => tokens.Add(new Token(TokenTypes._bool, lineNum),
                        "char" => tokens.Add(new Token(TokenTypes._char, lineNum),
                        "void" => tokens.Add(new Token(TokenTypes._void, lineNum),
                        _ => tokens.Add(new Token(TokenTypes.identifier, buf, lineNum)
                        });
                        */
                }
                buf = "";

            } else if(Char.IsDigit((char)peek())) {
                while(Char.IsDigit((char)peek())) {
                    buf += consume();
                }
                tokens.Add(new Token(TokenTypes.intLit, buf, lineNum));
                buf = "";
            } else if((char)peek() == ' ' || (char)peek() == '\n' || char.IsWhiteSpace((char)peek())) {
                consume();
                lineNum++;
            } else {
                switch(consume()) {
                    case ';':
                        tokens.Add(new Token(TokenTypes.semi, lineNum));
                        break;
                    case '(':
                        tokens.Add(new Token(TokenTypes.openParen, lineNum));
                        break;
                    case ')':
                        tokens.Add(new Token(TokenTypes.closeParen, lineNum));
                        break;
                    case '{':
                        tokens.Add(new Token(TokenTypes.openCurley, lineNum));
                        break;
                    case '}':
                        tokens.Add(new Token(TokenTypes.closeCurley, lineNum));
                        break;
                    case '[':
                        tokens.Add(new Token(TokenTypes.openSquare, lineNum));
                        break;
                    case ']':
                        tokens.Add(new Token(TokenTypes.closeSquare, lineNum));
                        break;
                    case ',':
                        tokens.Add(new Token(TokenTypes.comma, lineNum));
                        break;
                    case '=':
                        if(peek() == '=') {
                            tokens.Add(new Token(TokenTypes.eqTo, lineNum));
                            consume();
                        } else {
                            tokens.Add(new Token(TokenTypes.eq, lineNum));
                        }
                        break;
                    case '>':
                        if(peek() == '=') {
                            tokens.Add(new Token(TokenTypes.greaterThanEq, lineNum));
                            consume();
                        } else {
                            tokens.Add(new Token(TokenTypes.greaterThan, lineNum));
                        }
                        break;
                    case '<':
                        if(peek() == '=') {
                            tokens.Add(new Token(TokenTypes.lessThanEq, lineNum));
                            consume();
                        } else {
                            tokens.Add(new Token(TokenTypes.lessThan, lineNum));
                        }
                        break;
                    case '!':
                        if(peek() == '=') {
                            tokens.Add(new Token(TokenTypes.notEqTo, lineNum));
                            consume();
                        } else {
                            tokens.Add(new Token(TokenTypes.not, lineNum));
                        }
                        break;
                    case '&':
                        tokens.Add(new Token(TokenTypes.and, lineNum));
                        break;
                    case '|':
                        tokens.Add(new Token(TokenTypes.or, lineNum));
                        break;
                    case '+':
                        if(peek() == '+') {
                            consume();
                            tokens.Add(new Token(TokenTypes.increment, lineNum));
                        } else {
                            tokens.Add(new Token(TokenTypes.add, lineNum));
                        }
                        break;
                    case '-':
                        if(peek() == '-') {
                            consume();
                            tokens.Add(new Token(TokenTypes.decrement, lineNum));
                        } else {
                            tokens.Add(new Token(TokenTypes.sub, lineNum));
                        }
                        break;
                    case '*':
                        tokens.Add(new Token(TokenTypes.mul, lineNum));
                        break;
                    case '/':
                        if(peek() == '/') {
                            while(peek() != ';') {
                                consume();
                            }
                            consume();
                        } else {
                            tokens.Add(new Token(TokenTypes.div, lineNum));
                        }
                        break;
                }
            }
        }
        if(!tokens.Contains(TokenTypes._exit)){
            tokens.Add(new Token(TokenTypes._exit, 0));
            tokens.Add(new Token(TokenTypes.openParen, 0));
            tokens.Add(new Token(TokenTypes.intLit, 192, 0));
            tokens.Add(new Token(TokenTypes.closeParen, 0));
            tokens.Add(new Token(TokenTypes.semi, 0));
        }
        return tokens;
    }
}
