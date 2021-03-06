﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using EntryPoint.Common;
using System.Reflection;
using EntryPoint.Exceptions;

namespace EntryPoint.Arguments.OptionStrategies {
    internal static class ValueConverter {

        // Sanitise values before trying to store them
        public static object ConvertValue(object value, Type outputType) {
            if (value == null) {
                return value;
            }

            if (Nullable.GetUnderlyingType(outputType) != null) {
                return Convert.ChangeType(value, Nullable.GetUnderlyingType(outputType));
            }

            value = SanitiseSpecialTypes(value, outputType);

            if (value is IConvertible) {
                return ChangeType(value, outputType);
            } else if (value.GetType() == outputType) {
                return value;
            }

            throw new InvalidCastException(
                $"The requested type `{outputType.Name}` could not be converted to "
                + $"from the type: `{value.GetType().Name}` with value: `{value.ToString()}`");
        }

        static object ChangeType(object value, Type outputType) {
            try {
                return Convert.ChangeType(value, outputType);
            } catch (FormatException e) {
                throw new VariableTypeException(
                    $"`{value}` cannot be converted to [{outputType.Name}]");
            }
        }

        static object SanitiseSpecialTypes(object value, Type outputType) {
            if (outputType == typeof(bool)) {
                return DeserialiseBool(value);
            }
            if (outputType.BaseType() == typeof(Enum) || outputType == typeof(Enum)) {
                return DeserialiseEnum(value, outputType);
            }
            if (outputType.IsList()) {
                return DeserialiseList(value, outputType);
            }
            return value;
        }

        // Converts an int or string representation of a bool into a bool
        // todo: what about bool.TryParse(...)? probably more appropriate as it supports string representations natively
        static object DeserialiseBool(object value) {
            int v;
            if (int.TryParse(value.ToString(), out v)) {
                value = (v != 0);
            }
            return value;
        }

        // Converts an int or string representation of an application enum into that enum
        static object DeserialiseEnum(object value, Type outputType) {
            try {
                return Enum.Parse(outputType, value.ToString(), true);
            } catch (ArgumentException e) {
                throw new VariableTypeException(
                    $"`{value}` is not a value present in {outputType.Name}. " +
                    $"Valid options are: [{String.Join("|", Enum.GetNames(outputType))}]");
            }
        }

        // Split a serialised list by its delimiters 
        // and convert its values to its core generic type
        // Reflectively create and return the required list instance
        static object DeserialiseList(object serialisedList, Type listType) {
            var listInstance = Activator.CreateInstance(listType);

            Type listTypeArg = listType.GenericTypeArguments[0];
            MethodInfo listAddMethod = GetListAddMethod(listType, listTypeArg);

            string[] stringValues = serialisedList.ToString().Split(',');
            foreach (var stringValue in stringValues) {
                var value = ConvertValue(stringValue, listTypeArg);
                listAddMethod.Invoke(listInstance, new[] { value });
            }

            return listInstance;
        }

        static MethodInfo GetListAddMethod(Type listType, Type typeArg) {
            return listType
                .GetRuntimeMethod("Add", new Type[] { typeArg });
        }
    }
}
