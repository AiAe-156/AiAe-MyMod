using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace UtilLibs;

public static class TranspilerHelper
{
	private static readonly HashSet<OpCode> loadVarCodes = new HashSet<OpCode>
	{
		OpCodes.Ldloc_0,
		OpCodes.Ldloc_1,
		OpCodes.Ldloc_2,
		OpCodes.Ldloc_3,
		OpCodes.Ldloc,
		OpCodes.Ldloca,
		OpCodes.Ldloc_S,
		OpCodes.Ldloca_S
	};

	public static bool IsLdloc(this CodeInstruction code, LocalBuilder variable = null)
	{
		if (!loadVarCodes.Contains(code.opcode))
		{
			return false;
		}
		if (variable != null)
		{
			return object.Equals(variable, code.operand);
		}
		return true;
	}

	public static bool CallsConstructor(this CodeInstruction code, ConstructorInfo constructor)
	{
		if (constructor == null)
		{
			throw new ArgumentNullException("constructor");
		}
		if (code.opcode != OpCodes.Newobj)
		{
			return false;
		}
		return object.Equals(code.operand, constructor);
	}

	public static int FindIndexOfNextLocalIndex(List<CodeInstruction> codeInstructions, CodeInstruction start, bool goingDescending = true)
	{
		return FindIndexOfNextLocalIndex(codeInstructions, codeInstructions.IndexOf(start), goingDescending);
	}

	public static int FindIndexOfNextLocalIndex(List<CodeInstruction> codeInstructions, int insertionIndex, bool goingDescending = true)
	{
		return FindIndicesOfLocalsByIndex(codeInstructions, insertionIndex, 1, goingDescending)[0];
	}

	public static int[] FindIndicesOfLocalsByIndex(List<CodeInstruction> codeInstructions, int insertionIndex, int numberOfVarsToFind = 1, bool goingDescending = true)
	{
		List<int> list = new List<int>();
		if (insertionIndex != -1)
		{
			int num = ((!goingDescending) ? 1 : (-1));
			for (int i = insertionIndex + num; i >= 0 && i < codeInstructions.Count; i += num)
			{
				if (list.Count >= numberOfVarsToFind)
				{
					break;
				}
				if (codeInstructions[i].IsLdloc())
				{
					int item = GiveOpCodeIndexFromLocalBuilder(codeInstructions[i]);
					if (!list.Contains(item))
					{
						list.Insert(0, item);
					}
					break;
				}
			}
		}
		else
		{
			list.Add(-1);
		}
		return list.ToArray();
	}

	public static Tuple<int, int> FindIndexOfNextLocalIndexWithPosition(List<CodeInstruction> codeInstructions, int insertionIndex, bool goingBackwards = true)
	{
		Tuple<int[], int[]> val = FindIndicesOfLocalsByIndexWithPositions(codeInstructions, insertionIndex, 1, goingBackwards);
		return new Tuple<int, int>(val.first[0], val.second[0]);
	}

	public static Tuple<int[], int[]> FindIndicesOfLocalsByIndexWithPositions(List<CodeInstruction> codeInstructions, int insertionIndex, int numberOfVarsToFind = 1, bool goingBackwards = true)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		if (insertionIndex != -1)
		{
			int num = ((!goingBackwards) ? 1 : (-1));
			for (int i = insertionIndex - 1; i >= 0 && i < codeInstructions.Count; i += num)
			{
				if (list.Count >= numberOfVarsToFind)
				{
					break;
				}
				if (codeInstructions[i].IsLdloc())
				{
					int item = GiveOpCodeIndexFromLocalBuilder(codeInstructions[i]);
					if (!list.Contains(item))
					{
						list.Insert(0, item);
						list2.Insert(0, i);
					}
					break;
				}
			}
		}
		else
		{
			list.Add(-1);
		}
		return new Tuple<int[], int[]>(list.ToArray(), list2.ToArray());
	}

	private static int GiveOpCodeIndexFromLocalBuilder(CodeInstruction codeInstruction)
	{
		OpCode opcode = codeInstruction.opcode;
		if (opcode == OpCodes.Ldloc_0)
		{
			return 0;
		}
		if (opcode == OpCodes.Ldloc_1)
		{
			return 1;
		}
		if (opcode == OpCodes.Ldloc_2)
		{
			return 2;
		}
		if (opcode == OpCodes.Ldloc_3)
		{
			return 3;
		}
		if (codeInstruction.operand == null)
		{
			return -1;
		}
		return ((LocalBuilder)codeInstruction.operand).LocalIndex;
	}

	public static void PrintInstructions(List<CodeInstruction> codes, bool extendedInfo = false)
	{
		SgtLogger.l("\n", "IL-Dump Start:");
		for (int i = 0; i < codes.Count; i++)
		{
			CodeInstruction val = codes[i];
			if (extendedInfo)
			{
				if (val.operand != null)
				{
					SgtLogger.l(i + "=> OpCode: " + val.opcode.ToString() + "::" + val.operand?.ToString() + "<> typeof (" + (val.operand?.GetType())?.ToString() + ")", "IL");
				}
				else
				{
					SgtLogger.l(i + "=> OpCode: " + val.opcode, "IL");
				}
			}
			else
			{
				SgtLogger.l(i + ": " + (object)val, "IL");
			}
		}
		SgtLogger.l("\n", "IL-Dump Finished");
	}

	public static bool GetLocIndexOfFirst<T>(MethodBase original, out int index)
	{
		index = -1;
		MethodBody methodBody = original.GetMethodBody();
		if (methodBody == null)
		{
			return false;
		}
		IList<LocalVariableInfo> localVariables = methodBody.LocalVariables;
		foreach (LocalVariableInfo item in localVariables)
		{
			if (item == null || !(item.LocalType == typeof(T)))
			{
				continue;
			}
			index = item.LocalIndex;
			return true;
		}
		return false;
	}

	public static bool GetLocIndexOfLast<T>(MethodBase original, out int index)
	{
		index = -1;
		MethodBody methodBody = original.GetMethodBody();
		if (methodBody == null)
		{
			return false;
		}
		IList<LocalVariableInfo> localVariables = methodBody.LocalVariables;
		localVariables = localVariables.Reverse().ToList();
		foreach (LocalVariableInfo item in localVariables)
		{
			if (item == null || !(item.LocalType == typeof(T)))
			{
				continue;
			}
			index = item.LocalIndex;
			return true;
		}
		return false;
	}
}
