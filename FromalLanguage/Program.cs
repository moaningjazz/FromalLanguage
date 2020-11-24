using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FromalLanguage
{
    class Program
    {
        static void Main(string[] args)
        {
            Recognizer recognizer = new Recognizer("code.txt", "result.json");
            recognizer.Recognize();
        }
    }
}
