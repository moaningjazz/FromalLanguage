using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FromalLanguage
{
    public class Recognizer
    {
        string fileIn;
        string fileOut;
        List<(Automation Automation, string Name)> automationAndNames = new List<(Automation, string)>();

        public Recognizer(string fileIn, string fileOut)
        {
            this.fileIn = fileIn;
            this.fileOut = fileOut;
        }

        public void Recognize()
        {
            string code;

            using (StreamReader reader = new StreamReader(fileIn))
                code = reader.ReadToEnd();

            RegisterAutomation(code);

            JArray resultJson = new JArray();

            int countSkipSymbols = 0;
            while (countSkipSymbols != code.Length)
            {
                bool isRecognize = false;
                foreach (var automation in automationAndNames)
                {
                    automation.Automation.CountSkipSymbols = countSkipSymbols;
                    var result = automation.Automation.Max();
                    if (result.State == true)
                    {
                        resultJson.Add(JToken.FromObject(
                            new KeyValuePair<string, string>(
                                automation.Name, 
                                code.Substring(
                                    countSkipSymbols, 
                                    result.CountRecognizeSymbols
                                    )
                                ))
                            );

                        countSkipSymbols += result.CountRecognizeSymbols;
                        isRecognize = true;
                        break;
                    }
                }

                if (!isRecognize)
                {
                    resultJson.Add(JToken.FromObject(
                        new KeyValuePair<string, string>(
                            "Error recognize", 
                            countSkipSymbols.ToString()
                            ))
                        );
                    break;
                }
            }

            using (StreamWriter writer = new StreamWriter(fileOut))
                writer.Write(resultJson.ToString());
        }

        private void RegisterAutomation(string code)
        {
            automationAndNames.Add((new Automation(code, ReadJson("intKeyword.json")), "IntKeyword"));
            automationAndNames.Add((new Automation(code, ReadJson("doubleKeyword.json")), "DoubleKeyword"));
            automationAndNames.Add((new Automation(code, ReadJson("whileKeyword.json")), "WhileKeyword"));
            automationAndNames.Add((new Automation(code, ReadJson("falseKeyword.json")), "FalseKeyword"));
            automationAndNames.Add((new Automation(code, ReadJson("whitespaces.json")), "Whitespaces"));
            automationAndNames.Add((new Automation(code, ReadJson("arithmeticOperation.json")), "ArithmeticOperation"));
            automationAndNames.Add((new Automation(code, ReadJson("logicOperation.json")), "LogicOperation"));
            automationAndNames.Add((new Automation(code, ReadJson("floatValue.json")), "FloatValue"));
            automationAndNames.Add((new Automation(code, ReadJson("intValue.json")), "IntValue"));
            automationAndNames.Add((new Automation(code, ReadJson("brackets.json")), "Brackets"));
            automationAndNames.Add((new Automation(code, ReadJson("comparisonOperator.json")), "ComparisonOperator"));
            automationAndNames.Add((new Automation(code, ReadJson("varNames.json")), "VarNames"));
            automationAndNames.Add((new Automation(code, ReadJson("serviceSymbol.json")), "ServiceSymbols"));
        }

        private JObject ReadJson(string filename)
        {
            string json;
            using (StreamReader reader = new StreamReader(filename))
            {
                json = reader.ReadToEnd();
            }
            return JObject.Parse(json);
        }


    }
}
