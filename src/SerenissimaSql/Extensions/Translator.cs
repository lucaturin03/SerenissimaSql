using SerenissimaSql.Entities;
using SerenissimaSql.Exceptions;

namespace SerenissimaSql.Extensions;

public static class Translator
{
    public static string Translate(this string venetoQuery)
    {
        if (string.IsNullOrWhiteSpace(venetoQuery))
        {
            throw new NoVaUnOstregaException("Ma cossa me scrivito? Ea query xe voda!");
        }

        var result = venetoQuery.Tokenize().ComposeQuery();
        var firstWord = result.Split([' '], 2)[0];
        if (!Constants.ValidStarts.Contains(firstWord))
        {
            throw new NoVaUnOstregaException(
                $"Sta roba no la xe na query bona: la scominsia co '{firstWord}'. Te ga da partir co ciapa, sbati rento, rangia, buta via o na transazion.");
        }

        return result;
    }

    extension(List<Token> tokens)
    {
        private string ComposeQuery()
        {
            var output = new List<string>();

            var i = 0;
            while (i < tokens.Count)
            {
                if (tokens[i].IsLiteral)
                {
                    output.Add(tokens[i].Text);
                    i++;
                    continue;
                }

                var (keyword, len) = tokens.MatchKeyword(i);
                if (len > 0)
                {
                    output.Add(keyword);
                    i += len;
                }
                else
                {
                    output.Add(tokens[i].Text);
                    i++;
                }
            }

            return string.Join(" ", output).Trim();
        }

        private (string Sql, int Length) MatchKeyword(int start)
        {
            for (var len = Math.Min(Constants.MaxPhraseWords, tokens.Count - start); len >= 1; len--)
            {
                var window = tokens.GetRange(start, len);
                if (window.Any(t => t.IsLiteral))
                {
                    continue;
                }

                var phrase = string.Join(" ", window.Select(t => t.Text));
                if (Constants.Keywords.TryGetValue(phrase, out var sql))
                {
                    return (sql, len);
                }
            }

            return (string.Empty, 0);
        }
    }
}