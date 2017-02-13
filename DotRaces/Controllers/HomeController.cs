﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotRaces.Models;
using DotRaces.DAL;
using System.Data.Entity;

namespace DotRaces.Controllers
{
    public class HomeController : Controller
    {

        DotRacesDataContext db = new DotRacesDataContext();
        private static Random random = new Random();

        public ActionResult Index()
        {
            OSUSurvey survey = new OSUSurvey();
            Separator separator = db.Separators.Find(1);
            SettingSet settings;
            if (separator.CurrentVersion == true)
            {
                settings = db.SettingSets.Find(1);
                separator.CurrentVersion = false;
                db.Entry(separator).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.Version = "UO";
            }
            else
            {
                settings = db.SettingSets.Find(2);
                separator.CurrentVersion = true;
                db.Entry(separator).State = EntityState.Modified;
                db.SaveChanges();
                ViewBag.Version = "OU";
            }

            survey.PointTotal = 0;
            survey.SettingsID = settings.SettingSetID;
            survey.CreatedDate = DateTime.Now;
            return View(survey);
        }

        [HttpPost]
        public ActionResult Index(OSUSurvey survey)
        {
            if(ModelState.IsValid)
            {
                db.Surveys.Add(survey);
                db.SaveChanges();

                return RedirectToAction("Introduction", new { id = survey.SurveyID });
            }
            return View(survey);
        }

        public ActionResult Introduction(int id)
        {
            OSUSurvey survey = db.Surveys.Find(id);

            return View(survey);
        }

        [HttpPost]
        public ActionResult Introduction(OSUSurvey survey)
        {
            return RedirectToAction("Rules", new { id = survey.SurveyID });
        }

        public ActionResult Rules(int id)
        {
            OSUSurvey survey = db.Surveys.Find(id);
            SettingSet settings = db.SettingSets.Find(survey.SettingsID);

            string lastRaceNoun = "";
            string lastRaceAdjective = "";

            string secondToLastRaceNoun = "";
            string secondToLastRaceAdjective = "";

            switch (settings.NumberOfRaces)
            {
                case 5:
                    lastRaceNoun = "five";
                    lastRaceAdjective = "fifth";
                    secondToLastRaceNoun = "four";
                    secondToLastRaceAdjective = "fourth";
                    break;
                case 7:
                    lastRaceNoun = "seven";
                    lastRaceAdjective = "seventh";
                    secondToLastRaceNoun = "six";
                    secondToLastRaceAdjective = "sixth";
                    break;
                case 9:
                    lastRaceNoun = "nine";
                    lastRaceAdjective = "ninth";
                    secondToLastRaceNoun = "eight";
                    secondToLastRaceAdjective = "eighth";
                    break;
                case 11:
                    lastRaceNoun = "eleven";
                    lastRaceAdjective = "eleventh";
                    secondToLastRaceNoun = "ten";
                    secondToLastRaceAdjective = "tenth";
                    break;
            }

            string multiplierVerb = "";

            switch (settings.LastRaceMultiplier)
            {
                case 2:
                    multiplierVerb = "double";
                    break;
                case 3:
                    multiplierVerb = "triple";
                    break;
                case 4:
                    multiplierVerb = "quadruple";
                    break;
            }

            ViewBag.MultiplierVerb = multiplierVerb;
            ViewBag.LastRaceNoun = lastRaceNoun;
            ViewBag.LastRaceAdjective = lastRaceAdjective;
            ViewBag.SecondToLastRaceNoun = secondToLastRaceNoun;
            ViewBag.SecondToLastRaceAdjective = secondToLastRaceAdjective;

            ViewBag.BeginTotal = settings.PotentialLowScore;
            ViewBag.MultipliedTotal = 2 * settings.PotentialLowScore;
            ViewBag.PointsPerRound = settings.PointsPerRound;
            ViewBag.GiftCertificateAmount = "35";
            
            return View(survey);
        }

        [HttpPost]
        public ActionResult Rules(OSUSurvey survey)
        {
            return RedirectToAction("Begin", new { id = survey.SurveyID });
        }

        public ActionResult Begin(int id)
        {
            OSUSurvey survey = db.Surveys.Find(id);

            return View(survey);
        }

        [HttpPost]
        public ActionResult Begin(OSUSurvey survey)
        {
            return RedirectToAction("Race", new { id = survey.SurveyID });
        }

