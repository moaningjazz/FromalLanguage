using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FromalLanguage
{
    public class Automation
    {
        private string code;

        private string startState;
        private string finishState;

        private string currentState;

        private bool isUsingRegex;

        public int CountSkipSymbols { get; set; }

        private List<(string StateOut, IList<char> Value, string StateIn)> matrix = new List<(string StateOut, IList<char> Value, string StateIn)>();
        private List<(string StateOut, string RegexValue, string StateIn)> matrixRegex = new List<(string StateOut, string RegexValue, string StateIn)>();

        public Automation(string code, JObject json)
        {
            if (json["IsRegexRecognize"].Value<bool>())
            {
                foreach (var matrixRow in json["Matrix"].AsJEnumerable())
                {
                    string stateOut = matrixRow["StateOut"].Value<string>();
                    string stateIn = matrixRow["StateIn"].Value<string>();
                    string value = matrixRow["Value"].Value<string>();
                    matrixRegex.Add((stateOut, value, stateIn));
                }
                isUsingRegex = true;
            }
            else
            {
                foreach (var matrixRow in json["Matrix"].AsJEnumerable())
                {
                    string stateOut = matrixRow["StateOut"].Value<string>();
                    string stateIn = matrixRow["StateIn"].Value<string>();
                    IList<char> value = matrixRow["Value"].ToObject<List<char>>();
                    matrix.Add((stateOut, value, stateIn));
                }
            }

            startState = json["StartState"].Value<string>();
            finishState = json["FinishState"].Value<string>();
            this.code = code;
        }

        public (bool State, int CountRecognizeSymbols) Max()
        {
            currentState = startState;
            if (isUsingRegex)
            {
                int countRecognizeSymbols = 0;

                while (true)
                {
                    if (RegexRecognizeSymbol(code, CountSkipSymbols))
                    {
                        countRecognizeSymbols++;
                        CountSkipSymbols++;
                    }
                    else
                        break;
                }

                if (countRecognizeSymbols > 0 && 
                    currentState == finishState)
                    return (true, countRecognizeSymbols);

                else
                    return (false, countRecognizeSymbols);
            }
            else
            {
                int countRecognizeSymbols = 0;
                while (currentState != finishState)
                {
                    if (RecognizeSymbol(code, CountSkipSymbols))
                    {
                        countRecognizeSymbols++;
                        CountSkipSymbols++;
                    }
                    else
                        return (false, countRecognizeSymbols);

                }

                return (true, countRecognizeSymbols);
            }
        }

        private bool RecognizeSymbol(string code, int countSkipSymbols)
        {
            var currentInformationForRecognize = matrix
                .ToList()
                .FindAll(e => e.StateOut == currentState).ToList();

            var state = currentInformationForRecognize
                .Find(e => e.Value.Contains(code[countSkipSymbols]));

            if (state != default)
            {
                currentState = state.StateIn;
                return true;
            }
            else
                return false;
        }

        private bool RegexRecognizeSymbol(string code, int countSkipSymbols)
        {
            if (countSkipSymbols == code.Length)
                return false;

            var currentInformationForRecognize = matrixRegex
                .ToList()
                .FindAll(e => e.StateOut == currentState).ToList();

            var state = currentInformationForRecognize
                .Find(e => Regex.IsMatch(code[countSkipSymbols].ToString(), e.RegexValue));

            if (state != default)
            {
                currentState = state.StateIn;
                return true;
            }
            else
                return false;
        }
    }
}
