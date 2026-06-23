using System;
using VYaml.Annotations;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;

namespace ElementData;

[YamlObject(NamingConvention.LowerCamelCase)]
public class ElementEntry
{
	[Preserve]
	public class ElementEntryGeneratedFormatter : IYamlFormatter<ElementEntry?>, IYamlFormatter
	{
		private static readonly byte[] elementIdKeyUtf8Bytes = new byte[9] { 101, 108, 101, 109, 101, 110, 116, 73, 100 };

		private static readonly byte[] specificHeatCapacityKeyUtf8Bytes = new byte[20]
		{
			115, 112, 101, 99, 105, 102, 105, 99, 72, 101,
			97, 116, 67, 97, 112, 97, 99, 105, 116, 121
		};

		private static readonly byte[] thermalConductivityKeyUtf8Bytes = new byte[19]
		{
			116, 104, 101, 114, 109, 97, 108, 67, 111, 110,
			100, 117, 99, 116, 105, 118, 105, 116, 121
		};

		private static readonly byte[] solidSurfaceAreaMultiplierKeyUtf8Bytes = new byte[26]
		{
			115, 111, 108, 105, 100, 83, 117, 114, 102, 97,
			99, 101, 65, 114, 101, 97, 77, 117, 108, 116,
			105, 112, 108, 105, 101, 114
		};

		private static readonly byte[] liquidSurfaceAreaMultiplierKeyUtf8Bytes = new byte[27]
		{
			108, 105, 113, 117, 105, 100, 83, 117, 114, 102,
			97, 99, 101, 65, 114, 101, 97, 77, 117, 108,
			116, 105, 112, 108, 105, 101, 114
		};

		private static readonly byte[] gasSurfaceAreaMultiplierKeyUtf8Bytes = new byte[24]
		{
			103, 97, 115, 83, 117, 114, 102, 97, 99, 101,
			65, 114, 101, 97, 77, 117, 108, 116, 105, 112,
			108, 105, 101, 114
		};

		private static readonly byte[] defaultMassKeyUtf8Bytes = new byte[11]
		{
			100, 101, 102, 97, 117, 108, 116, 77, 97, 115,
			115
		};

		private static readonly byte[] defaultTemperatureKeyUtf8Bytes = new byte[18]
		{
			100, 101, 102, 97, 117, 108, 116, 84, 101, 109,
			112, 101, 114, 97, 116, 117, 114, 101
		};

		private static readonly byte[] defaultPressureKeyUtf8Bytes = new byte[15]
		{
			100, 101, 102, 97, 117, 108, 116, 80, 114, 101,
			115, 115, 117, 114, 101
		};

		private static readonly byte[] molarMassKeyUtf8Bytes = new byte[9] { 109, 111, 108, 97, 114, 77, 97, 115, 115 };

		private static readonly byte[] lightAbsorptionFactorKeyUtf8Bytes = new byte[21]
		{
			108, 105, 103, 104, 116, 65, 98, 115, 111, 114,
			112, 116, 105, 111, 110, 70, 97, 99, 116, 111,
			114
		};

		private static readonly byte[] radiationAbsorptionFactorKeyUtf8Bytes = new byte[25]
		{
			114, 97, 100, 105, 97, 116, 105, 111, 110, 65,
			98, 115, 111, 114, 112, 116, 105, 111, 110, 70,
			97, 99, 116, 111, 114
		};

		private static readonly byte[] radiationPer1000MassKeyUtf8Bytes = new byte[20]
		{
			114, 97, 100, 105, 97, 116, 105, 111, 110, 80,
			101, 114, 49, 48, 48, 48, 77, 97, 115, 115
		};

		private static readonly byte[] lowTempTransitionTargetKeyUtf8Bytes = new byte[23]
		{
			108, 111, 119, 84, 101, 109, 112, 84, 114, 97,
			110, 115, 105, 116, 105, 111, 110, 84, 97, 114,
			103, 101, 116
		};

		private static readonly byte[] lowTempKeyUtf8Bytes = new byte[7] { 108, 111, 119, 84, 101, 109, 112 };

		private static readonly byte[] highTempTransitionTargetKeyUtf8Bytes = new byte[24]
		{
			104, 105, 103, 104, 84, 101, 109, 112, 84, 114,
			97, 110, 115, 105, 116, 105, 111, 110, 84, 97,
			114, 103, 101, 116
		};

		private static readonly byte[] highTempKeyUtf8Bytes = new byte[8] { 104, 105, 103, 104, 84, 101, 109, 112 };

