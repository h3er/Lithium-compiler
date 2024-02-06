// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
#pragma warning disable 8618, 8629

namespace Lithium;

class Generator {
    private int stackSize, labelCount, index = -1, funcOffset = -1;
    private string jumpCmd = "jz", endLabel;
    private readonly List<int> scopes = [];
    private readonly List<string> identifiers = [];
    private readonly Dictionary<string, List<string>> functions = new Dictionary<string, List<string>>();
    public List<Token> tokens = [];
    public string asm = "global _start\n_start:\n";

    private Token? peek(int n = 1) {
        if(index + n < tokens.Count) {
            return tokens[index + n];
        } else {
            return null;
        }
    }

    private Token consume(TokenTypes? check = null) {
        if (peek().Value.type != check && check != null) {
            throw new Exception("Expected: " + check);
        }
        index++;
        return tokens[index];
    }

    private int? binPrec(TokenTypes type) {
        switch(type) {
            case TokenTypes.sub:
            case TokenTypes.add:
                return 1;
            case TokenTypes.div:
            case TokenTypes.mul:
                return 2;
            default:
                return null;
        }
    }

    private bool isCompOp(TokenTypes type) {
        switch(type) {
            case TokenTypes.eqTo:
            case TokenTypes.notEqTo:
            case TokenTypes.greaterThan:
            case TokenTypes.greaterThanEq:
            case TokenTypes.lessThan:
            case TokenTypes.lessThanEq:
                return true;
        }
        return false;
    }

    private void evalCompOps()
    {
        jumpCmd = consume().type switch
        {
            TokenTypes.eqTo => "jne",
            TokenTypes.notEqTo => "je",
            TokenTypes.greaterThan => "jge",
            TokenTypes.greaterThanEq => "jg",
            TokenTypes.lessThan => "jbe",
            TokenTypes.lessThanEq => "jb",
            _ => throw new Exception("what??? how?????")
        };
    }
    
    private string evalSmallExpr() {
        if(peek().Value.value == "openParen") {
            consume(TokenTypes.openParen);
            string val = evalExpr();
            consume();
            return val;
        } else if(peek().Value.type == TokenTypes.intLit) {
            return consume(TokenTypes.intLit).value;
        }
        Console.WriteLine("stackSize: " + stackSize);
        Console.WriteLine("identifiers.Count: " + identifiers.Count);
        Console.WriteLine("identifiers.index: " + identifiers.IndexOf(peek().Value.value));
        Console.WriteLine("funcOffset: " + funcOffset);
        identifiers.ForEach(Console.WriteLine);
        return "QWORD [rsp + " + ((stackSize - identifiers.IndexOf(consume().value) + funcOffset) * 8) + "] ;; 7";
    }

    private string evalExpr(int min_prec = 1) {
        string lhs = evalSmallExpr();
        push(lhs);
        if(isCompOp(peek().Value.type)) {
            evalCompOps();
            evalExpr();
        }
        while(peek().Value.type != TokenTypes.semi && binPrec(peek().Value.type).HasValue && binPrec(peek().Value.type) >= min_prec) {
            TokenTypes op = peek().Value.type;

            evalExpr(binPrec(consume().type).Value + 1);

            pop("rdi");
            pop("rax");
            switch(op) {
                case TokenTypes.add:
                    appendASM("    add rax, rdi");
                    break;
                case TokenTypes.sub:
                    appendASM("    sub rax, rdi");
                    break;
                case TokenTypes.mul:
                    appendASM("    mul rdi");
                    break;
                case TokenTypes.div:
                    appendASM("    div rdi");
                    break;
            }
            push("rax");
        }
        return lhs;
    }

    private void appendASM(string s) {
        asm += s + "\n";
    }

    private void pop(string s) {
        asm += "    pop " + s + "\n";
        stackSize--;
    }

    private void push(string s) {
        asm += "    push " + s + "\n";
        stackSize++;
    }

    private void handleScope() {
        if(consume().type == TokenTypes.openCurley) {
            scopes.Add(stackSize);
        } else {
            appendASM("    add rsp, " + Convert.ToString((stackSize - scopes.Last()) * 8));
            identifiers.RemoveRange(scopes.Last(), identifiers.Count - scopes.Last());
            stackSize = scopes.Last();
            scopes.RemoveAt(scopes.Count - 1);
        }
    }

    private string createLabel() {
        return "l" + labelCount++;
    }