        public ActionResult StartAudio()
        {
            var file = Server.MapPath("~/Content/Sounds/pistol.mp3");
            return File(file, "audio/mp3");
        }

        public ActionResult CrowdAudio()
        {
            var file = Server.MapPath("~/Content/Sounds/crowd.mp3");
            return File(file, "audio/mp3");
        }

        public ActionResult Cheer1Audio()
        {
            var file = Server.MapPath("~/Content/Sounds/cheer1.mp3");
            return File(file, "audio/mp3");
        }

        public ActionResult Cheer2Audio()
        {
            var file = Server.MapPath("~/Content/Sounds/cheer2.mp3");
            return File(file, "audio/mp3");
        }

        public ActionResult Cheer3Audio()
        {
            var file = Server.MapPath("~/Content/Sounds/cheer3.mp3");
            return File(file, "audio/mp3");
        }

        public ActionResult Race(int id, int? num)
        {
            RaceViewModel model = new RaceViewModel();
            OSUSurvey survey = db.Surveys.Find(id);
            SettingSet settings = db.SettingSets.Find(survey.SettingsID);

            // Get the Races

            List<Race> races = db.Races.Where(x => x.SettingSetID == survey.SettingsID).OrderBy(x => x.RaceNum).ToList();

            if(num == null)
            {
                // We are on the first race

                Race race = races.FirstOrDefault(x => x.RaceNum == 1);
                {
                    if(race == null)
                    {
                        // No first race, this is bad.
                    }
                    else
                    {
                        model.Race = race;
                        model.CurrentPoints = 0;
                        model.UOWins = 0;
                        model.OSUWins = 0;

                        if (race.Winner)
                        {
                            model.AfterPoints = 0;
                            model.AfterUOWins = 1;
                        }
                        else
                        {
                            model.AfterPoints = settings.PointsPerRound;
                            model.AfterOSUWins = 1;
                        }

                        model.RaceID = race.RaceID;
                        model.RaceNum = race.RaceNum;
                        model.SurveyID = survey.SurveyID;
                        return View(model);
                    }
                }
            }
            else
            {
                // We are not on the first race anymore

                Race lastRace = races.FirstOrDefault(x => x.RaceNum == num);

                int currentNum = (int)num + 1;

                if(currentNum < settings.NumberOfRaces)
                {
                    Race race = races.FirstOrDefault(x => x.RaceNum == currentNum);
                    if(race != null)
                    {
                        model.Race = race;

                        if (lastRace.Winner)
                        {
                            model.UOWins++;
                        }
                        else
                        {
                            if (survey.PointTotal == null)
                            {
                                survey.PointTotal = settings.PointsPerRound;
                                //model.UOWins++;
                            }
                            else
                            {
                                survey.PointTotal += settings.PointsPerRound;
                            }
                            model.OSUWins++;
                        }

                        db.Entry(survey).State = EntityState.Modified;
                        db.SaveChanges();

                        model.CurrentPoints = (int)survey.PointTotal;

                        if(race.Winner)
                        {
                            model.AfterPoints = model.CurrentPoints; 
                            model.AfterUOWins++;
                        }
                        else
                        {
                            model.AfterPoints = model.CurrentPoints + settings.PointsPerRound;
                            model.AfterOSUWins++;
                        }
                       
                        model.RaceID = race.RaceID;
                        model.RaceNum = currentNum;
                        model.SurveyID = survey.SurveyID;
                        return View(model);
                    }
                }
                else
                {

                    if (lastRace.Winner)
                    {
                        
                        model.UOWins++;
                    }
                    else
                    {
                        survey.PointTotal += settings.PointsPerRound;
                        model.OSUWins++;
                    }

                    db.Entry(survey).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Bet", new { id = survey.SurveyID });
                }
            }
            return View();

        }

