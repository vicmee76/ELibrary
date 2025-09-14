using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELibrary.Infrastruture.Models
{

    public class GutendexBooksResponse
    {
        public int count { get; set; }
        public string next { get; set; }
        public string previous { get; set; }
        public List<Result> results { get; set; }
    }

    

    public class Translator
    {
        public string name { get; set; }
        public int? birth_year { get; set; }
        public int? death_year { get; set; }
    }

}
