#pragma warning disable 8618, 8629

namespace Lithium;

enum TokenTypes {
    //keywords
    _exit,
    _print,
    _if,
    _elif,
    _else,
    _for,
    _while,
    _return,
    //not keywords
    identifier,
    intLit,
    //types
    _int,
    _bool,
    _char,
    _void,
    //symbols
    openParen,
    closeParen,
    openCurley,
    closeCurley,
    openSquare,
    closeSquare,
    comma,
    quote,
    dQuote,
    semi,
    eq,
    add,
    sub,
    mul,
    div,
    greaterThan,
    lessThan,
    greaterThanEq,
    lessThanEq,
    eqTo,
    notEqTo,
    not,
    and,
    or,
    increment,
    decrement
}

struct Token(TokenTypes t, int l, string v = "") {
    public readonly TokenTypes type = t;
    public readonly string value = v;
    public readonly int lineNum = l;
}