        [HttpPost]
        public ActionResult Race(RaceViewModel model)
        {
            //return RedirectToAction("Race", new { id = model.SurveyID, num = model.RaceNum, tot = model.AfterPoints });
            //RaceViewModel model = new RaceViewModel();
            OSUSurvey survey = db.Surveys.Find(model.SurveyID);
            SettingSet settings = db.SettingSets.Find(survey.SettingsID);

            // Get the Races

            List<Race> races = db.Races.Where(x => x.SettingSetID == survey.SettingsID).OrderBy(x => x.RaceNum).ToList();

            Race lastRace = races.FirstOrDefault(x => x.RaceNum == model.RaceNum);

            int currentNum = (int)model.RaceNum + 1;

            if (currentNum < settings.NumberOfRaces)
            {
                Race race = races.FirstOrDefault(x => x.RaceNum == currentNum);
                if (race != null)
                {
                    model.Race = race;

                    if (lastRace.Winner)
                    {    
                        model.UOWins++;
                    }
                    else
                    {
                        if (survey.PointTotal == null)
                        {
                            survey.PointTotal = settings.PointsPerRound;
                            //model.UOWins++;
                        }
                        else
                        {
                            survey.PointTotal += settings.PointsPerRound;
                        }
                        model.OSUWins++;
                    }

                    db.Entry(survey).State = EntityState.Modified;
                    db.SaveChanges();

                    model.CurrentPoints = (int)survey.PointTotal;

                    if (race.Winner)
                    {
                        model.AfterPoints = model.CurrentPoints;
                        model.AfterUOWins++;
                    }
                    else
                    {
                        model.AfterPoints = model.CurrentPoints + settings.PointsPerRound;
                        model.AfterOSUWins++;
                    }

                    model.RaceID = race.RaceID;
                    model.RaceNum = currentNum;
                    model.SurveyID = survey.SurveyID;
                    ModelState.Clear();
                    return View(model);
                }
            }
            else
            {

                if (lastRace.Winner)
                {
                    
                    model.UOWins++;
                }
                else
                {
                    survey.PointTotal += settings.PointsPerRound;
                    model.OSUWins++;
                }

                survey.UOWins = model.UOWins;
                survey.OSUWins = model.OSUWins;
                db.Entry(survey).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Bet", new { id = survey.SurveyID });
            }
            return View(model);
        }

        public ActionResult Bet(int? id)
        {
            OSUSurvey survey = db.Surveys.Find(id);

            return View(survey);
        }

        [HttpPost]
        public ActionResult Bet(OSUSurvey survey)
        {
            if (survey.ChoseToBetFlag == null)
            {
                ModelState.AddModelError("ChoseToBetFlag", "You must indicate whether you would like to bet or not.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(survey).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("SurveyIntroduction", new { id = survey.SurveyID });
            }
            return View(survey);
        }

        public ActionResult SurveyIntroduction(int? id)
        {
            OSUSurvey survey = db.Surveys.Find(id);

            return View(survey);
        }

        [HttpPost]
        public ActionResult SurveyIntroduction(OSUSurvey survey)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Survey", new { id = survey.SurveyID });
            }
            return View(survey);
        }

