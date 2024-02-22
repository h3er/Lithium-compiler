#pragma warning disable 8618, 8629

using System.Text;

namespace Lithium;

class Tokenizer {
    private int index = -1;
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
                        tokens.Add(new Token(TokenTypes._exit));
                        break;
                    case "print":
                        tokens.Add(new Token(TokenTypes._print));
                        break;
                    case "if":
                        tokens.Add(new Token(TokenTypes._if));
                        break;
                    case "elif":
                        tokens.Add(new Token(TokenTypes._elif));
                        break;
                    case "else":
                        tokens.Add(new Token(TokenTypes._else));
                        break;
                    case "for":
                        tokens.Add(new Token(TokenTypes._for));
                        break;
                    case "while":
                        tokens.Add(new Token(TokenTypes._while));
                        break;
                    case "func":
                        tokens.Add(new Token(TokenTypes._func));
                        break;
                    case "return":
                        tokens.Add(new Token(TokenTypes._return));
                        break;
                    case "int":
                        tokens.Add(new Token(TokenTypes._int));
                        break;
                    case "bool":
                        tokens.Add(new Token(TokenTypes._bool));
                        break;
                    case "char":
                        tokens.Add(new Token(TokenTypes._char));
                        break;
                    case "void":
                        tokens.Add(new Token(TokenTypes._void));
                        break;
                    default:
                        tokens.Add(new Token(TokenTypes.identifier, buf));
                        break;

                        /*
                        tokens.Add( buf switch {
                        "exit" => new Token(TokenTypes._exit),
                        "print" => new Token(TokenTypes._print),
                        "if" => new Token(TokenTypes._if),
                        "elif" => tokens.Add(new Token(TokenTypes._elif),
                        "else" => tokens.Add(new Token(TokenTypes._else),
                        "for" => tokens.Add(new Token(TokenTypes._for),
                        "while" => tokens.Add(new Token(TokenTypes._while),
                        "func" => tokens.Add(new Token(TokenTypes._func),
                        "return" => tokens.Add(new Token(TokenTypes._return),
                        "int" => tokens.Add(new Token(TokenTypes._int),
                        "bool" => tokens.Add(new Token(TokenTypes._bool),
                        "char" => tokens.Add(new Token(TokenTypes._char),
                        "void" => tokens.Add(new Token(TokenTypes._void),
                        _ => tokens.Add(new Token(TokenTypes.identifier, buf)
                        });
                        */
                }
                buf = "";

            } else if(Char.IsDigit((char)peek())) {
                while(Char.IsDigit((char)peek())) {
                    buf += consume();
                }
                tokens.Add(new Token(TokenTypes.intLit, buf));
                buf = "";
            } else if((char)peek() == ' ' || (char)peek() == '\n' || char.IsWhiteSpace((char)peek())) {
                consume();
            } else {
                switch(consume()) {
                    case ';':
                        tokens.Add(new Token(TokenTypes.semi));
                        break;
                    case '(':
                        tokens.Add(new Token(TokenTypes.openParen));
                        break;
                    case ')':
                        tokens.Add(new Token(TokenTypes.closeParen));
                        break;
                    case '{':
                        tokens.Add(new Token(TokenTypes.openCurley));
                        break;
                    case '}':
                        tokens.Add(new Token(TokenTypes.closeCurley));
                        break;
                    case '[':
                        tokens.Add(new Token(TokenTypes.openSquare));
                        break;
                    case ']':
                        tokens.Add(new Token(TokenTypes.closeSquare));
                        break;
                    case ',':
                        tokens.Add(new Token(TokenTypes.comma));
                        break;
                    case '=':
                        if(peek() == '=') {
                            tokens.Add(new Token(TokenTypes.eqTo));
                            consume();
                        } else {
                            tokens.Add(new Token(TokenTypes.eq));
                        }
                        break;
                    case '>':
                        if(peek() == '=') {
                            tokens.Add(new Token(TokenTypes.greaterThanEq));
                            consume();
                        } else {
                            tokens.Add(new Token(TokenTypes.greaterThan));
                        }
                        break;
                    case '<':
                        if(peek() == '=') {
                            tokens.Add(new Token(TokenTypes.lessThanEq));
                            consume();
                        } else {
                            tokens.Add(new Token(TokenTypes.lessThan));
                        }
                        break;
                    case '!':
                        if(peek() == '=') {
                            tokens.Add(new Token(TokenTypes.notEqTo));
                            consume();
                        } else {
                            tokens.Add(new Token(TokenTypes.not));
                        }
                        break;
                    case '&':
                        tokens.Add(new Token(TokenTypes.and));
                        break;
                    case '|':
                        tokens.Add(new Token(TokenTypes.or));
                        break;
                    case '+':
                        if(peek() == '+') {
                            consume();
                            tokens.Add(new Token(TokenTypes.increment));
                        } else {
                            tokens.Add(new Token(TokenTypes.add));
                        }
                        break;
                    case '-':
                        if(peek() == '-') {
                            consume();
                            tokens.Add(new Token(TokenTypes.decrement));
                        } else {
                            tokens.Add(new Token(TokenTypes.sub));
                        }
                        break;
                    case '*':
                        tokens.Add(new Token(TokenTypes.mul));
                        break;
                    case '/':
                        if(peek() == '/') {
                            while(peek() != ';') {
                                consume();
                            }
                            consume();
                        } else {
                            tokens.Add(new Token(TokenTypes.div));
                        }
                        break;
                }
            }
        }
        if(!tokens.Contains(TokenTypes._exit)){
            tokens.Add(new Token(TokenTypes._exit));
            tokens.Add(new Token(TokenTypes.openParen));
            tokens.Add(new Token(TokenTypes.intLit, 192));
            tokens.Add(new Token(TokenTypes.closeParen));
            tokens.Add(new Token(TokenTypes.semi));
        }
        return tokens;
    }
}
