//full rework in progress

interface Statement{
    int lineNumber;
}

struct exitStatement : Statement {
    string expression;

    public exitStatement(string e, int l){
      exitCode = e;
      lineNumber = l;
    }
}

struct ifStatement : Statement {
    string condition, endLabel;
    bool hasChain;

    public ifStatement(string c, string e, bool h, int l){
      condition = c;
      endLabel = e;
      hasChain = h;
        lineNumber = l;
    }
}

struct variableDeclaration : Statement {
    string name, value;

    public variableDeclaration(string n, string v, int l){
      name = n;
      value = v;
      lineNumber = l;
    }
}

struct variableAssignment : Statement {
    string name, value;

    public variableDeclaration(string n, string v, int l){
      name = n;
      value = v;
      lineNumber = l
    }
}

struct forStatement : Statement {
    int count;

    public forStatement(int c, int l){
      count = c;
      lineNumber = l
    }
}

Queue<Statement> parsedCode;
