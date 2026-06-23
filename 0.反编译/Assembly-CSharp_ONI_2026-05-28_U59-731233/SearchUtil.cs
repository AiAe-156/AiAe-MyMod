using System;
using System.Collections.Generic;
using System.Text;
using Database;
using STRINGS;

public static class SearchUtil
{
	private interface IScore
	{
		int Score { get; }
	}

	private struct TieBreaker
	{
		private readonly int globalMax;

		private int globalMaxCmp;

		private int localMaxScore;

		private int localMaxCmp;

		public readonly bool IsTieBroken => globalMaxCmp != 0;

		public TieBreaker(int _globalMax)
		{
			globalMax = _globalMax;
			globalMaxCmp = 0;
			localMaxScore = -1;
			localMaxCmp = 0;
		}

		private int CacheLocalScore(int score, int cmp)
		{
			if (localMaxScore == -1 || localMaxScore < score)
			{
				localMaxScore = score;
				localMaxCmp = cmp;
			}
			return localMaxCmp;
		}

		private int CacheScore(int score, int cmp)
		{
			if (score == globalMax)
			{
				globalMaxCmp = cmp;
				return globalMaxCmp;
			}
			return CacheLocalScore(score, cmp);
		}

		public int Consider(int lhs, int rhs)
		{
			if (IsTieBroken)
			{
				return globalMaxCmp;
			}
			switch (-lhs.CompareTo(rhs))
			{
			case 0:
				return (localMaxScore != -1) ? localMaxCmp : 0;
			case -1:
				return CacheScore(lhs, -1);
			case 1:
				return CacheScore(rhs, 1);
			default:
				Debug.Assert(condition: false);
				return 0;
			}
		}

		public int Consider<T>(T lhs, T rhs) where T : IComparable, IScore
		{
			if (IsTieBroken)
			{
				return globalMaxCmp;
			}
			if (lhs == null)
			{
				if (rhs == null)
				{
					return (localMaxScore != -1) ? localMaxCmp : 0;
				}
				return CacheScore(rhs.Score, 1);
			}
			if (rhs == null)
			{
				return CacheScore(lhs.Score, -1);
			}
			object obj = rhs;
			switch (lhs.CompareTo(obj))
			{
			case 0:
				return (localMaxScore != -1) ? localMaxCmp : 0;
			case -1:
				return CacheScore(lhs.Score, -1);
			case 1:
				return CacheScore(rhs.Score, 1);
			default:
				Debug.Assert(condition: false);
				return 0;
			}
		}
	}

	public class MatchCache : IComparable, IScore
	{
		public string text;

		public int Score => FuzzyMatch.score;

		public FuzzySearch.Match FuzzyMatch { get; private set; }

		public bool IsPassingScore()
		{
			return Score >= 79;
		}

		public void Bind(string searchStringUpper)
		{
			try
			{
				FuzzyMatch = FuzzySearch.ScoreCanonicalCandidate(searchStringUpper, text);
			}
			catch (Exception innerException)
			{
				throw new Exception("searchStringUpper: " + searchStringUpper + ", text: " + text, innerException);
			}
		}

		public void Reset()
		{
			FuzzyMatch = FuzzySearch.Match.NONE;
		}

		public int CompareTo(object obj)
		{
			MatchCache matchCache = (MatchCache)obj;
			return -Score.CompareTo(matchCache.Score);
		}
	}

	public class NameDescCache : IComparable, IScore
	{
		public MatchCache name;

		public MatchCache desc;

		public int Score => Math.Max(name.Score, desc.Score);

		public void Bind(string searchStringUpper)
		{
			name.Bind(searchStringUpper);
			desc.Bind(searchStringUpper);
		}

		public void Reset()
		{
			name.Reset();
			desc.Reset();
		}

		public int CompareTo(object obj)
		{
			NameDescCache nameDescCache = (NameDescCache)obj;
			int score = Score;
			int score2 = nameDescCache.Score;
			int num = -score.CompareTo(score2);
			if (num != 0)
			{
				return num;
			}
			TieBreaker tieBreaker = new TieBreaker(score);
			tieBreaker.Consider(name, nameDescCache.name);
			return tieBreaker.Consider(desc, nameDescCache.desc);
		}
	}

	public class NameDescSearchTermsCache : IComparable, IScore
	{
		public NameDescCache nameDesc;

		public IReadOnlyList<string> searchTerms;

		public FuzzySearch.Match SearchTermsScore { get; private set; }

		public int Score => Math.Max(nameDesc.Score, SearchTermsScore.score);

		public void Bind(string searchStringUpper)
		{
			nameDesc.Bind(searchStringUpper);
			SearchTermsScore = FuzzySearch.ScoreTokens(searchStringUpper, searchTerms);
		}

		public void Reset()
		{
			nameDesc.Reset();
			SearchTermsScore = FuzzySearch.Match.NONE;
		}

