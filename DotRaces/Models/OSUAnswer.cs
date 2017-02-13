using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotRaces.Models
{
    [Table("dotraces_OSUAnswers")]
    public class OSUAnswer
    {
        [Key]
        public int AnswerID { get; set; }
        public int SurveyID { get; set; }
        public int QuestionID { get; set; }
        public bool? AnswerFlag { get; set; }
        public int? AnswerValue { get; set; }
    }
}
