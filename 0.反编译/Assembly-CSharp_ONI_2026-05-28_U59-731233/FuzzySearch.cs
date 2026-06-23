using System;
using System.Collections.Generic;
using FuzzySharp;
using STRINGS;

public class FuzzySearch
{
	[Flags]
	public enum Features
	{
		Suppress1And2LetterWords = 1,
		SuppressMeaninglessWords = 2,
		Initialism = 4
	}

	public struct Match
	{
		public int score;

		public string token;

		public static readonly Match NONE = new Match
		{
			score = 0,
			token = string.Empty
		};
	}

	public const Features PHRASE_MUTATION_FEATURES = Features.Suppress1And2LetterWords | Features.SuppressMeaninglessWords;

	public static readonly char[] TOKEN_SEPARATORS = new char[15]
	{
		' ', '.', '\n', ',', ';', ':', '?', '!', '-', '(',
		')', '[', ']', '{', '}'
	};

	public static Features GetFeatures()
	{
		Features features = Features.Initialism;
		Localization.Locale locale = Localization.GetLocale();
		if (locale == null)
		{
			features |= Features.Suppress1And2LetterWords;
			features |= Features.SuppressMeaninglessWords;
		}
		return features;
	}

	public static string Canonicalize(string s)
	{
		return UI.StripLinkFormatting(UI.StripStyleFormatting(s));
	}

	private static int ScoreImpl_Unchecked(string searchString, string candidate)
	{
		return Fuzz.Ratio(searchString, candidate);
	}

	private static int ScoreImpl(string searchString, string candidate)
	{
		return ScoreImpl_Unchecked(searchString, candidate);
	}

	private static bool IsUpper(string s)
	{
		foreach (char c in s)
		{
			if (char.IsLetter(c) && !char.IsUpper(c))
			{
				return false;
			}
		}
		return true;
	}

	private static Match ScoreTokens_Unchecked(string searchStringUpper, string[] tokens)
	{
		if (tokens.Length == 0)
		{
			return Match.NONE;
		}
		int? num = null;
		string token = null;
		foreach (string text in tokens)
		{
			int num2 = ScoreImpl_Unchecked(searchStringUpper, text);
			if (!num.HasValue || num2 > num)
			{
				num = num2;
				token = text;
			}
		}
		return new Match
		{
			score = num.Value,
			token = token
		};
	}

	private static Match ScoreTokens_Unchecked(string searchStringUpper, IReadOnlyList<string> tokens)
	{
		if (tokens.Count == 0)
		{
			return Match.NONE;
		}
		int? num = null;
		string token = null;
		foreach (string token2 in tokens)
		{
			int num2 = ScoreImpl_Unchecked(searchStringUpper, token2);
			if (!num.HasValue || num2 > num)
			{
				num = num2;
				token = token2;
			}
		}
		return new Match
		{
			score = num.Value,
			token = token
		};
	}

	public static Match ScoreTokens(string searchStringUpper, string[] tokens)
	{
		return ScoreTokens_Unchecked(searchStringUpper, tokens);
	}

	public static Match ScoreTokens(string searchStringUpper, IReadOnlyList<string> tokens)
	{
		return ScoreTokens_Unchecked(searchStringUpper, tokens);
	}

	public static Match ScoreCanonicalCandidate(string searchStringUpper, string canonicalCandidate, string candidate = null)
	{
		Match match = new Match
		{
			score = Fuzz.WeightedRatio(searchStringUpper, canonicalCandidate),
			token = (candidate ?? canonicalCandidate)
		};
		if ((GetFeatures() & Features.Initialism) != 0)
		{
			int num = Fuzz.TokenInitialismRatio(searchStringUpper, canonicalCandidate);
			if (num > match.score)
			{
				match.score = num;
			}
		}
		string[] tokens = canonicalCandidate.Split(TOKEN_SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
		Match match2 = ScoreTokens_Unchecked(searchStringUpper, tokens);
		return (match2.score > match.score) ? match2 : match;
	}

	public static Match CanonicalizeAndScore(string searchStringUpper, string candidate)
	{
		return ScoreCanonicalCandidate(searchStringUpper, Canonicalize(candidate).ToUpper(), candidate);
	}
}