		public bool IsPassingScore()
		{
			return Score >= 79;
		}

		public int CompareTo(object obj)
		{
			NameDescSearchTermsCache nameDescSearchTermsCache = (NameDescSearchTermsCache)obj;
			int score = Score;
			int score2 = nameDescSearchTermsCache.Score;
			int num = -score.CompareTo(score2);
			if (num != 0)
			{
				return num;
			}
			TieBreaker tieBreaker = new TieBreaker(score);
			tieBreaker.Consider(nameDesc.name, nameDescSearchTermsCache.nameDesc.name);
			tieBreaker.Consider(SearchTermsScore.score, nameDescSearchTermsCache.SearchTermsScore.score);
			return tieBreaker.Consider(nameDesc.desc, nameDescSearchTermsCache.nameDesc.desc);
		}
	}

	public class BuildingDefCache : IComparable, IScore
	{
		public NameDescSearchTermsCache nameDescSearchTerms;

		public MatchCache effect;

		public List<NameDescCache> recipes;

		public NameDescCache BestRecipe { get; private set; }

		public int Score => Math.Max(nameDescSearchTerms.Score, Math.Max(effect.Score, (BestRecipe != null) ? BestRecipe.Score : 0));

		public void Bind(string searchStringUpper)
		{
			nameDescSearchTerms.Bind(searchStringUpper);
			effect.Bind(searchStringUpper);
			BestRecipe = null;
			foreach (NameDescCache recipe in recipes)
			{
				recipe.Bind(searchStringUpper);
				if (BestRecipe == null || recipe.CompareTo(BestRecipe) == -1)
				{
					BestRecipe = recipe;
				}
			}
		}

		public void Reset()
		{
			nameDescSearchTerms.Reset();
			effect.Reset();
			foreach (NameDescCache recipe in recipes)
			{
				recipe.Reset();
			}
			BestRecipe = null;
		}

		public bool IsPassingScore()
		{
			return Score >= 79;
		}

		public int CompareTo(object obj)
		{
			BuildingDefCache buildingDefCache = (BuildingDefCache)obj;
			int score = Score;
			int score2 = buildingDefCache.Score;
			int num = -score.CompareTo(score2);
			if (num != 0)
			{
				return num;
			}
			TieBreaker tieBreaker = new TieBreaker(score);
			tieBreaker.Consider(nameDescSearchTerms.nameDesc.name, buildingDefCache.nameDescSearchTerms.nameDesc.name);
			tieBreaker.Consider(nameDescSearchTerms.SearchTermsScore.score, buildingDefCache.nameDescSearchTerms.SearchTermsScore.score);
			tieBreaker.Consider(effect, buildingDefCache.effect);
			return tieBreaker.Consider(nameDescSearchTerms.nameDesc.desc, buildingDefCache.nameDescSearchTerms.nameDesc.desc);
		}
	}

	public class TechItemCache : IComparable, IScore
	{
		public NameDescSearchTermsCache nameDescSearchTerms;

		public List<NameDescCache> recipes;

		public int tier;

		public NameDescCache BestRecipe { get; private set; }

		public int Score => Math.Max(nameDescSearchTerms.Score, (BestRecipe != null) ? BestRecipe.Score : 0);

		public void Bind(string searchStringUpper)
		{
			nameDescSearchTerms.Bind(searchStringUpper);
			BestRecipe = null;
			foreach (NameDescCache recipe in recipes)
			{
				recipe.Bind(searchStringUpper);
				if (BestRecipe == null || recipe.CompareTo(BestRecipe) == -1)
				{
					BestRecipe = recipe;
				}
			}
		}

		public void Reset()
		{
			nameDescSearchTerms.Reset();
			foreach (NameDescCache recipe in recipes)
			{
				recipe.Reset();
			}
			BestRecipe = null;
		}

		public bool IsPassingScore()
		{
			return Score >= 79;
		}

		public int CompareTo(object obj)
		{
			TechItemCache techItemCache = (TechItemCache)obj;
			int score = Score;
			int score2 = techItemCache.Score;
			int num = -score.CompareTo(score2);
			if (num != 0)
			{
				return num;
			}
			TieBreaker tieBreaker = new TieBreaker(score);
			tieBreaker.Consider(nameDescSearchTerms.nameDesc.name, techItemCache.nameDescSearchTerms.nameDesc.name);
			tieBreaker.Consider(nameDescSearchTerms.SearchTermsScore.score, techItemCache.nameDescSearchTerms.SearchTermsScore.score);
			if (!tieBreaker.IsTieBroken)
			{
				int num2 = tier.CompareTo(techItemCache.tier);
				if (num2 != 0)
				{
					return num2;
				}
			}
			tieBreaker.Consider(nameDescSearchTerms.nameDesc.desc, techItemCache.nameDescSearchTerms.nameDesc.desc);
			return tieBreaker.Consider(BestRecipe, techItemCache.BestRecipe);
		}
	}