    public void generateCode(TokenTypes? condition = null) {
        while(peek() != null && peek().Value.type != condition) {
            switch(peek().Value.type) {
                case TokenTypes._exit:
                    consume(TokenTypes._exit);
                    consume(TokenTypes.openParen);
                    evalExpr();
                    consume(TokenTypes.closeParen);
                    consume(TokenTypes.semi);
                    pop("rdi");
                    appendASM("    mov rax, 60");
                    appendASM("    syscall");
                    break;
                case TokenTypes._if:
                    int oldStackSize = stackSize;
                    string label = createLabel();
                    consume(TokenTypes._if);
                    consume(TokenTypes.openParen);
                    evalExpr();
                    consume(TokenTypes.closeParen);
                    pop("rdi");
                    if(oldStackSize == stackSize) {
                        appendASM("    test rdi, rdi");
                        appendASM("    jz " + label);
                    } else {
                        pop("rax");
                        appendASM("    cmp rdi, rax");
                        appendASM("    " + jumpCmd + " " + label);
                    }
                    generateCode(TokenTypes.closeCurley);
                    handleScope();
                    if(peek().Value.type == TokenTypes._elif || peek().Value.type == TokenTypes._else) {
                        endLabel = createLabel();
                        appendASM("    jmp " + endLabel);
                        appendASM(label + ":");
                        generateCode(TokenTypes.closeCurley);
                    } else {
                        appendASM(label + ":");
                    }
                    break;
                case TokenTypes._elif:
                    oldStackSize = stackSize;
                    label = createLabel();
                    consume(TokenTypes._elif);
                    consume(TokenTypes.openParen);
                    evalExpr();
                    consume(TokenTypes.closeParen);
                    pop("rdi");
                    if(oldStackSize == stackSize) {
                        appendASM("    test rdi, rdi");
                        appendASM("    jz " + label);
                    } else {
                        pop("rax");
                        appendASM("    cmp rdi, rax");
                        appendASM("    " + jumpCmd + " " + label);
                    }
                    generateCode(TokenTypes.closeCurley);
                    handleScope();
                    appendASM("    jmp " + endLabel);
                    if(peek().Value.type == TokenTypes._elif || peek().Value.type == TokenTypes._else) {
                        appendASM(label + ":");
                        generateCode(TokenTypes.closeCurley);
                    } else {
                        appendASM(label + ":");
                        appendASM("    jmp " + endLabel);
                        appendASM(endLabel + ":");
                    }
                    break;
                case TokenTypes._else:
                    consume(TokenTypes._else);
                    generateCode(TokenTypes.closeCurley);
                    appendASM("    jmp " + endLabel);
                    appendASM(endLabel + ":");
                    break;
                case TokenTypes._for:
                    label = createLabel();
                    string otherLabel = createLabel();
                    consume(TokenTypes._for);
                    consume(TokenTypes.openParen);
                    evalExpr();
                    consume(TokenTypes.closeParen);
                    pop("rbx");
                    appendASM(label + ":");
                    generateCode(TokenTypes.closeCurley);
                    appendASM("    sub rbx, 1");
                    appendASM("    test rbx, rbx");
                    appendASM("    jz " + otherLabel);
                    appendASM("    jmp " + label);
                    appendASM(otherLabel + ":");
                    break;
                case TokenTypes._while:
                    label = createLabel();
                    otherLabel = createLabel();
                    oldStackSize = stackSize;
                    appendASM(label + ":");
                    consume(TokenTypes._while);
                    consume(TokenTypes.openParen);
                    evalExpr();
                    consume(TokenTypes.closeParen);
                    pop("rdi");
                    if(oldStackSize == stackSize) {
                        appendASM("    test rdi, rdi");
                        appendASM("    jz " + otherLabel);
                    } else {
                        pop("rax");
                        appendASM("    cmp rdi, rax");
                        appendASM("    " + jumpCmd + " " + otherLabel);
                    }
                    generateCode(TokenTypes.closeCurley);
                    appendASM("    jmp " + label);
                    handleScope();
                    appendASM(otherLabel + ":");
                    break;
                case TokenTypes._func:
                    consume(TokenTypes._func);
                    label = createLabel();
                    otherLabel = createLabel();
                    string name = consume(TokenTypes.identifier).value;
                    functions.Add(name, [label]);
                    while(peek().Value.type != TokenTypes.closeParen) {
                        if(peek().Value.type == TokenTypes.identifier) {
                            identifiers.Add(peek().Value.value);
                            functions[name].Add(consume(TokenTypes.identifier).value);
                            push("193");
                        } else {
                            consume(TokenTypes.comma);
                        }
                    }
                    consume(TokenTypes.closeParen);
                    handleScope();
                    identifiers.Add("peenisnsi");
                    appendASM("    jmp " + otherLabel);
                    appendASM(label + ":");
                    funcOffset++;
                    generateCode(TokenTypes.closeCurley);
                    if (peek().Value.type == TokenTypes.closeCurley)
                    {
                        handleScope();   
                    }
                    appendASM(otherLabel + ":");
                    funcOffset--;
                    break;
                case TokenTypes._return:
                    consume(TokenTypes._return);
                    if(peek().Value.type == TokenTypes.semi) {
                        consume(TokenTypes.semi);
                        handleScope();
                        appendASM("    ret");
                        return;
                    } else {
                        //check return type, if it returns create extra space at the top of the stack for value
                        throw new Exception("Not implemented");
                    }
                case TokenTypes._int:
                    consume(TokenTypes._int);
                    identifiers.Add(consume(TokenTypes.identifier).value);
                    consume(TokenTypes.eq);
                    evalExpr();
                    consume(TokenTypes.semi);
                    break;
                case TokenTypes.identifier:
                    string l = consume(TokenTypes.identifier).value;
                    if(identifiers.IndexOf(l) != -1) {
                        int loc = identifiers.IndexOf(l);
                        if(peek().Value.type == TokenTypes.increment || peek().Value.type == TokenTypes.decrement) {
                            appendASM("    mov rdi, QWORD [rsp + " + (identifiers.Count - loc + funcOffset) * 8 + "] ;; 6");
                            appendASM(consume().type == TokenTypes.increment ? "    inc rdi" : "    dec rdi");
                            appendASM("    mov [rsp + " + (identifiers.Count - loc + funcOffset) * 8 + "], rdi ;; 5");
                        } else {
                            consume(TokenTypes.eq);
                            evalExpr();
                            pop("rdi");
                            appendASM("    mov [rsp + " + (identifiers.Count - loc + funcOffset) * 8 + "], rdi ;; 4");
                        }
                        consume(TokenTypes.semi);
                    } else {
                        int paramCount = 1;
                        consume(TokenTypes.openParen);
                        while(peek().Value.type != TokenTypes.closeParen) {
                            if(peek().Value.type == TokenTypes.intLit) {
                                appendASM("    mov rdi, " + consume(TokenTypes.intLit).value + " ;; 3");
                            } else {
                                appendASM("    mov rdi, QWORD [rsp + " + (identifiers.Count - identifiers.IndexOf(consume(TokenTypes.identifier).value) + funcOffset) * 8 + "] ;; 2");
                            }
                            Console.WriteLine("stackSize: " + stackSize);
                            Console.WriteLine("identifiers.Count: " + identifiers.Count);
                            Console.WriteLine("functions[l][paramCount]: " + functions[l][paramCount]);
                            Console.WriteLine("identifiers.index: " + identifiers.IndexOf(peek().Value.value));
                            Console.WriteLine("identifiers.index: " + identifiers.IndexOf(functions[l][paramCount]));
                            Console.WriteLine("funcOffset: " + funcOffset);
                            identifiers.ForEach(Console.WriteLine);
                            appendASM("    mov [rsp + " + (stackSize - identifiers.IndexOf(functions[l][paramCount]) + funcOffset) * 8 + "], rdi ;; 1");
                            paramCount++;
                            if(peek().Value.type == TokenTypes.comma) {
                                consume(TokenTypes.comma);
                            }
                        }
                        appendASM("    call " + functions[l][0]);
                        appendASM("    add rsp, " + (paramCount - 1) * 8);
                        stackSize -= (paramCount - 1);
                        consume(TokenTypes.closeParen);
                        consume(TokenTypes.semi);
                    }
                    break;
                case TokenTypes.openCurley:
                case TokenTypes.closeCurley:
                    handleScope();
                    break;
            }

        }
    }
}

/*
Console.WriteLine("stackSize: " + stackSize);
Console.WriteLine("identifiers.Count: " + identifiers.Count);
    Console.WriteLine("functions[l][paramCount]: " + functions[l][paramCount]);
    Console.WriteLine("identifiers.index: " + identifiers.IndexOf(peek().Value.value));
Console.WriteLine("identifiers.index: " + identifiers.IndexOf(functions[l][paramCount]));
Console.WriteLine("funcOffset: " + funcOffset);
identifiers.ForEach(Console.WriteLine);
*/