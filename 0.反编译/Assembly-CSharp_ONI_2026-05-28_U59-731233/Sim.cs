using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class Sim
{
	public delegate int GAME_MessageHandler(int message_id, IntPtr data);

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct DLLExceptionHandlerMessage
	{
		public IntPtr callstack;

		public IntPtr dmpFilename;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct DLLReportMessageMessage
	{
		public IntPtr callstack;

		public IntPtr message;

		public IntPtr file;

		public int line;
	}

	private enum GameHandledMessages
	{
		ExceptionHandler,
		ReportMessage
	}

	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct PhysicsData
	{
		public float temperature;

		public float mass;

		public float pressure;

		public void Write(BinaryWriter writer)
		{
			writer.Write(temperature);
			writer.Write(mass);
			writer.Write(pressure);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Cell
	{
		public enum Properties
		{
			GasImpermeable = 1,
			LiquidImpermeable = 2,
			SolidImpermeable = 4,
			Unbreakable = 8,
			Transparent = 0x10,
			Opaque = 0x20,
			NotifyOnMelt = 0x40,
			ConstructedTile = 0x80
		}

		public ushort elementIdx;

		public byte properties;

		public byte insulation;

		public byte strengthInfo;

		public byte pad0;

		public byte pad1;

		public byte pad2;

		public float temperature;

		public float mass;

		public void Write(BinaryWriter writer)
		{
			writer.Write(elementIdx);
			writer.Write((byte)0);
			writer.Write(insulation);
			writer.Write((byte)0);
			writer.Write(pad0);
			writer.Write(pad1);
			writer.Write(pad2);
			writer.Write(temperature);
			writer.Write(mass);
		}

		public void SetValues(global::Element elem, List<global::Element> elements)
		{
			SetValues(elem, elem.defaultValues, elements);
		}

		public void SetValues(global::Element elem, PhysicsData pd, List<global::Element> elements)
		{
			elementIdx = (ushort)elements.IndexOf(elem);
			temperature = pd.temperature;
			mass = pd.mass;
			insulation = byte.MaxValue;
			DebugUtil.Assert(temperature > 0f || mass == 0f, "A non-zero mass cannot have a <= 0 temperature");
		}

		public void SetValues(ushort new_elem_idx, float new_temperature, float new_mass)
		{
			elementIdx = new_elem_idx;
			temperature = new_temperature;
			mass = new_mass;
			insulation = byte.MaxValue;
			DebugUtil.Assert(temperature > 0f || mass == 0f, "A non-zero mass cannot have a <= 0 temperature");
		}
	}

	public enum MaterialPropertiesAccessor
	{
		Glows = 0,
		Caustics = 1,
		Metalic = 2,
		OpaqueLiquid = 3,
		TextureID = 24
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct SimBackwall
	{
		public ushort elementIdx;

		private byte pad0;

		private byte pad1;

		public float mass;

		public float temperature;

		public void SetValues(ushort elementIdx, float mass, float temperature)
		{
			this.elementIdx = elementIdx;
			this.mass = mass;
			this.temperature = temperature;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(elementIdx);
			writer.Write(pad0);
			writer.Write(pad1);
			writer.Write(mass);
			writer.Write(temperature);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct Element
	{
		private const int kMaxGradientCount = 6;

		public SimHashes id;

		public ushort lowTempTransitionIdx;

		public ushort highTempTransitionIdx;

		public ushort elementsTableIdx;

		public byte state;

		public byte numberOfGradientColors;

		public float specificHeatCapacity;

		public float thermalConductivity;

		public float molarMass;

		public float solidSurfaceAreaMultiplier;

		public float liquidSurfaceAreaMultiplier;

		public float gasSurfaceAreaMultiplier;

		public float flow;

		public float viscosity;

		public float minHorizontalFlow;

		public float minVerticalFlow;

		public float maxMass;

		public float lowTemp;

		public float highTemp;

		public float strength;

		public SimHashes lowTempTransitionOreID;

		public float lowTempTransitionOreMassConversion;

		public SimHashes highTempTransitionOreID;

		public float highTempTransitionOreMassConversion;

		public ushort sublimateIndex;

		public ushort convertIndex;

		public uint materialProperties;

		public uint colour;

		public unsafe fixed uint gradientColours[6];

		public SpawnFXHashes sublimateFX;

		public float sublimateRate;

		public float sublimateEfficiency;

		public float sublimateProbability;

		public float offGasProbability;

		public float lightAbsorptionFactor;

		public float radiationAbsorptionFactor;

		public float radiationPer1000Mass;

		public PhysicsData defaultValues;

		public unsafe Element(global::Element e, List<global::Element> elements)
		{
			id = e.id;
			state = (byte)e.state;
			if (e.HasTag(GameTags.Unstable))
			{
				state |= 8;
			}
			int num = elements.FindIndex((global::Element ele) => ele.id == e.lowTempTransitionTarget);
			int num2 = elements.FindIndex((global::Element ele) => ele.id == e.highTempTransitionTarget);
			lowTempTransitionIdx = (ushort)((num >= 0) ? ((uint)num) : 65535u);
			highTempTransitionIdx = (ushort)((num2 >= 0) ? ((uint)num2) : 65535u);
			elementsTableIdx = (ushort)elements.IndexOf(e);
			specificHeatCapacity = e.specificHeatCapacity;
			thermalConductivity = e.thermalConductivity;
			solidSurfaceAreaMultiplier = e.solidSurfaceAreaMultiplier;
			liquidSurfaceAreaMultiplier = e.liquidSurfaceAreaMultiplier;
			gasSurfaceAreaMultiplier = e.gasSurfaceAreaMultiplier;
			molarMass = e.molarMass;
			strength = e.strength;
			flow = e.flow;
			viscosity = e.viscosity;
			minHorizontalFlow = e.minHorizontalFlow;
			minVerticalFlow = e.minVerticalFlow;
			maxMass = e.maxMass;
			lowTemp = e.lowTemp;
			highTemp = e.highTemp;
			highTempTransitionOreID = e.highTempTransitionOreID;
			highTempTransitionOreMassConversion = e.highTempTransitionOreMassConversion;
			lowTempTransitionOreID = e.lowTempTransitionOreID;
			lowTempTransitionOreMassConversion = e.lowTempTransitionOreMassConversion;
			sublimateIndex = (ushort)elements.FindIndex((global::Element ele) => ele.id == e.sublimateId);
			convertIndex = (ushort)elements.FindIndex((global::Element ele) => ele.id == e.convertId);
			numberOfGradientColors = (byte)e.substance.Gradient.colorKeys.Length;
			for (int num3 = 0; num3 < 6; num3++)
			{
				if (num3 < e.substance.Gradient.colorKeys.Length)
				{
					Color color = e.substance.Gradient.colorKeys[num3].color;
					color.a = e.substance.Gradient.colorKeys[num3].time;
					Color32 color2 = color;
					uint num4 = (uint)((color2.a << 24) | (color2.b << 16) | (color2.g << 8) | color2.r);
					gradientColours[num3] = num4;
				}
				else
				{
					gradientColours[num3] = 0u;
				}
			}
			materialProperties = 0u;
			if (e.substance.Glows)
			{
				materialProperties |= 1u;
			}
			if (e.substance.LiquidCaustics)
			{
				materialProperties |= 2u;
			}
			if (e.substance.Metalic)
			{
				materialProperties |= 4u;
			}
			if (e.substance.IsOpaqueLiquid)
			{
				materialProperties |= 8u;
			}
			materialProperties |= (uint)((Substance.SubstanceTexture)255 & e.substance.Texture) << 24;
			if (e.substance == null)
			{
				colour = 0u;
			}
			else
			{
				Color32 color3 = e.substance.colour;
				colour = (uint)((color3.a << 24) | (color3.b << 16) | (color3.g << 8) | color3.r);
			}
			sublimateFX = e.sublimateFX;
			sublimateRate = e.sublimateRate;
			sublimateEfficiency = e.sublimateEfficiency;
			sublimateProbability = e.sublimateProbability;
			offGasProbability = e.offGasPercentage;
			lightAbsorptionFactor = e.lightAbsorptionFactor;
			radiationAbsorptionFactor = e.radiationAbsorptionFactor;
			radiationPer1000Mass = e.radiationPer1000Mass;
			defaultValues = e.defaultValues;
		}

		public unsafe void Write(BinaryWriter writer)
		{
			writer.Write((int)id);
			writer.Write(lowTempTransitionIdx);
			writer.Write(highTempTransitionIdx);
			writer.Write(elementsTableIdx);
			writer.Write(state);
			writer.Write(numberOfGradientColors);
			writer.Write(specificHeatCapacity);
			writer.Write(thermalConductivity);
			writer.Write(molarMass);
			writer.Write(solidSurfaceAreaMultiplier);
			writer.Write(liquidSurfaceAreaMultiplier);
			writer.Write(gasSurfaceAreaMultiplier);
			writer.Write(flow);
			writer.Write(viscosity);
			writer.Write(minHorizontalFlow);
			writer.Write(minVerticalFlow);
			writer.Write(maxMass);
			writer.Write(lowTemp);
			writer.Write(highTemp);
			writer.Write(strength);
			writer.Write((int)lowTempTransitionOreID);
			writer.Write(lowTempTransitionOreMassConversion);
			writer.Write((int)highTempTransitionOreID);
			writer.Write(highTempTransitionOreMassConversion);
			writer.Write(sublimateIndex);
			writer.Write(convertIndex);
			writer.Write(materialProperties);
			writer.Write(colour);
			for (int i = 0; i < 6; i++)
			{
				writer.Write(gradientColours[i]);
			}
			writer.Write((int)sublimateFX);
			writer.Write(sublimateRate);
			writer.Write(sublimateEfficiency);
			writer.Write(sublimateProbability);
			writer.Write(offGasProbability);
			writer.Write(lightAbsorptionFactor);
			writer.Write(radiationAbsorptionFactor);
			writer.Write(radiationPer1000Mass);
			defaultValues.Write(writer);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct DiseaseCell
	{
		public byte diseaseIdx;

		private byte reservedInfestationTickCount;

		private byte pad1;

		private byte pad2;

		public int elementCount;

		private float reservedAccumulatedError;

		public static readonly DiseaseCell Invalid = new DiseaseCell
		{
			diseaseIdx = byte.MaxValue,
			elementCount = 0
		};

		public void Write(BinaryWriter writer)
		{
			writer.Write(diseaseIdx);
			writer.Write(reservedInfestationTickCount);
			writer.Write(pad1);
			writer.Write(pad2);
			writer.Write(elementCount);
			writer.Write(reservedAccumulatedError);
		}
	}

	public delegate void GAME_Callback();

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct SolidInfo
	{
		public int cellIdx;

		public int isSolid;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct LiquidChangeInfo
	{
		public int cellIdx;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct SolidSubstanceChangeInfo
	{
		public int cellIdx;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct SubstanceChangeInfo
	{
		public int cellIdx;

		public ushort oldElemIdx;

		public ushort newElemIdx;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct CallbackInfo
	{
		public int callbackIdx;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct GameDataUpdate
	{
		public int numFramesProcessed;

		public unsafe ushort* elementIdx;

		public unsafe float* temperature;

		public unsafe float* mass;

		public unsafe byte* properties;

		public unsafe byte* insulation;

		public unsafe byte* strengthInfo;

		public unsafe float* radiation;

		public unsafe byte* diseaseIdx;

		public unsafe int* diseaseCount;

		public unsafe ushort* backwallElement;

		public unsafe float* backwallMass;

		public unsafe float* backwallTemperature;

		public int numSolidInfo;

		public unsafe SolidInfo* solidInfo;

		public int numLiquidChangeInfo;

		public unsafe LiquidChangeInfo* liquidChangeInfo;

		public int numSolidSubstanceChangeInfo;

		public unsafe SolidSubstanceChangeInfo* solidSubstanceChangeInfo;

		public int numSubstanceChangeInfo;

		public unsafe SubstanceChangeInfo* substanceChangeInfo;

		public int numCallbackInfo;

		public unsafe CallbackInfo* callbackInfo;

		public int numSpawnFallingLiquidInfo;

		public unsafe SpawnFallingLiquidInfo* spawnFallingLiquidInfo;

		public int numDigInfo;

		public unsafe SpawnOreInfo* digInfo;

		public int numSpawnOreInfo;

		public unsafe SpawnOreInfo* spawnOreInfo;

		public int numSpawnFXInfo;

		public unsafe SpawnFXInfo* spawnFXInfo;

		public int numUnstableCellInfo;

		public unsafe UnstableCellInfo* unstableCellInfo;

		public int numWorldDamageInfo;

		public unsafe WorldDamageInfo* worldDamageInfo;

		public int numBuildingTemperatures;

		public unsafe BuildingTemperatureInfo* buildingTemperatures;

		public int numMassConsumedCallbacks;

		public unsafe MassConsumedCallback* massConsumedCallbacks;

		public int numMassEmittedCallbacks;

		public unsafe MassEmittedCallback* massEmittedCallbacks;

		public int numDiseaseConsumptionCallbacks;

		public unsafe DiseaseConsumptionCallback* diseaseConsumptionCallbacks;

		public int numComponentStateChangedMessages;

		public unsafe ComponentStateChangedMessage* componentStateChangedMessages;

		public int numRemovedMassEntries;

		public unsafe ConsumedMassInfo* removedMassEntries;

		public int numEmittedMassEntries;

		public unsafe EmittedMassInfo* emittedMassEntries;

		public int numElementChunkInfos;

		public unsafe ElementChunkInfo* elementChunkInfos;

		public int numElementChunkMeltedInfos;

		public unsafe MeltedInfo* elementChunkMeltedInfos;

		public int numBuildingOverheatInfos;

		public unsafe MeltedInfo* buildingOverheatInfos;

		public int numBuildingNoLongerOverheatedInfos;

		public unsafe MeltedInfo* buildingNoLongerOverheatedInfos;

		public int numBuildingMeltedInfos;

		public unsafe MeltedInfo* buildingMeltedInfos;

		public int numCellMeltedInfos;

		public unsafe CellMeltedInfo* cellMeltedInfos;

		public int numBackwallElementChangedInfos;

		public unsafe BackwallElementChangedInfo* backwallElementChangedInfos;

		public int numBackwallShouldTransitionInfos;

		public unsafe BackwallShouldTransitionInfo* backwallShouldTransitionInfos;

		public int numDiseaseEmittedInfos;

		public unsafe DiseaseEmittedInfo* diseaseEmittedInfos;

		public int numDiseaseConsumedInfos;

		public unsafe DiseaseConsumedInfo* diseaseConsumedInfos;

		public int numRadiationConsumedCallbacks;

		public unsafe ConsumedRadiationCallback* radiationConsumedCallbacks;

		public unsafe float* accumulatedFlow;

		public IntPtr propertyTextureFlow;

		public IntPtr propertyTextureLiquid;

		public IntPtr propertyTextureLiquidData;

		public IntPtr propertyTextureMaterialData;

		public IntPtr propertyTextureExposedToSunlight;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct SpawnFallingLiquidInfo
	{
		public int cellIdx;

		public ushort elemIdx;

		public byte diseaseIdx;

		public byte pad0;

		public float mass;

		public float temperature;

		public int diseaseCount;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct SpawnOreInfo
	{
		public int cellIdx;

		public ushort elemIdx;

		public byte diseaseIdx;

		private byte pad0;

		public float mass;

		public float temperature;

		public int diseaseCount;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct SpawnFXInfo
	{
		public int cellIdx;

		public int fxHash;

		public float rotation;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct UnstableCellInfo
	{
		public enum FallingInfo
		{
			StartedFalling,
			StoppedFalling
		}

		public int cellIdx;

		public ushort elemIdx;

		public byte fallingInfo;

		public byte diseaseIdx;

		public float mass;

		public float temperature;

		public int diseaseCount;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct NewGameFrame
	{
		public float elapsedSeconds;

		public int minX;

		public int minY;

		public int maxX;

		public int maxY;

		public float currentSunlightIntensity;

		public float currentCosmicRadiationIntensity;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct WorldDamageInfo
	{
		public int gameCell;

		public int damageSourceOffset;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct PipeTemperatureChange
	{
		public int cellIdx;

		public float temperature;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct MassConsumedCallback
	{
		public int callbackIdx;

		public ushort elemIdx;

		public byte diseaseIdx;

		private byte pad0;

		public float mass;

		public float temperature;

		public int diseaseCount;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct MassEmittedCallback
	{
		public int callbackIdx;

		public ushort elemIdx;

		public byte suceeded;

		public byte diseaseIdx;

		public float mass;

		public float temperature;

		public int diseaseCount;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct DiseaseConsumptionCallback
	{
		public int callbackIdx;

		public byte diseaseIdx;

		private byte pad0;

		private byte pad1;

		private byte pad2;

		public int diseaseCount;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct ComponentStateChangedMessage
	{
		public int callbackIdx;

		public int simHandle;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct DebugProperties
	{
		public float buildingTemperatureScale;

		public float buildingToBuildingTemperatureScale;

		public byte isDebugEditing;

		public byte pad0;

		public byte pad1;

		public byte pad2;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct EmittedMassInfo
	{
		public ushort elemIdx;

		public byte diseaseIdx;

		public byte pad0;

		public float mass;

		public float temperature;

		public int diseaseCount;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct ConsumedMassInfo
	{
		public int simHandle;

		public ushort removedElemIdx;

		public byte diseaseIdx;

		private byte pad0;

		public float mass;

		public float temperature;

		public int diseaseCount;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct ConsumedDiseaseInfo
	{
		public int simHandle;

		public byte diseaseIdx;

		private byte pad0;

		private byte pad1;

		private byte pad2;

		public int diseaseCount;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct ElementChunkInfo
	{
		public float temperature;

		public float deltaKJ;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct MeltedInfo
	{
		public int handle;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct CellMeltedInfo
	{
		public int gameCell;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct BackwallElementChangedInfo
	{
		public int gameCell;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct BackwallShouldTransitionInfo
	{
		public int gameCell;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct BuildingTemperatureInfo
	{
		public int handle;

		public float temperature;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct BuildingConductivityData
	{
		public float temperature;

		public float heatCapacity;

		public float thermalConductivity;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct DiseaseEmittedInfo
	{
		public byte diseaseIdx;

		private byte pad0;

		private byte pad1;

		private byte pad2;

		public int count;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct DiseaseConsumedInfo
	{
		public byte diseaseIdx;

		private byte pad0;

		private byte pad1;

		private byte pad2;

		public int count;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct ConsumedRadiationCallback
	{
		public int callbackIdx;

		public int gameCell;

		public float radiation;
	}

	public const int InvalidHandle = -1;

	public const int QueuedRegisterHandle = -2;

	public const byte InvalidDiseaseIdx = byte.MaxValue;

	public const ushort InvalidElementIdx = ushort.MaxValue;

	public const byte SpaceZoneID = byte.MaxValue;

	public const byte SolidZoneID = 0;

	public const int ChunkEdgeSize = 32;

	public const float StateTransitionEnergy = 3f;

	public const float ZeroDegreesCentigrade = 273.15f;

	public const float StandardTemperature = 293.15f;

	public const float StandardMeltingPointOffset = 10f;

	public const float StandardPressure = 101.3f;

	public const float Epsilon = 0.0001f;

	public const float MaxTemperature = 10000f;

	public const float MinTemperature = 0f;

	public const float MaxRadiation = 9000000f;

	public const float MinRadiation = 0f;

	public const float MaxMass = 10000f;

	public const float MinMass = 1.0001f;

	public const float MAX_SUBLIMATE_MASS = 1.8f;

	public const float MIN_LIQUID_MASS = 0.01f;

	public const float MIN_GAS_MASS = 1E-09f;

	public const float MIN_STATE_TRANSITION_MASS = 0.001f;

	private const int PressureUpdateInterval = 1;

	private const int TemperatureUpdateInterval = 1;

	private const int LiquidUpdateInterval = 1;

	private const int LifeUpdateInterval = 1;

	public const byte ClearSkyGridValue = 253;

	public const int PACKING_ALIGNMENT = 4;

	public static bool IsRadiationEnabled()
	{
		return DlcManager.FeatureRadiationEnabled();
	}

	public static bool IsValidHandle(int h)
	{
		return h != -1 && h != -2;
	}

	public static int GetHandleIndex(int h)
	{
		return h & 0xFFFFFF;
	}

	[DllImport("SimDLL")]
	public static extern void SIM_Initialize(GAME_MessageHandler callback);

	[DllImport("SimDLL")]
	public static extern void SIM_Shutdown();

	[DllImport("SimDLL")]
	public unsafe static extern IntPtr SIM_HandleMessage(int sim_msg_id, int msg_length, byte* msg);

	[DllImport("SimDLL")]
	public unsafe static extern IntPtr SIM_HandleMessages(int sim_msg_id, int msg_length, int msg_count, byte* msg);

	[DllImport("SimDLL")]
	private unsafe static extern byte* SIM_BeginSave(int* size, int x, int y);

	[DllImport("SimDLL")]
	private static extern void SIM_EndSave();

	[DllImport("SimDLL")]
	public static extern void SIM_DebugCrash();

	public unsafe static IntPtr HandleMessage(SimMessageHashes sim_msg_id, int msg_length, byte[] msg)
	{
		IntPtr result;
		fixed (byte* msg2 = msg)
		{
			result = SIM_HandleMessage((int)sim_msg_id, msg_length, msg2);
		}
		return result;
	}

	public unsafe static void Save(BinaryWriter writer, int x, int y)
	{
		int num = default(int);
		byte* ptr = SIM_BeginSave(&num, x, y);
		byte[] array = new byte[num];
		Marshal.Copy((IntPtr)ptr, array, 0, num);
		SIM_EndSave();
		writer.Write(num);
		writer.Write(array);
	}

	public unsafe static int LoadWorld(IReader reader)
	{
		int num = reader.ReadInt32();
		IntPtr intPtr;
		fixed (byte* msg = reader.ReadBytes(num))
		{
			intPtr = SIM_HandleMessage(-672538170, num, msg);
		}
		if (intPtr == IntPtr.Zero)
		{
			return -1;
		}
		return 0;
	}

	public static void AllocateCells(int width, int height, bool headless = false)
	{
		using MemoryStream memoryStream = new MemoryStream(16);
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(width);
		binaryWriter.Write(height);
		bool value = IsRadiationEnabled();
		binaryWriter.Write(value);
		binaryWriter.Write(headless);
		binaryWriter.Flush();
		HandleMessage(SimMessageHashes.AllocateCells, (int)memoryStream.Length, memoryStream.GetBuffer());
	}

	public unsafe static int Load(IReader reader)
	{
		int num = reader.ReadInt32();
		IntPtr intPtr;
		fixed (byte* msg = reader.ReadBytes(num))
		{
			intPtr = SIM_HandleMessage(-672538170, num, msg);
		}
		if (intPtr == IntPtr.Zero)
		{
			return -1;
		}
		return 0;
	}

	public unsafe static void Start()
	{
		GameDataUpdate* ptr = (GameDataUpdate*)(void*)SIM_HandleMessage(-931446686, 0, null);
		Grid.elementIdx = ptr->elementIdx;
		Grid.temperature = ptr->temperature;
		Grid.radiation = ptr->radiation;
		Grid.mass = ptr->mass;
		Grid.properties = ptr->properties;
		Grid.strengthInfo = ptr->strengthInfo;
		Grid.insulation = ptr->insulation;
		Grid.diseaseIdx = ptr->diseaseIdx;
		Grid.diseaseCount = ptr->diseaseCount;
		BackwallManager.UpdateFromSim(ptr);
		Grid.AccumulatedFlowValues = ptr->accumulatedFlow;
		PropertyTextures.externalFlowTex = ptr->propertyTextureFlow;
		PropertyTextures.externalLiquidTex = ptr->propertyTextureLiquid;
		PropertyTextures.externalLiquidDataTex = ptr->propertyTextureLiquidData;
		PropertyTextures.externalMaterialDataTex = ptr->propertyTextureMaterialData;
		PropertyTextures.externalExposedToSunlight = ptr->propertyTextureExposedToSunlight;
		Grid.InitializeCells();
	}

	public unsafe static void Shutdown()
	{
		SIM_Shutdown();
		Grid.mass = null;
	}

	[DllImport("SimDLL")]
	public unsafe static extern char* SYSINFO_Acquire();

	[DllImport("SimDLL")]
	public static extern void SYSINFO_Release();

	public unsafe static int DLL_MessageHandler(int message_id, IntPtr data)
	{
		switch ((GameHandledMessages)message_id)
		{
		case GameHandledMessages.ReportMessage:
		{
			DLLReportMessageMessage* ptr2 = (DLLReportMessageMessage*)(void*)data;
			string msg = "SimMessage: " + Marshal.PtrToStringAnsi(ptr2->message);
			string stack_trace2;
			if (ptr2->callstack != IntPtr.Zero)
			{
				stack_trace2 = Marshal.PtrToStringAnsi(ptr2->callstack);
			}
			else
			{
				string text = Marshal.PtrToStringAnsi(ptr2->file);
				int line = ptr2->line;
				stack_trace2 = text + ":" + line;
			}
			KCrashReporter.ReportSimDLLCrash(msg, stack_trace2, null);
			return 0;
		}
		case GameHandledMessages.ExceptionHandler:
		{
			DLLExceptionHandlerMessage* ptr = (DLLExceptionHandlerMessage*)(void*)data;
			string stack_trace = Marshal.PtrToStringAnsi(ptr->callstack);
			string dmp_filename = Marshal.PtrToStringAnsi(ptr->dmpFilename);
			KCrashReporter.ReportSimDLLCrash("SimDLL Crash Dump", stack_trace, dmp_filename);
			return 0;
		}
		default:
			return -1;
		}
	}
}