	public class TechCache : IComparable
	{
		public NameDescSearchTermsCache tech;

		public Dictionary<string, TechItemCache> techItems;

		public int tier;

		public TechItemCache BestItem { get; private set; }

		public int Score => Math.Max(tech.Score, (BestItem != null) ? BestItem.Score : 0);

		public void Bind(string searchStringUpper)
		{
			tech.Bind(searchStringUpper);
			BestItem = null;
			foreach (KeyValuePair<string, TechItemCache> techItem in techItems)
			{
				techItem.Value.Bind(searchStringUpper);
				if (BestItem == null || techItem.Value.CompareTo(BestItem) == -1)
				{
					BestItem = techItem.Value;
				}
			}
		}

		public void Reset()
		{
			tech.Reset();
			foreach (KeyValuePair<string, TechItemCache> techItem in techItems)
			{
				techItem.Value.Reset();
			}
			BestItem = null;
		}

		public bool IsPassingScore()
		{
			return Score >= 79;
		}

		public int CompareTo(object obj)
		{
			TechCache techCache = (TechCache)obj;
			int score = Score;
			int score2 = techCache.Score;
			int num = -score.CompareTo(score2);
			if (num != 0)
			{
				return num;
			}
			TieBreaker tieBreaker = new TieBreaker(score);
			tieBreaker.Consider(tech.nameDesc.name, techCache.tech.nameDesc.name);
			tieBreaker.Consider(tech.SearchTermsScore.score, techCache.tech.SearchTermsScore.score);
			if (!tieBreaker.IsTieBroken)
			{
				int num2 = tier.CompareTo(techCache.tier);
				if (num2 != 0)
				{
					return num2;
				}
			}
			tieBreaker.Consider(tech.nameDesc.desc, techCache.tech.nameDesc.desc);
			return tieBreaker.Consider(BestItem, techCache.BestItem);
		}
	}

	public class SubcategoryCache : IComparable
	{
		public MatchCache subcategory;

		public HashSet<BuildingDefCache> buildingDefs;

		public BuildingDefCache BestBuildingDef { get; private set; }

		public int Score => Math.Max(subcategory.Score, (BestBuildingDef != null) ? BestBuildingDef.Score : 0);

		public void Bind(string searchStringUpper)
		{
			subcategory.Bind(searchStringUpper);
			BestBuildingDef = null;
			foreach (BuildingDefCache buildingDef in buildingDefs)
			{
				buildingDef.Bind(searchStringUpper);
				if (BestBuildingDef == null || buildingDef.CompareTo(BestBuildingDef) == -1)
				{
					BestBuildingDef = buildingDef;
				}
			}
		}

		public void Reset()
		{
			subcategory.Reset();
			foreach (BuildingDefCache buildingDef in buildingDefs)
			{
				buildingDef.Reset();
			}
			BestBuildingDef = null;
		}

		public bool IsPassingScore()
		{
			return Score >= 79;
		}

		public int CompareTo(object obj)
		{
			SubcategoryCache subcategoryCache = (SubcategoryCache)obj;
			int score = Score;
			int score2 = subcategoryCache.Score;
			int num = -score.CompareTo(score2);
			if (num != 0)
			{
				return num;
			}
			TieBreaker tieBreaker = new TieBreaker(score);
			tieBreaker.Consider(subcategory, subcategoryCache.subcategory);
			return tieBreaker.Consider(BestBuildingDef, subcategoryCache.BestBuildingDef);
		}
	}

	public const int MATCH_SCORE_MIN = 0;

	public const int MATCH_SCORE_MAX = 100;

	public const int MATCH_SCORE_THRESHOLD = 79;

	private static readonly HashSet<string> MeaninglessWords = new HashSet<string>();

	private static readonly char[] COMMA_DELIMETERS = new char[2] { ' ', ',' };

	private const int LHS_GT_RHS = -1;

	private const int RHS_GT_LHS = 1;

	private static void CacheMeaninglessWords()
	{
		if (MeaninglessWords.Count != 0)
		{
			return;
		}
		ListPool<string, MatchCache>.PooledList pooledList = ListPool<string, MatchCache>.Allocate();
		AddCommaDelimitedSearchTerms(SEARCH_TERMS.SUPPRESSED, pooledList);
		foreach (string item in pooledList)
		{
			MeaninglessWords.Add(item);
		}
		pooledList.Recycle();
	}

	public static bool IsPassingScore(int score)
	{
		return score >= 79;
	}

	public static string Canonicalize(string s)
	{
		return FuzzySearch.Canonicalize(s).ToUpper();
	}