		private static readonly byte[] lowTempTransitionOreIdKeyUtf8Bytes = new byte[22]
		{
			108, 111, 119, 84, 101, 109, 112, 84, 114, 97,
			110, 115, 105, 116, 105, 111, 110, 79, 114, 101,
			73, 100
		};

		private static readonly byte[] lowTempTransitionOreMassConversionKeyUtf8Bytes = new byte[34]
		{
			108, 111, 119, 84, 101, 109, 112, 84, 114, 97,
			110, 115, 105, 116, 105, 111, 110, 79, 114, 101,
			77, 97, 115, 115, 67, 111, 110, 118, 101, 114,
			115, 105, 111, 110
		};

		private static readonly byte[] highTempTransitionOreIdKeyUtf8Bytes = new byte[23]
		{
			104, 105, 103, 104, 84, 101, 109, 112, 84, 114,
			97, 110, 115, 105, 116, 105, 111, 110, 79, 114,
			101, 73, 100
		};

		private static readonly byte[] highTempTransitionOreMassConversionKeyUtf8Bytes = new byte[35]
		{
			104, 105, 103, 104, 84, 101, 109, 112, 84, 114,
			97, 110, 115, 105, 116, 105, 111, 110, 79, 114,
			101, 77, 97, 115, 115, 67, 111, 110, 118, 101,
			114, 115, 105, 111, 110
		};

		private static readonly byte[] sublimateIdKeyUtf8Bytes = new byte[11]
		{
			115, 117, 98, 108, 105, 109, 97, 116, 101, 73,
			100
		};

		private static readonly byte[] sublimateFxKeyUtf8Bytes = new byte[11]
		{
			115, 117, 98, 108, 105, 109, 97, 116, 101, 70,
			120
		};

		private static readonly byte[] sublimateRateKeyUtf8Bytes = new byte[13]
		{
			115, 117, 98, 108, 105, 109, 97, 116, 101, 82,
			97, 116, 101
		};

		private static readonly byte[] sublimateEfficiencyKeyUtf8Bytes = new byte[19]
		{
			115, 117, 98, 108, 105, 109, 97, 116, 101, 69,
			102, 102, 105, 99, 105, 101, 110, 99, 121
		};

		private static readonly byte[] sublimateProbabilityKeyUtf8Bytes = new byte[20]
		{
			115, 117, 98, 108, 105, 109, 97, 116, 101, 80,
			114, 111, 98, 97, 98, 105, 108, 105, 116, 121
		};

		private static readonly byte[] offGasPercentageKeyUtf8Bytes = new byte[16]
		{
			111, 102, 102, 71, 97, 115, 80, 101, 114, 99,
			101, 110, 116, 97, 103, 101
		};

		private static readonly byte[] materialCategoryKeyUtf8Bytes = new byte[16]
		{
			109, 97, 116, 101, 114, 105, 97, 108, 67, 97,
			116, 101, 103, 111, 114, 121
		};

		private static readonly byte[] tagsKeyUtf8Bytes = new byte[4] { 116, 97, 103, 115 };

		private static readonly byte[] isDisabledKeyUtf8Bytes = new byte[10] { 105, 115, 68, 105, 115, 97, 98, 108, 101, 100 };

		private static readonly byte[] strengthKeyUtf8Bytes = new byte[8] { 115, 116, 114, 101, 110, 103, 116, 104 };

		private static readonly byte[] maxMassKeyUtf8Bytes = new byte[7] { 109, 97, 120, 77, 97, 115, 115 };

		private static readonly byte[] hardnessKeyUtf8Bytes = new byte[8] { 104, 97, 114, 100, 110, 101, 115, 115 };

		private static readonly byte[] toxicityKeyUtf8Bytes = new byte[8] { 116, 111, 120, 105, 99, 105, 116, 121 };

		private static readonly byte[] liquidCompressionKeyUtf8Bytes = new byte[17]
		{
			108, 105, 113, 117, 105, 100, 67, 111, 109, 112,
			114, 101, 115, 115, 105, 111, 110
		};

		private static readonly byte[] speedKeyUtf8Bytes = new byte[5] { 115, 112, 101, 101, 100 };

		private static readonly byte[] minHorizontalFlowKeyUtf8Bytes = new byte[17]
		{
			109, 105, 110, 72, 111, 114, 105, 122, 111, 110,
			116, 97, 108, 70, 108, 111, 119
		};

		private static readonly byte[] minVerticalFlowKeyUtf8Bytes = new byte[15]
		{
			109, 105, 110, 86, 101, 114, 116, 105, 99, 97,
			108, 70, 108, 111, 119
		};

		private static readonly byte[] convertIdKeyUtf8Bytes = new byte[9] { 99, 111, 110, 118, 101, 114, 116, 73, 100 };

