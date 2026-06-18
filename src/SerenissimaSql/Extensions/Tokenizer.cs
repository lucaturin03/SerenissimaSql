using System.Text;
using SerenissimaSql.Entities;

namespace SerenissimaSql.Extensions;

public static class Tokenizer
{
    public static List<Token> Tokenize(this string input)
    {
        var tokens = new List<Token>();
        var i = 0;

        while (i < input.Length)
        {
            if (char.IsWhiteSpace(input[i]))
            {
                i++;
                continue;
            }

            var token = input[i] is '\'' or '"'
                ? ReadStringLiteral(input, ref i)
                : ReadWord(input, ref i);

            tokens.Add(token);
        }

        return tokens;
    }

    private static Token ReadStringLiteral(string input, ref int i)
    {
        var quote = input[i];
        var sb = new StringBuilder();
        sb.Append(quote);
        i++;

        while (i < input.Length && input[i] != quote)
        {
            sb.Append(input[i]);
            i++;
        }
        
        if (i >= input.Length)
        {
            return new Token(sb.ToString(), true);
        }
        
        sb.Append(quote);
        i++;

        return new Token(sb.ToString(), true);
    }

    private static Token ReadWord(string input, ref int i)
    {
        var sb = new StringBuilder();
        while (i < input.Length && !char.IsWhiteSpace(input[i]))
        {
            sb.Append(input[i]);
            i++;
        }

        return new Token(sb.ToString(), false);
    }
}