	public static string CanonicalizePhrase(string s)
	{
		string text = FuzzySearch.Canonicalize(s).ToUpper();
		FuzzySearch.Features features = FuzzySearch.GetFeatures();
		if ((features & (FuzzySearch.Features.Suppress1And2LetterWords | FuzzySearch.Features.SuppressMeaninglessWords)) == 0)
		{
			return text;
		}
		string[] array = text.Split(FuzzySearch.TOKEN_SEPARATORS);
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = (features & FuzzySearch.Features.Suppress1And2LetterWords) != 0;
		bool flag2 = (features & FuzzySearch.Features.SuppressMeaninglessWords) != 0;
		if (flag2)
		{
			CacheMeaninglessWords();
		}
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if ((!flag || text2.Length > 2) && (!flag2 || !MeaninglessWords.Contains(text2)))
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.AppendFormat(" {0}", text2);
				}
				else
				{
					stringBuilder.Append(text2);
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static void AddCommaDelimitedSearchTerms(string commaDelimitedSearchTerms, List<string> searchTerms)
	{
		string[] array = commaDelimitedSearchTerms.ToUpper().Split(COMMA_DELIMETERS, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string item in array2)
		{
			searchTerms.Add(item);
		}
	}

	public static Dictionary<string, TechCache> CacheTechs()
	{
		Dictionary<string, TechCache> dictionary = new Dictionary<string, TechCache>();
		ListPool<ComplexRecipe, TechCache>.PooledList pooledList = ListPool<ComplexRecipe, TechCache>.Allocate();
		Techs techs = Db.Get().Techs;
		for (int i = 0; i != techs.Count; i++)
		{
			Tech tech = (Tech)techs.GetResource(i);
			Dictionary<string, TechItemCache> dictionary2 = new Dictionary<string, TechItemCache>();
			foreach (TechItem unlockedItem in tech.unlockedItems)
			{
				pooledList.Clear();
				BuildingDef.CollectFabricationRecipes(unlockedItem.Id, pooledList);
				List<NameDescCache> list = new List<NameDescCache>();
				foreach (ComplexRecipe item in pooledList)
				{
					list.Add(new NameDescCache
					{
						name = new MatchCache
						{
							text = Canonicalize(item.GetUIName(includeAmounts: false))
						},
						desc = new MatchCache
						{
							text = CanonicalizePhrase(item.description)
						}
					});
				}
				TechItem techItem = Db.Get().TechItems.Get(unlockedItem.Id);
				TechItemCache value = new TechItemCache
				{
					nameDescSearchTerms = new NameDescSearchTermsCache
					{
						nameDesc = new NameDescCache
						{
							name = new MatchCache
							{
								text = Canonicalize(techItem.Name)
							},
							desc = new MatchCache
							{
								text = CanonicalizePhrase(techItem.description)
							}
						},
						searchTerms = techItem.searchTerms
					},
					recipes = list,
					tier = tech.tier
				};
				dictionary2[unlockedItem.Id] = value;
			}
			TechCache value2 = new TechCache
			{
				tech = new NameDescSearchTermsCache
				{
					nameDesc = new NameDescCache
					{
						name = new MatchCache
						{
							text = Canonicalize(tech.Name)
						},
						desc = new MatchCache
						{
							text = CanonicalizePhrase(tech.desc)
						}
					},
					searchTerms = tech.searchTerms
				},
				techItems = dictionary2,
				tier = tech.tier
			};
			dictionary[tech.Id] = value2;
		}
		pooledList.Recycle();
		return dictionary;
	}

	public static BuildingDefCache MakeBuildingDefCache(BuildingDef def)
	{
		NameDescSearchTermsCache nameDescSearchTerms = new NameDescSearchTermsCache
		{
			nameDesc = new NameDescCache
			{
				name = new MatchCache
				{
					text = Canonicalize(def.Name)
				},
				desc = new MatchCache
				{
					text = CanonicalizePhrase(def.Desc)
				}
			},
			searchTerms = def.SearchTerms
		};
		MatchCache effect = new MatchCache
		{
			text = CanonicalizePhrase(def.Effect)
		};
		List<NameDescCache> list = new List<NameDescCache>();
		ListPool<ComplexRecipe, PlanBuildingToggle>.PooledList pooledList = ListPool<ComplexRecipe, PlanBuildingToggle>.Allocate();
		BuildingDef.CollectFabricationRecipes(def.PrefabID, pooledList);
		foreach (ComplexRecipe item in pooledList)
		{
			list.Add(new NameDescCache
			{
				name = new MatchCache
				{
					text = Canonicalize(item.GetUIName(includeAmounts: false))
				},
				desc = new MatchCache
				{
					text = CanonicalizePhrase(item.description)
				}
			});
		}
		pooledList.Recycle();
		return new BuildingDefCache
		{
			nameDescSearchTerms = nameDescSearchTerms,
			effect = effect,
			recipes = list
		};
	}
}
