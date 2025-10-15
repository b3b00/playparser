#:package sly@3.7.6

using sly.lexer;
using sly.lexer.fsm;
using sly.parser.generator;

public enum PostfixHexa
{
    EOS,

    [Extension] Hexa,

    [Int] Int,

    [AlphaId] Id
}

public class Parser
{
	[Production("main : item+ ")]
	public string Main(List<string> items)
	{
		return string.Join("\n", items);
	}

	[Production("item : [Id | Hexa | Int]" )]
	public string Item(Token<PostfixHexa> token)
	{
		return token.ToString();
	}
	
}


public class App
{

	public static void Main(string[] args)
	{
		Action<PostfixHexa, LexemeAttribute, GenericLexer<PostfixHexa>> extensionBuilder = (token, attribute, lexer) =>
										{
											if (token == PostfixHexa.Hexa)
											{

												bool CheckHexa(ReadOnlyMemory<char> value)
												{
													char[] hexaLetters = new char[] { 'a', 'b', 'c', 'd', 'e', 'f' };
													var ok = false;
													return value.ToArray().All(x => Char.IsDigit(x) || hexaLetters.Contains(x));
												}

												NodeCallback<GenericToken> callback = (FSMMatch<GenericToken> match) =>
														{
															// this store the token id the the FSMMatch object to be later returned by GenericLexer.Tokenize 
															match.Properties[GenericLexer<PostfixHexa>.DerivedToken] = PostfixHexa.Hexa;
															return match;
														};
												var builder = lexer.FSMBuilder;
												builder.GoTo(GenericLexer<PostfixHexa>.start)
																		.MultiRangeTransition([('0', '9'), ('a', 'f'), ('A', 'Z')])
																		.Mark("in_hexa")
																		.MultiRangeTransitionTo("in_hexa", [('0', '9'), ('a', 'f'), ('A', 'Z')])
																		.Transition('h')
																		.Mark("hexa_done")
																		.End(GenericToken.Extension, false)
																		.CallBack(callback);
												builder.GoTo(GenericLexer<PostfixHexa>.in_int)
																		.MultiRangeTransition([('0', '9'), ('a', 'f'), ('A', 'Z')])
																		.Mark("in_hexa_after_int")
																		.MultiRangeTransitionTo("in_hexa_after_int", [('0', '9'), ('a', 'f'), ('A', 'Z')])
																		.Transition('h')
																		.End(GenericToken.Extension, false)
																		.CallBack(callback);
											}
										};


		// this post processor retag identifier tokens ending with 'h' and only containing hex digits as Hexa tokens
		LexerPostProcess<PostfixHexa> postProcess = (tokens) =>
								{
									return tokens.Select(x =>
											{
												if (x.TokenID == PostfixHexa.Id && x.Value.EndsWith("h"))
												{
													x.TokenID = PostfixHexa.Hexa;
												}
												return x;
											}).ToList();
								};


var source = "01fh 42 identifier abc abch";

            var lexerBuild = LexerBuilder.BuildLexer<PostfixHexa>(lexerPostProcess:postProcess,extensionBuilder:extensionBuilder, lang:"en");
            if (lexerBuild.IsError)
						{
								Console.WriteLine("Lexer build error");
								foreach (var error in lexerBuild.Errors)
								{
										Console.WriteLine(error);
								}
								return;
						}
            var lexer = lexerBuild.Result;
		var lexed = lexer.Tokenize(source, true);
		if (lexed.IsError)
		{
			Console.WriteLine("lex error");
			Console.WriteLine(lexed.Error);
			return;
		}
		Console.WriteLine("========================== LEXER SUCCESS ==========================");
		foreach (var token in lexed.Tokens.MainTokens())
		{
			Console.WriteLine(token);
		}
				Console.WriteLine("===================================================================");

		var instance = new Parser();
		ParserBuilder<PostfixHexa,string> parserBuilder = new ParserBuilder<PostfixHexa,string>();
		var parserBuild = parserBuilder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "main",extensionBuilder: extensionBuilder, lexerPostProcess: postProcess);
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
		Console.WriteLine($"parsed: \n{parsed.Result}");
Console.WriteLine("===================================================================");
	}
}

