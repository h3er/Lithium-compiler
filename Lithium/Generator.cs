// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
#pragma warning disable 8618, 8629
namespace Lithium;

class Generator {
    private int stackSize, labelCount, index = -1, funcOffset = -1;
    private string jumpCmd = "jz", endLabel;
    private List<int> scopes = [], identifierSize = [];
    private List<string> identifiers = [];
    private Dictionary<string, List<string>> functions = new Dictionary<string, List<string>>();
    public List<Token> tokens = [];
    public string asm = "global _start\n_start:\n";

    private Token? peek(int n = 1) {
        if(index + n < tokens.Count) {
            return tokens[index + n];
        } else {
            return null;
        }
    }

    private Token consume(int n = 1) {
        index += n;
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

    private void evalCompOps(){
        switch(consume().type) {
            case TokenTypes.eqTo:
                jumpCmd = "jne";
                break;
            case TokenTypes.notEqTo:
                jumpCmd = "je";
                break;
            case TokenTypes.greaterThan:
                jumpCmd = "jge";
                break;
            case TokenTypes.greaterThanEq:
                jumpCmd = "jg";
                break;
            case TokenTypes.lessThan:
                jumpCmd = "jbe";
                break;
            case TokenTypes.lessThanEq:
                jumpCmd = "jb";
                break;
        }
    }
    
    private string evalSmallExpr() {
        if(peek().Value.value == "openParen") {
            consume();
            string val = evalExpr();
            consume();
            return val;
        } else if(peek().Value.type == TokenTypes.intLit) {
            return consume().value;
        }
        Console.WriteLine("stackSize: " + stackSize);
        Console.WriteLine("identifiers.Count: " + identifiers.Count);
        Console.WriteLine("identifiers.index: " + identifiers.IndexOf(peek().Value.value));
        Console.WriteLine("funcOffset: " + funcOffset);
        identifiers.ForEach(Console.WriteLine);
        return "QWORD [rsp + " + ((stackSize - identifiers.IndexOf(consume().value)) * 8) + "] ;; 7";
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
                    consume(2);
                    evalExpr();
                    consume(2);
                    pop("rdi");
                    appendASM("    mov rax, 60");
                    appendASM("    syscall");
                    break;
                case TokenTypes._if:
                    int oldStackSize = stackSize;
                    string label = createLabel();
                    consume(2);
                    evalExpr();
                    consume();
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
                    consume(2);
                    evalExpr();
                    consume();
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
                    consume();
                    generateCode(TokenTypes.closeCurley);
                    appendASM("    jmp " + endLabel);
                    appendASM(endLabel + ":");
                    break;
                case TokenTypes._for:
                    label = createLabel();
                    string otherLabel = createLabel();
                    consume(2);
                    evalExpr();
                    consume();
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
                    consume(2);
                    evalExpr();
                    consume();
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
                    consume();
                    label = createLabel();
                    otherLabel = createLabel();
                    string name = consume().value;
                    functions.Add(name, [label]);
                    while(peek().Value.type != TokenTypes.closeParen) {
                        if(peek().Value.type == TokenTypes.identifier) {
                            identifiers.Add(peek().Value.value);
                            identifierSize.Add(1);
                            functions[name].Add(consume().value);
                            push("193");
                        } else {
                            consume();
                        }
                    }
                    consume();
                    handleScope();
                    identifiers.Add("peenisnsi");
                    appendASM("    jmp " + otherLabel);
                    appendASM(label + ":");
                    generateCode(TokenTypes.closeCurley);
                    handleScope();
                    appendASM(otherLabel + ":");
                    break;
                case TokenTypes._return:
                    consume();
                    if(peek().Value.type == TokenTypes.semi) {
                        consume();
                        handleScope();
                        appendASM("    ret");
                        return;
                    } else {
                        //check return type, if it returns create extra space at the top of the stack for value
                        Console.WriteLine("return value, not implemented");
                        evalExpr();
                    }
                    break;
                case TokenTypes._int:
                    consume();
                    if(peek().Value.type == TokenTypes.openSquare){
                        consume();
                        identifierSize.Add(Convert.ToInt32(peek().Value.value));
                        appendASM("    mov rdi, " + consume().value);
                        appendASM("    sal rdi, 3");
                        appendASM("    sub rsp, rdi");
                        consume();
                        identifiers.Add(consume().value);
                        consume(2);
                        while(peek().Value.type != TokenTypes.closeSquare){
                            if(peek().Value.type == TokenTypes.comma){
                                consume();
                            }else{
                                
                            }
                        }
                        consume(2);
                    } else{
                        identifiers.Add(consume().value);
                        identifierSize.Add(1);
                        consume();
                        evalExpr();
                        consume();
                    }
                    break;
                case TokenTypes.identifier:
                    string l = consume().value;
                    if(identifiers.IndexOf(l) != -1) {
                        int loc = identifiers.IndexOf(l);
                        if(peek().Value.type == TokenTypes.increment || peek().Value.type == TokenTypes.decrement) {
                            appendASM("    mov rdi, QWORD [rsp + " + (identifiers.Count - loc + funcOffset) * 8 + "] ;; 6");
                            appendASM(consume().type == TokenTypes.increment ? "    inc rdi" : "    dec rdi");
                            appendASM("    mov [rsp + " + (identifiers.Count - loc + funcOffset) * 8 + "], rdi ;; 5");
                        } else {
                            consume();
                            evalExpr();
                            pop("rdi");
                            appendASM("    mov [rsp + " + (identifiers.Count - loc + funcOffset) * 8 + "], rdi ;; 4");
                        }
                        consume();
                    } else {
                        int paramCount = 1;
                        consume();
                        while(peek().Value.type != TokenTypes.closeParen) {
                            if(peek().Value.type == TokenTypes.intLit) {
                                appendASM("    mov rdi, " + consume().value + " ;; 3");
                            } else {
                                appendASM("    mov rdi, QWORD [rsp + " + (identifiers.Count - identifiers.IndexOf(consume().value) + funcOffset) * 8 + "] ;; 2");
                            }
                            appendASM("    mov [rsp + " + (stackSize - identifiers.IndexOf(functions[l][paramCount]) + funcOffset) * 8 + "], rdi ;; 1");
                            paramCount++;
                            if(peek().Value.type == TokenTypes.comma) {
                                consume();
                            }
                        }
                        appendASM("    call " + functions[l][0]);
                        appendASM("    add rsp, " + (paramCount - 1) * 8);
                        stackSize -= (paramCount - 1);
                        consume(2);
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