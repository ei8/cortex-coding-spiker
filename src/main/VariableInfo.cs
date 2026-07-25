using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace ei8.Cortex.Coding.Spiker
{
    public class VariableInfo
    {
        public static bool TryParse(
            string parameterExpression,
            [NotNullWhen(true)]
            out VariableInfo? result
        )
        {
            bool bResult = false;
            result = null;

            if (VariableInfo.TryGetVariableName(parameterExpression, out var tempResult))
            {
                // capitalize first letter
                tempResult = tempResult[0].ToString().ToUpper() + tempResult.Substring(1);

                // operator open [type]___ eg. XOR___, NOT___
                // dot _
                // operands separator __ (comma)
                // operator close _z_ (optional)

                tempResult = tempResult
                    .Replace("_z_", ")")
                    .Replace("___", "(")
                    .Replace("__", ",")
                    .Replace("_", ".");

                if (tempResult.Count(r => r == ',') > 1)
                    throw new NotSupportedException("Nested operations are currently not supported.");

                // add closing parentheses based on number of unclosed open parentheses
                tempResult += new string(')', tempResult.Count(c => c == '(') - tempResult.Count(c => c == ')'));

                // TEST:
                // Result
                // half2.NOT(Half1.XOR.Result,Half1.XOR.Result)
                // half1.OUT(half1.NOT(Minuend))
                var match = Regex.Match(
                    tempResult,
                    @"
                    (
                       (?<Function>
                          [^(]+
                       )
                       [\(]
                    )?
                    (?<Input>
                        .+(?!$)[^\)]?
                    )
                    (\))?
                    ",
                    RegexOptions.Compiled |
                    RegexOptions.Multiline |
                    RegexOptions.IgnorePatternWhitespace
                );

                if (match.Success)
                {
                    result = new VariableInfo(
                        match.Groups["Function"].Value,
                        match.Groups["Input"].Value.Split(',')
                    );

                    bResult = true;
                }
            }

            return bResult;
        }

        public override string ToString()
        {
            string functionValue = !string.IsNullOrWhiteSpace(this.Function) ? this.Function + "(" : string.Empty,
                functionClose = functionValue.Length > 0 ? ")" : string.Empty;

            return $"{functionValue}{string.Join(',', this.Inputs)}{functionClose}";
        }

        public string Function { get; private set; }

        public IEnumerable<string> Inputs { get; private set; }

        public VariableInfo(string function, IEnumerable<string> inputs)
        {
            this.Function = function;
            this.Inputs = inputs;
        }

        private static bool TryGetVariableName(
            string parameterExpression,
            [NotNullWhen(true)] out string? result
        )
        {
            var bResult = false;
            result = null;
            if (!string.IsNullOrWhiteSpace(parameterExpression))
            {
                // if variableName contains type
                if (parameterExpression.Contains(' '))
                    // ... separate variable name from type 
                    result = parameterExpression.Split(' ').ElementAt(1);
                else
                    result = parameterExpression;

                bResult = true;
            }

            return bResult;
        }
    }
}
