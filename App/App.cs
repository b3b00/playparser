
using System.Text;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;


public static class E
{
	public static IEnumerable<string> GetLines(this string s)
	{
		StringReader sr = new StringReader(s);
		string line;
		while ((line = sr.ReadLine()) != null)
			yield return line;
	}

	public static string Tab(this string s)
	{
		var lines = s.GetLines();
		var builder = new StringBuilder();
		foreach (var line in lines)
			builder.AppendLine("\t"+line);
		return builder.ToString();
	}
}

[Lexer(IndentationAWare = true)]
    public enum L
    {
        [Keyword("if")]
        IF,
        [Keyword("else")]
        ELSE,
        [AlphaId]
        ID,
        [Int]
        INT,
        [Sugar("==")]
        EQ,
        [Sugar("=")]
        SET,
    }

[ParserRoot("root")]
[AutoCloseIndentations]
    public class P
    {
        [Production("root : instr +")]
        public string Root(List<string> instructions)
        {
            return string.Join(Environment.NewLine, instructions);
        }

        [Production("instr : ID SET[d] INT")]
        public string Assign(Token<L> id, Token<L> value)
        {
	        return $"{id.Value} <- {value.Value}";
        }

        [Production("instr : IF[d] ID EQ[d] INT blk else?")]
        public string IfThenElse(Token<L> id, Token<L> value, string thenBlock, ValueOption<string> elseBlock)
        {
	        return $@"if ({id.Value} == {value.Value}) {{
{thenBlock.Tab()}}} {elseBlock.Match(x => x, () => "")}";
        }

        [Production("else : ELSE[d] blk")]
        public string Else(string block)
        {
	        return $@"else {{
{block.Tab()}}}";
        }

        [Production("blk : INDENT[d] instr + UINDENT[d]")]
        public string Block(List<string> instructions)
        {
            return string.Join(Environment.NewLine, instructions);
        }
    }


public class App
{

	public static void Main(string[] args)
	{




		var source = @"
if a == 1
	a = 2
else
	a = 3
	if b == 12
		c = 78
	else 
		c = 552";
		
            
		var instance = new P();
		ParserBuilder<L,string> parserBuilder = new ParserBuilder<L,string>();
		var parserBuild = parserBuilder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
		if (parserBuild.IsError)
		{
			Console.WriteLine("Parser build error");
			foreach (var error in parserBuild.Errors)
			{
				Console.WriteLine(error);
			}
			return;
		}

		var parser = parserBuild.Result;


		

		var parsed = parser.Parse(source);
		if (parsed.IsError)
		{
			Console.WriteLine("parse error");
			foreach (var error in parsed.Errors)
			{
				Console.WriteLine(error);
			}
			return;
		}
Console.WriteLine("========================== PARSE SUCCESS ==========================");
		Console.WriteLine(parsed.Result);
Console.WriteLine("===================================================================");
	}
}

