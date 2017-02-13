using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotRaces.Models
{
    public class QuestionViewModel
    {
        public int SurveyID { get; set; }
        public int QuestionNum { get; set; }
        public List<Question> Questions { get; set; }
        public OSUAnswer Answer { get; set; }
    }
}