		private static readonly byte[] flowKeyUtf8Bytes = new byte[4] { 102, 108, 111, 119 };

		private static readonly byte[] buildMenuSortKeyUtf8Bytes = new byte[13]
		{
			98, 117, 105, 108, 100, 77, 101, 110, 117, 83,
			111, 114, 116
		};

		private static readonly byte[] stateKeyUtf8Bytes = new byte[5] { 115, 116, 97, 116, 101 };

		private static readonly byte[] localizationIDKeyUtf8Bytes = new byte[14]
		{
			108, 111, 99, 97, 108, 105, 122, 97, 116, 105,
			111, 110, 73, 68
		};

		private static readonly byte[] dlcIdKeyUtf8Bytes = new byte[5] { 100, 108, 99, 73, 100 };

		private static readonly byte[] refinedMetalTargetKeyUtf8Bytes = new byte[18]
		{
			114, 101, 102, 105, 110, 101, 100, 77, 101, 116,
			97, 108, 84, 97, 114, 103, 101, 116
		};

		private static readonly byte[] compositionKeyUtf8Bytes = new byte[11]
		{
			99, 111, 109, 112, 111, 115, 105, 116, 105, 111,
			110
		};

		private static readonly byte[] descriptionKeyUtf8Bytes = new byte[11]
		{
			100, 101, 115, 99, 114, 105, 112, 116, 105, 111,
			110
		};

