namespace Lithium;

static class Compiler {
    static void Main(string[] args) {
        if (args.Length == 0){
            throw new Exception("No source file given");
        }
        Tokenizer tok = new Tokenizer(args[0]);
        Generator gen = new Generator{
            tokens = tok.tokenizeCode()
        };
        gen.generateCode();
        using StreamWriter sw = new StreamWriter("testCode.asm");
        sw.Write(gen.asm);
    }
}