        public ActionResult Survey(int? id)
        {
            QuestionViewModel model = new QuestionViewModel();

            OSUSurvey survey = db.Surveys.Find(id);
            if(survey != null)
            {
                SettingSet settings = db.SettingSets.Find(survey.SettingsID);

                List<Question> questions = db.Questions.Where(x => x.SettingSetID == settings.SettingSetID && x.QuestionRoundNum == 1).ToList();

                List<Question> shuffledQuestions = new List<Question>();

                int n = questions.Count();
                while (n >= 1)
                {
                    n--;
                    int k = random.Next(n + 1);
                    Question question = questions[k];
                    questions.Remove(question);
                    shuffledQuestions.Add(question);
                }

                model.Questions = shuffledQuestions;
                model.QuestionNum = 0;
                model.Questions[0].CurrentQuestionFlag = true;

                OSUAnswer answer = new OSUAnswer();
                answer.SurveyID = survey.SurveyID;
                answer.QuestionID = model.Questions[0].QuestionID;
                //answer.QuestionRoundNum = model.Questions[0].QuestionRoundNum;
                model.Answer = answer;
                model.SurveyID = survey.SurveyID;

            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Survey([Bind(Include = "SurveyID,QuestionNum,Questions,Answer")]QuestionViewModel model)
        {

            OSUSurvey survey = db.Surveys.Find(model.SurveyID);

            if (survey != null)
            {
                while (model.QuestionNum < (model.Questions.Count() - 1))
                {
                    Question currentQuestion = model.Questions[model.QuestionNum];
                    currentQuestion.AskedFlag = true;
                    currentQuestion.CurrentQuestionFlag = false;
                    int currentIndex = model.Questions.IndexOf(currentQuestion);
                    model.Questions[model.QuestionNum].CurrentQuestionFlag = false;
                    model.Questions[model.QuestionNum].AskedFlag = true;

                    model.QuestionNum = model.QuestionNum + 1;
                    model.Questions[model.QuestionNum].CurrentQuestionFlag = true;
                    Question nextQuestion = model.Questions[model.QuestionNum];
                    nextQuestion.CurrentQuestionFlag = true;
                    

                    OSUAnswer answer = new OSUAnswer();
                    answer.AnswerFlag = model.Answer.AnswerFlag;
                    answer.AnswerValue = model.Answer.AnswerValue;
                    //answer.QuestionRoundNum = 1;
                    answer.SurveyID = model.Answer.SurveyID;
                    answer.QuestionID = model.Answer.QuestionID;
                    db.Answers.Add(answer);
                    db.SaveChanges();

                    OSUAnswer newAnswer = new OSUAnswer();
                    newAnswer.SurveyID = survey.SurveyID;
                    newAnswer.QuestionID = nextQuestion.QuestionID;
                    //newAnswer.QuestionRoundNum = 1;
                    model.Answer = newAnswer;
                    ModelState.Clear();
                    return View(model);
                }

                OSUAnswer finalAnswer = new OSUAnswer();
                finalAnswer.AnswerFlag = model.Answer.AnswerFlag;
                finalAnswer.AnswerValue = model.Answer.AnswerValue;
                finalAnswer.SurveyID = model.Answer.SurveyID;
                finalAnswer.QuestionID = model.Answer.QuestionID;
                //finalAnswer.QuestionRoundNum = model.Answer.QuestionRoundNum;
                db.Answers.Add(finalAnswer);
                db.SaveChanges();

                return RedirectToAction("FinalRaceIntro", new { id = model.SurveyID });
            }

            return View(model);
        }

        public ActionResult FinalRaceIntro(int? id)
        {
            OSUSurvey survey = db.Surveys.Find(id);
            return View(survey);
        }

        [HttpPost]
        public ActionResult FinalRaceIntro(OSUSurvey survey)
        {
            return RedirectToAction("FinalRace", new { id = survey.SurveyID });
        }

        public ActionResult FinalRace(int? id)
        {

            RaceViewModel model = new RaceViewModel();
            OSUSurvey survey = db.Surveys.Find(id);
            SettingSet settings = db.SettingSets.Find(survey.SettingsID);

            // Get the Final Race

            Race race = db.Races.FirstOrDefault(x => x.SettingSetID == survey.SettingsID && x.RaceNum == settings.NumberOfRaces);

            //List<Race> races = db.Races.Where(x => x.SettingSetID == survey.SettingsID).OrderBy(x => x.RaceNum).ToList();

            model.Race = race;
            model.CurrentPoints = (int)survey.PointTotal;
            model.RaceID = race.RaceID;
            model.RaceNum = race.RaceNum;
            model.SurveyID = survey.SurveyID;
            model.OSUWins = (int)survey.OSUWins;
            model.UOWins = (int)survey.UOWins;

            if (!race.Winner && (bool)survey.ChoseToBetFlag)
            {
                model.AfterPoints = (int)survey.PointTotal * 2;
            }
            else if (!race.Winner && survey.ChoseToBetFlag != true)
            {
                model.AfterPoints = (int)survey.PointTotal;
            }
            else if (race.Winner && survey.ChoseToBetFlag == true)
            {
                model.AfterPoints = 0;
            }
            else
            {
                model.AfterPoints = (int)survey.PointTotal;
            }

            if(race.Winner)
            {
                model.AfterOSUWins = model.OSUWins;
                model.AfterUOWins = model.UOWins + 1;
            }
            else
            {
                model.AfterUOWins = model.UOWins;
                model.AfterOSUWins = model.OSUWins + 1;
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult FinalRace(RaceViewModel model)
        {
            OSUSurvey survey = db.Surveys.Find(model.SurveyID);
            SettingSet settings = db.SettingSets.Find(survey.SettingsID);
            Race race = db.Races.FirstOrDefault(x => x.SettingSetID == survey.SettingsID && x.RaceNum == settings.NumberOfRaces);

            survey.PointTotal = model.AfterPoints;

            db.Entry(survey).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("FollowUpIntroduction", new { id = model.SurveyID });
        }

        public ActionResult FollowUpIntroduction(int? id)
        {
            OSUSurvey survey = db.Surveys.Find(id);

            return View(survey);
        }

        [HttpPost]
        public ActionResult FollowUpIntroduction(OSUSurvey survey)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("FollowUpSurvey", new { id = survey.SurveyID });
            }
            return View(survey);
        }

        public ActionResult FollowUpSurvey(int? id)
        {
            QuestionViewModel model = new QuestionViewModel();

             OSUSurvey survey = db.Surveys.Find(id);
            if (survey != null)
            {
                SettingSet settings = db.SettingSets.Find(survey.SettingsID);

                List<Question> questions = db.Questions.Where(x => x.SettingSetID == settings.SettingSetID && x.QuestionRoundNum == 2).ToList();

                List<Question> shuffledQuestions = new List<Question>();

                int n = questions.Count();
                while (n >= 1)
                {
                    n--;
                    int k = random.Next(n + 1);
                    Question question = questions[k];
                    questions.Remove(question);
                    shuffledQuestions.Add(question);
                }

                model.Questions = shuffledQuestions;
                model.QuestionNum = 0;
                model.Questions[0].CurrentQuestionFlag = true;

                OSUAnswer answer = new OSUAnswer();
                answer.SurveyID = survey.SurveyID;
                answer.QuestionID = model.Questions[0].QuestionID;
                //answer.QuestionRoundNum = 2;
                model.Answer = answer;
                model.SurveyID = survey.SurveyID;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FollowUpSurvey([Bind(Include = "SurveyID,QuestionNum,Questions,Answer")]QuestionViewModel model)
        {

            OSUSurvey survey = db.Surveys.Find(model.SurveyID);

            if (survey != null)
            {
                while (model.QuestionNum < (model.Questions.Count() - 1))
                {
                    Question currentQuestion = model.Questions[model.QuestionNum];
                    currentQuestion.AskedFlag = true;
                    currentQuestion.CurrentQuestionFlag = false;
                    int currentIndex = model.Questions.IndexOf(currentQuestion);
                    model.Questions[model.QuestionNum].CurrentQuestionFlag = false;
                    model.Questions[model.QuestionNum].AskedFlag = true;

                    model.QuestionNum = model.QuestionNum + 1;
                    model.Questions[model.QuestionNum].CurrentQuestionFlag = true;
                    Question nextQuestion = model.Questions[model.QuestionNum];
                    nextQuestion.CurrentQuestionFlag = true;

                    OSUAnswer answer = new OSUAnswer();
                    answer.AnswerFlag = model.Answer.AnswerFlag;
                    answer.AnswerValue = model.Answer.AnswerValue;
                    answer.SurveyID = model.Answer.SurveyID;
                    answer.QuestionID = model.Answer.QuestionID;
                    //answer.QuestionRoundNum = 2;
                    db.Answers.Add(answer);
                    db.SaveChanges();

                    OSUAnswer newAnswer = new OSUAnswer();
                    newAnswer.AnswerFlag = null;
                    newAnswer.SurveyID = survey.SurveyID;
                    newAnswer.QuestionID = nextQuestion.QuestionID;
                    //newAnswer.QuestionRoundNum = 2;
                    model.Answer = newAnswer;
                    ModelState.Clear();
                    return View(model);
                }

                OSUAnswer finalAnswer = new OSUAnswer();
                finalAnswer.AnswerFlag = model.Answer.AnswerFlag;
                finalAnswer.AnswerValue = model.Answer.AnswerValue;
                finalAnswer.SurveyID = model.Answer.SurveyID;
                finalAnswer.QuestionID = model.Answer.QuestionID;
                //finalAnswer.QuestionRoundNum = 2;
                db.Answers.Add(finalAnswer);
                db.SaveChanges();

                return RedirectToAction("Enjoyment", new { id = model.SurveyID });
            }

            return View(model);
        }

        public ActionResult Enjoyment(int? id)
        {
            OSUSurvey survey = db.Surveys.Find(id);
            SettingSet settings = db.SettingSets.Find(survey.SettingsID);
            EnjoymentViewModel model = new EnjoymentViewModel();

            List<Question> questions = db.Questions.Where(x => x.QuestionRoundNum == 3 && x.SettingSetID == settings.SettingSetID).ToList();
            List <OSUAnswer> answers = new List<OSUAnswer>();

            List<Question> shuffledQuestions = new List<Question>();

            int n = questions.Count();
            while (n >= 1)
            {
                n--;
                int k = random.Next(n + 1);
                Question question = questions[k];
                questions.Remove(question);
                shuffledQuestions.Add(question);
            }

            foreach (Question question in shuffledQuestions)
            {
                OSUAnswer answer = new OSUAnswer();
                answer.SurveyID = survey.SurveyID;
                //answer.QuestionRoundNum = 3;
                answer.QuestionID = question.QuestionID;
                answer.SurveyID = survey.SurveyID;
                answers.Add(answer);
            }

            model.SurveyID = survey.SurveyID;
            model.Questions = shuffledQuestions;
            model.Answers = answers;
            return View(model);
        }

        [HttpPost]
        public ActionResult Enjoyment(EnjoymentViewModel model)
        {
            if(ModelState.IsValid)
            {
                foreach(OSUAnswer answer in model.Answers)
                {
                    db.Answers.Add(answer);
                }
                db.SaveChanges();
                return RedirectToAction("Thoughts", new { id = model.SurveyID });
            }
            return View(model);
        }

        public ActionResult Thoughts(int? id)
        {
            OSUSurvey survey = db.Surveys.Find(id);
            SettingSet settings = db.SettingSets.Find(survey.SettingsID);
            EnjoymentViewModel model = new EnjoymentViewModel();

            List<Question> questions = db.Questions.Where(x => x.QuestionRoundNum == 4 && x.SettingSetID == settings.SettingSetID).OrderBy(x => x.GroupQuestionNumber).ToList();

            List<Question> ouQuestions = questions.Where(x => x.GroupQuestionNumber < 5).ToList();
            List<Question> uoQuestions = questions.Where(x => x.GroupQuestionNumber > 4).ToList();

            List<Question> thoughtsQuestions = new List<Question>();

            int k = random.Next(0, 2);

            if(k == 0)
            {
                thoughtsQuestions.AddRange(ouQuestions);
                thoughtsQuestions.AddRange(uoQuestions);
            }
            else
            {
                thoughtsQuestions.AddRange(uoQuestions);
                thoughtsQuestions.AddRange(ouQuestions);
            }

            List<OSUAnswer> answers = new List<OSUAnswer>();
            foreach (Question question in thoughtsQuestions)
            {
                OSUAnswer answer = new OSUAnswer();
                answer.SurveyID = survey.SurveyID;
                //answer.QuestionRoundNum = 4;
                answer.QuestionID = question.QuestionID;
                answer.SurveyID = survey.SurveyID;
                answers.Add(answer);
            }

            model.SurveyID = survey.SurveyID;
            model.Questions = thoughtsQuestions;
            model.Answers = answers;
            return View(model);
        }

        [HttpPost]
        public ActionResult Thoughts(EnjoymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                foreach (OSUAnswer answer in model.Answers)
                {
                    db.Answers.Add(answer);
                }
                db.SaveChanges();
                return RedirectToAction("Final", new { id = model.SurveyID });
            }
            return View(model);
        }

        public ActionResult Final(int? id)
        {
            OSUSurvey survey = db.Surveys.Find(id);
            SettingSet settings = db.SettingSets.Find(survey.SettingsID);
            EnjoymentViewModel model = new EnjoymentViewModel();

            List<Question> questions = db.Questions.Where(x => x.QuestionRoundNum == 5 && x.SettingSetID == settings.SettingSetID).OrderBy(x => x.GroupQuestionNumber).ToList();
            List<OSUAnswer> answers = new List<OSUAnswer>();

            List<Question> shuffledQuestions = new List<Question>();

            int n = questions.Count();
            while (n >= 1)
            {
                n--;
                int k = random.Next(n + 1);
                Question question = questions[k];
                questions.Remove(question);
                shuffledQuestions.Add(question);
            }

            foreach (Question question in shuffledQuestions)
            {
                OSUAnswer answer = new OSUAnswer();
                answer.SurveyID = survey.SurveyID;
                //answer.QuestionRoundNum = 5;
                answer.QuestionID = question.QuestionID;
                answer.SurveyID = survey.SurveyID;
                answers.Add(answer);
            }

            model.SurveyID = survey.SurveyID;
            model.Questions = shuffledQuestions;
            model.Answers = answers;
            return View(model);
        }

        [HttpPost]
        public ActionResult Final(EnjoymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                foreach (OSUAnswer answer in model.Answers)
                {
                    db.Answers.Add(answer);
                }
                db.SaveChanges();
                return RedirectToAction("End", new { id = model.SurveyID });
            }
            return View(model);
        }

        public ActionResult End(int? id)
        {
            OSUSurvey survey = db.Surveys.Find(id);
            return View(survey);
        }

        [HttpPost]
        public ActionResult End(OSUSurvey survey)
        {
             return RedirectPermanent("https://google.com");
        }
        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}
    }
}