		[Preserve]
		public void Serialize(ref Utf8YamlEmitter emitter, ElementEntry? value, YamlSerializationContext context)
		{
			if (value == null)
			{
				emitter.WriteNull();
				return;
			}
			emitter.BeginMapping();
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(elementIdKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(elementIdKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer, out int written);
				emitter.WriteScalar(threadStaticBuffer.AsSpan(0, written));
			}
			context.Serialize(ref emitter, value.elementId);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(specificHeatCapacityKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(specificHeatCapacityKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer2, out int written2);
				emitter.WriteScalar(threadStaticBuffer2.AsSpan(0, written2));
			}
			context.Serialize(ref emitter, value.specificHeatCapacity);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(thermalConductivityKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(thermalConductivityKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer3, out int written3);
				emitter.WriteScalar(threadStaticBuffer3.AsSpan(0, written3));
			}
			context.Serialize(ref emitter, value.thermalConductivity);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(solidSurfaceAreaMultiplierKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(solidSurfaceAreaMultiplierKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer4, out int written4);
				emitter.WriteScalar(threadStaticBuffer4.AsSpan(0, written4));
			}
			context.Serialize(ref emitter, value.solidSurfaceAreaMultiplier);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(liquidSurfaceAreaMultiplierKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(liquidSurfaceAreaMultiplierKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer5, out int written5);
				emitter.WriteScalar(threadStaticBuffer5.AsSpan(0, written5));
			}
			context.Serialize(ref emitter, value.liquidSurfaceAreaMultiplier);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(gasSurfaceAreaMultiplierKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(gasSurfaceAreaMultiplierKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer6, out int written6);
				emitter.WriteScalar(threadStaticBuffer6.AsSpan(0, written6));
			}
			context.Serialize(ref emitter, value.gasSurfaceAreaMultiplier);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(defaultMassKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(defaultMassKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer7, out int written7);
				emitter.WriteScalar(threadStaticBuffer7.AsSpan(0, written7));
			}
			context.Serialize(ref emitter, value.defaultMass);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(defaultTemperatureKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(defaultTemperatureKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer8, out int written8);
				emitter.WriteScalar(threadStaticBuffer8.AsSpan(0, written8));
			}
			context.Serialize(ref emitter, value.defaultTemperature);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(defaultPressureKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(defaultPressureKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer9, out int written9);
				emitter.WriteScalar(threadStaticBuffer9.AsSpan(0, written9));
			}
			context.Serialize(ref emitter, value.defaultPressure);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(molarMassKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(molarMassKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer10, out int written10);
				emitter.WriteScalar(threadStaticBuffer10.AsSpan(0, written10));
			}
			context.Serialize(ref emitter, value.molarMass);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(lightAbsorptionFactorKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(lightAbsorptionFactorKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer11, out int written11);
				emitter.WriteScalar(threadStaticBuffer11.AsSpan(0, written11));
			}
			context.Serialize(ref emitter, value.lightAbsorptionFactor);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(radiationAbsorptionFactorKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(radiationAbsorptionFactorKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer12, out int written12);
				emitter.WriteScalar(threadStaticBuffer12.AsSpan(0, written12));
			}
			context.Serialize(ref emitter, value.radiationAbsorptionFactor);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(radiationPer1000MassKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(radiationPer1000MassKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer13, out int written13);
				emitter.WriteScalar(threadStaticBuffer13.AsSpan(0, written13));
			}
			context.Serialize(ref emitter, value.radiationPer1000Mass);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(lowTempTransitionTargetKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(lowTempTransitionTargetKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer14, out int written14);
				emitter.WriteScalar(threadStaticBuffer14.AsSpan(0, written14));
			}
			context.Serialize(ref emitter, value.lowTempTransitionTarget);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(lowTempKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(lowTempKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer15, out int written15);
				emitter.WriteScalar(threadStaticBuffer15.AsSpan(0, written15));
			}
			context.Serialize(ref emitter, value.lowTemp);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(highTempTransitionTargetKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(highTempTransitionTargetKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer16, out int written16);
				emitter.WriteScalar(threadStaticBuffer16.AsSpan(0, written16));
			}
			context.Serialize(ref emitter, value.highTempTransitionTarget);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(highTempKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(highTempKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer17, out int written17);
				emitter.WriteScalar(threadStaticBuffer17.AsSpan(0, written17));
			}
			context.Serialize(ref emitter, value.highTemp);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(lowTempTransitionOreIdKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(lowTempTransitionOreIdKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer18, out int written18);
				emitter.WriteScalar(threadStaticBuffer18.AsSpan(0, written18));
			}
			context.Serialize(ref emitter, value.lowTempTransitionOreId);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(lowTempTransitionOreMassConversionKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(lowTempTransitionOreMassConversionKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer19, out int written19);
				emitter.WriteScalar(threadStaticBuffer19.AsSpan(0, written19));
			}
			context.Serialize(ref emitter, value.lowTempTransitionOreMassConversion);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(highTempTransitionOreIdKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(highTempTransitionOreIdKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer20, out int written20);
				emitter.WriteScalar(threadStaticBuffer20.AsSpan(0, written20));
			}
			context.Serialize(ref emitter, value.highTempTransitionOreId);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(highTempTransitionOreMassConversionKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(highTempTransitionOreMassConversionKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer21, out int written21);
				emitter.WriteScalar(threadStaticBuffer21.AsSpan(0, written21));
			}
			context.Serialize(ref emitter, value.highTempTransitionOreMassConversion);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(sublimateIdKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(sublimateIdKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer22, out int written22);
				emitter.WriteScalar(threadStaticBuffer22.AsSpan(0, written22));
			}
			context.Serialize(ref emitter, value.sublimateId);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(sublimateFxKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(sublimateFxKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer23, out int written23);
				emitter.WriteScalar(threadStaticBuffer23.AsSpan(0, written23));
			}
			context.Serialize(ref emitter, value.sublimateFx);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(sublimateRateKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(sublimateRateKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer24, out int written24);
				emitter.WriteScalar(threadStaticBuffer24.AsSpan(0, written24));
			}
			context.Serialize(ref emitter, value.sublimateRate);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(sublimateEfficiencyKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(sublimateEfficiencyKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer25, out int written25);
				emitter.WriteScalar(threadStaticBuffer25.AsSpan(0, written25));
			}
			context.Serialize(ref emitter, value.sublimateEfficiency);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(sublimateProbabilityKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(sublimateProbabilityKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer26, out int written26);
				emitter.WriteScalar(threadStaticBuffer26.AsSpan(0, written26));
			}
			context.Serialize(ref emitter, value.sublimateProbability);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(offGasPercentageKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(offGasPercentageKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer27, out int written27);
				emitter.WriteScalar(threadStaticBuffer27.AsSpan(0, written27));
			}
			context.Serialize(ref emitter, value.offGasPercentage);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(materialCategoryKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(materialCategoryKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer28, out int written28);
				emitter.WriteScalar(threadStaticBuffer28.AsSpan(0, written28));
			}
			context.Serialize(ref emitter, value.materialCategory);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(tagsKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(tagsKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer29, out int written29);
				emitter.WriteScalar(threadStaticBuffer29.AsSpan(0, written29));
			}
			context.Serialize(ref emitter, value.tags);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(isDisabledKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(isDisabledKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer30, out int written30);
				emitter.WriteScalar(threadStaticBuffer30.AsSpan(0, written30));
			}
			context.Serialize(ref emitter, value.isDisabled);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(strengthKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(strengthKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer31, out int written31);
				emitter.WriteScalar(threadStaticBuffer31.AsSpan(0, written31));
			}
			context.Serialize(ref emitter, value.strength);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(maxMassKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(maxMassKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer32, out int written32);
				emitter.WriteScalar(threadStaticBuffer32.AsSpan(0, written32));
			}
			context.Serialize(ref emitter, value.maxMass);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(hardnessKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(hardnessKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer33, out int written33);
				emitter.WriteScalar(threadStaticBuffer33.AsSpan(0, written33));
			}
			context.Serialize(ref emitter, value.hardness);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(toxicityKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(toxicityKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer34, out int written34);
				emitter.WriteScalar(threadStaticBuffer34.AsSpan(0, written34));
			}
			context.Serialize(ref emitter, value.toxicity);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(liquidCompressionKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(liquidCompressionKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer35, out int written35);
				emitter.WriteScalar(threadStaticBuffer35.AsSpan(0, written35));
			}
			context.Serialize(ref emitter, value.liquidCompression);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(speedKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(speedKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer36, out int written36);
				emitter.WriteScalar(threadStaticBuffer36.AsSpan(0, written36));
			}
			context.Serialize(ref emitter, value.speed);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(minHorizontalFlowKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(minHorizontalFlowKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer37, out int written37);
				emitter.WriteScalar(threadStaticBuffer37.AsSpan(0, written37));
			}
			context.Serialize(ref emitter, value.minHorizontalFlow);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(minVerticalFlowKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(minVerticalFlowKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer38, out int written38);
				emitter.WriteScalar(threadStaticBuffer38.AsSpan(0, written38));
			}
			context.Serialize(ref emitter, value.minVerticalFlow);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(convertIdKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(convertIdKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer39, out int written39);
				emitter.WriteScalar(threadStaticBuffer39.AsSpan(0, written39));
			}
			context.Serialize(ref emitter, value.convertId);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(flowKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(flowKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer40, out int written40);
				emitter.WriteScalar(threadStaticBuffer40.AsSpan(0, written40));
			}
			context.Serialize(ref emitter, value.flow);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(buildMenuSortKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(buildMenuSortKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer41, out int written41);
				emitter.WriteScalar(threadStaticBuffer41.AsSpan(0, written41));
			}
			context.Serialize(ref emitter, value.buildMenuSort);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(stateKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(stateKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer42, out int written42);
				emitter.WriteScalar(threadStaticBuffer42.AsSpan(0, written42));
			}
			context.Serialize(ref emitter, value.state);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(localizationIDKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(localizationIDKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer43, out int written43);
				emitter.WriteScalar(threadStaticBuffer43.AsSpan(0, written43));
			}
			context.Serialize(ref emitter, value.localizationID);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(dlcIdKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(dlcIdKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer44, out int written44);
				emitter.WriteScalar(threadStaticBuffer44.AsSpan(0, written44));
			}
			context.Serialize(ref emitter, value.dlcId);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(refinedMetalTargetKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(refinedMetalTargetKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer45, out int written45);
				emitter.WriteScalar(threadStaticBuffer45.AsSpan(0, written45));
			}
			context.Serialize(ref emitter, value.refinedMetalTarget);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(compositionKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(compositionKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer46, out int written46);
				emitter.WriteScalar(threadStaticBuffer46.AsSpan(0, written46));
			}
			context.Serialize(ref emitter, value.composition);
			if (context.Options.NamingConvention == NamingConvention.LowerCamelCase)
			{
				emitter.WriteScalar(descriptionKeyUtf8Bytes);
			}
			else
			{
				NamingConventionMutator.MutateToThreadStaticBufferUtf8(descriptionKeyUtf8Bytes, context.Options.NamingConvention, out byte[] threadStaticBuffer47, out int written47);
				emitter.WriteScalar(threadStaticBuffer47.AsSpan(0, written47));
			}
			context.Serialize(ref emitter, value.description);
			emitter.EndMapping();
		}

		[Preserve]
		public ElementEntry? Deserialize(ref YamlParser parser, YamlDeserializationContext context)
		{
			if (parser.IsNullScalar())
			{
				parser.Read();
				return null;
			}
			parser.ReadWithVerify(ParseEventType.MappingStart);
			string elementId = null;
			float specificHeatCapacity = 0f;
			float thermalConductivity = 0f;
			float solidSurfaceAreaMultiplier = 0f;
			float liquidSurfaceAreaMultiplier = 0f;
			float gasSurfaceAreaMultiplier = 0f;
			float defaultMass = 0f;
			float defaultTemperature = 0f;
			float defaultPressure = 0f;
			float molarMass = 0f;
			float lightAbsorptionFactor = 0f;
			float radiationAbsorptionFactor = 0f;
			float radiationPer1000Mass = 0f;
			string lowTempTransitionTarget = null;
			float? lowTemp = null;
			string highTempTransitionTarget = null;
			float? highTemp = null;
			string lowTempTransitionOreId = null;
			float lowTempTransitionOreMassConversion = 0f;
			string highTempTransitionOreId = null;
			float highTempTransitionOreMassConversion = 0f;
			string sublimateId = null;
			string sublimateFx = null;
			float sublimateRate = 0f;
			float sublimateEfficiency = 0f;
			float sublimateProbability = 0f;
			float offGasPercentage = 0f;
			string materialCategory = null;
			string[] tags = null;
			bool isDisabled = false;
			float strength = 0f;
			float maxMass = 0f;
			byte hardness = 0;
			float toxicity = 0f;
			float liquidCompression = 0f;
			float speed = 0f;
			float minHorizontalFlow = 0f;
			float minVerticalFlow = 0f;
			string convertId = null;
			float flow = 0f;
			int buildMenuSort = 0;
			Element.State state = Element.State.Vacuum;
			string localizationID = null;
			string dlcId = null;
			string refinedMetalTarget = null;
			ElementComposition[] composition = null;
			string description = null;
			while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)
			{
				if (parser.CurrentEventType != ParseEventType.Scalar)
				{
					throw new YamlSerializerException(parser.CurrentMark, "Custom type deserialization supports only string key");
				}
				if (!parser.TryGetScalarAsSpan(out var span))
				{
					throw new YamlSerializerException(parser.CurrentMark, "Custom type deserialization supports only string key");
				}
				if (context.Options.NamingConvention != NamingConvention.LowerCamelCase)
				{
					NamingConventionMutator.MutateToThreadStaticBufferUtf8(span, NamingConvention.LowerCamelCase, out byte[] threadStaticBuffer, out int written);
					span = threadStaticBuffer.AsSpan(0, written);
				}
				switch (span.Length)
				{
				case 9:
					if (span.SequenceEqual(elementIdKeyUtf8Bytes))
					{
						parser.Read();
						elementId = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					if (span.SequenceEqual(molarMassKeyUtf8Bytes))
					{
						parser.Read();
						molarMass = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(convertIdKeyUtf8Bytes))
					{
						parser.Read();
						convertId = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					break;
				case 20:
					if (span.SequenceEqual(specificHeatCapacityKeyUtf8Bytes))
					{
						parser.Read();
						specificHeatCapacity = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(radiationPer1000MassKeyUtf8Bytes))
					{
						parser.Read();
						radiationPer1000Mass = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(sublimateProbabilityKeyUtf8Bytes))
					{
						parser.Read();
						sublimateProbability = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 19:
					if (span.SequenceEqual(thermalConductivityKeyUtf8Bytes))
					{
						parser.Read();
						thermalConductivity = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(sublimateEfficiencyKeyUtf8Bytes))
					{
						parser.Read();
						sublimateEfficiency = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 26:
					if (span.SequenceEqual(solidSurfaceAreaMultiplierKeyUtf8Bytes))
					{
						parser.Read();
						solidSurfaceAreaMultiplier = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 27:
					if (span.SequenceEqual(liquidSurfaceAreaMultiplierKeyUtf8Bytes))
					{
						parser.Read();
						liquidSurfaceAreaMultiplier = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 24:
					if (span.SequenceEqual(gasSurfaceAreaMultiplierKeyUtf8Bytes))
					{
						parser.Read();
						gasSurfaceAreaMultiplier = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(highTempTransitionTargetKeyUtf8Bytes))
					{
						parser.Read();
						highTempTransitionTarget = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					break;
				case 11:
					if (span.SequenceEqual(defaultMassKeyUtf8Bytes))
					{
						parser.Read();
						defaultMass = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(sublimateIdKeyUtf8Bytes))
					{
						parser.Read();
						sublimateId = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					if (span.SequenceEqual(sublimateFxKeyUtf8Bytes))
					{
						parser.Read();
						sublimateFx = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					if (span.SequenceEqual(compositionKeyUtf8Bytes))
					{
						parser.Read();
						composition = context.DeserializeWithAlias<ElementComposition[]>(ref parser);
						continue;
					}
					if (span.SequenceEqual(descriptionKeyUtf8Bytes))
					{
						parser.Read();
						description = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					break;
				case 18:
					if (span.SequenceEqual(defaultTemperatureKeyUtf8Bytes))
					{
						parser.Read();
						defaultTemperature = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(refinedMetalTargetKeyUtf8Bytes))
					{
						parser.Read();
						refinedMetalTarget = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					break;
				case 15:
					if (span.SequenceEqual(defaultPressureKeyUtf8Bytes))
					{
						parser.Read();
						defaultPressure = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(minVerticalFlowKeyUtf8Bytes))
					{
						parser.Read();
						minVerticalFlow = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 21:
					if (span.SequenceEqual(lightAbsorptionFactorKeyUtf8Bytes))
					{
						parser.Read();
						lightAbsorptionFactor = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 25:
					if (span.SequenceEqual(radiationAbsorptionFactorKeyUtf8Bytes))
					{
						parser.Read();
						radiationAbsorptionFactor = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 23:
					if (span.SequenceEqual(lowTempTransitionTargetKeyUtf8Bytes))
					{
						parser.Read();
						lowTempTransitionTarget = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					if (span.SequenceEqual(highTempTransitionOreIdKeyUtf8Bytes))
					{
						parser.Read();
						highTempTransitionOreId = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					break;
				case 7:
					if (span.SequenceEqual(lowTempKeyUtf8Bytes))
					{
						parser.Read();
						lowTemp = context.DeserializeWithAlias<float?>(ref parser);
						continue;
					}
					if (span.SequenceEqual(maxMassKeyUtf8Bytes))
					{
						parser.Read();
						maxMass = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 8:
					if (span.SequenceEqual(highTempKeyUtf8Bytes))
					{
						parser.Read();
						highTemp = context.DeserializeWithAlias<float?>(ref parser);
						continue;
					}
					if (span.SequenceEqual(strengthKeyUtf8Bytes))
					{
						parser.Read();
						strength = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(hardnessKeyUtf8Bytes))
					{
						parser.Read();
						hardness = context.DeserializeWithAlias<byte>(ref parser);
						continue;
					}
					if (span.SequenceEqual(toxicityKeyUtf8Bytes))
					{
						parser.Read();
						toxicity = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 22:
					if (span.SequenceEqual(lowTempTransitionOreIdKeyUtf8Bytes))
					{
						parser.Read();
						lowTempTransitionOreId = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					break;
				case 34:
					if (span.SequenceEqual(lowTempTransitionOreMassConversionKeyUtf8Bytes))
					{
						parser.Read();
						lowTempTransitionOreMassConversion = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 35:
					if (span.SequenceEqual(highTempTransitionOreMassConversionKeyUtf8Bytes))
					{
						parser.Read();
						highTempTransitionOreMassConversion = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 13:
					if (span.SequenceEqual(sublimateRateKeyUtf8Bytes))
					{
						parser.Read();
						sublimateRate = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(buildMenuSortKeyUtf8Bytes))
					{
						parser.Read();
						buildMenuSort = context.DeserializeWithAlias<int>(ref parser);
						continue;
					}
					break;
				case 16:
					if (span.SequenceEqual(offGasPercentageKeyUtf8Bytes))
					{
						parser.Read();
						offGasPercentage = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(materialCategoryKeyUtf8Bytes))
					{
						parser.Read();
						materialCategory = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					break;
				case 4:
					if (span.SequenceEqual(tagsKeyUtf8Bytes))
					{
						parser.Read();
						tags = context.DeserializeWithAlias<string[]>(ref parser);
						continue;
					}
					if (span.SequenceEqual(flowKeyUtf8Bytes))
					{
						parser.Read();
						flow = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 10:
					if (span.SequenceEqual(isDisabledKeyUtf8Bytes))
					{
						parser.Read();
						isDisabled = context.DeserializeWithAlias<bool>(ref parser);
						continue;
					}
					break;
				case 17:
					if (span.SequenceEqual(liquidCompressionKeyUtf8Bytes))
					{
						parser.Read();
						liquidCompression = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(minHorizontalFlowKeyUtf8Bytes))
					{
						parser.Read();
						minHorizontalFlow = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					break;
				case 5:
					if (span.SequenceEqual(speedKeyUtf8Bytes))
					{
						parser.Read();
						speed = context.DeserializeWithAlias<float>(ref parser);
						continue;
					}
					if (span.SequenceEqual(stateKeyUtf8Bytes))
					{
						parser.Read();
						state = context.DeserializeWithAlias<Element.State>(ref parser);
						continue;
					}
					if (span.SequenceEqual(dlcIdKeyUtf8Bytes))
					{
						parser.Read();
						dlcId = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					break;
				case 14:
					if (span.SequenceEqual(localizationIDKeyUtf8Bytes))
					{
						parser.Read();
						localizationID = context.DeserializeWithAlias<string>(ref parser);
						continue;
					}
					break;
				}
				parser.Read();
				parser.SkipCurrentNode();
			}
			parser.ReadWithVerify(ParseEventType.MappingEnd);
			return new ElementEntry
			{
				elementId = elementId,
				specificHeatCapacity = specificHeatCapacity,
				thermalConductivity = thermalConductivity,
				solidSurfaceAreaMultiplier = solidSurfaceAreaMultiplier,
				liquidSurfaceAreaMultiplier = liquidSurfaceAreaMultiplier,
				gasSurfaceAreaMultiplier = gasSurfaceAreaMultiplier,
				defaultMass = defaultMass,
				defaultTemperature = defaultTemperature,
				defaultPressure = defaultPressure,
				molarMass = molarMass,
				lightAbsorptionFactor = lightAbsorptionFactor,
				radiationAbsorptionFactor = radiationAbsorptionFactor,
				radiationPer1000Mass = radiationPer1000Mass,
				lowTempTransitionTarget = lowTempTransitionTarget,
				lowTemp = lowTemp,
				highTempTransitionTarget = highTempTransitionTarget,
				highTemp = highTemp,
				lowTempTransitionOreId = lowTempTransitionOreId,
				lowTempTransitionOreMassConversion = lowTempTransitionOreMassConversion,
				highTempTransitionOreId = highTempTransitionOreId,
				highTempTransitionOreMassConversion = highTempTransitionOreMassConversion,
				sublimateId = sublimateId,
				sublimateFx = sublimateFx,
				sublimateRate = sublimateRate,
				sublimateEfficiency = sublimateEfficiency,
				sublimateProbability = sublimateProbability,
				offGasPercentage = offGasPercentage,
				materialCategory = materialCategory,
				tags = tags,
				isDisabled = isDisabled,
				strength = strength,
				maxMass = maxMass,
				hardness = hardness,
				toxicity = toxicity,
				liquidCompression = liquidCompression,
				speed = speed,
				minHorizontalFlow = minHorizontalFlow,
				minVerticalFlow = minVerticalFlow,
				convertId = convertId,
				flow = flow,
				buildMenuSort = buildMenuSort,
				state = state,
				localizationID = localizationID,
				dlcId = dlcId,
				refinedMetalTarget = refinedMetalTarget,
				composition = composition,
				description = description
			};
		}
	}

	private string description_backing;

	public string elementId { get; set; }

	public float specificHeatCapacity { get; set; }

	public float thermalConductivity { get; set; }

	public float solidSurfaceAreaMultiplier { get; set; }

	public float liquidSurfaceAreaMultiplier { get; set; }

	public float gasSurfaceAreaMultiplier { get; set; }

	public float defaultMass { get; set; }

	public float defaultTemperature { get; set; }

	public float defaultPressure { get; set; }

	public float molarMass { get; set; }

	public float lightAbsorptionFactor { get; set; }

	public float radiationAbsorptionFactor { get; set; }

	public float radiationPer1000Mass { get; set; }

	public string lowTempTransitionTarget { get; set; }

	public float? lowTemp { get; set; }

	public string highTempTransitionTarget { get; set; }

	public float? highTemp { get; set; }

	public string lowTempTransitionOreId { get; set; }

	public float lowTempTransitionOreMassConversion { get; set; }

	public string highTempTransitionOreId { get; set; }

	public float highTempTransitionOreMassConversion { get; set; }

	public string sublimateId { get; set; }

	public string sublimateFx { get; set; }

	public float sublimateRate { get; set; }

	public float sublimateEfficiency { get; set; }

	public float sublimateProbability { get; set; }

	public float offGasPercentage { get; set; }

	public string materialCategory { get; set; }

	public string[] tags { get; set; }

	public bool isDisabled { get; set; }

	public float strength { get; set; }

	public float maxMass { get; set; }

	public byte hardness { get; set; }

	public float toxicity { get; set; }

	public float liquidCompression { get; set; }

	public float speed { get; set; }

	public float minHorizontalFlow { get; set; }

	public float minVerticalFlow { get; set; }

	public string convertId { get; set; }

	public float flow { get; set; }

	public int buildMenuSort { get; set; }

	public Element.State state { get; set; }

	public string localizationID { get; set; }

	public string dlcId { get; set; }

	public string refinedMetalTarget { get; set; }

	public ElementComposition[] composition { get; set; }

	public string description
	{
		get
		{
			return description_backing ?? ("STRINGS.ELEMENTS." + elementId.ToString().ToUpper() + ".DESC");
		}
		set
		{
			description_backing = value;
		}
	}

	[Preserve]
	public static void __RegisterVYamlFormatter()
	{
		GeneratedResolver.Register(new ElementEntryGeneratedFormatter());
	}
}
