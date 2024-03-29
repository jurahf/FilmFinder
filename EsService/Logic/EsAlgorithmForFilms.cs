﻿using ExpertSystemDb;
using ExpertSystemDb.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    public class EsAlgorithmForFilms : EsAlgorithm
    {
        private const string esName = "Фильмы";

        private const string переменнаяЗнаюЧегоХочу = "ЗнаюЧегоХочу";
        private const string ответЗнаюЧегоХочу = "Знаю жанр или настроение фильма";
        private const string ответНеЗнаюЧегоХочу = "Нет, хочу определиться";

        private const string переменнаяЖанрыИТэги = "ЖанрыИТэги";
        private const string ответПереходНаЖанры = "Указать жанры";
        private const string ответПереходНаТэги = "Указать тэги";
        private const string ответЗакончитьВыбор = "Завершить выбор";

        private const int максимумЖанров = 3;


        public EsAlgorithmForFilms(DBWork db)
            : base(db)
        {
        }


        public override Session CreateSessionAndGoConsult(string esName)
        {
            Session session = CreateSession();
            var pq = new PreprocessQuestions()
            {
                Session = session,
                IKnowThatIWant = null,
                ActiveFilterType = FilterType.Genre
            };
            db.Insert(pq);

            return session;
        }

        public override QuestionOrResultDto GetNextQuestionOrResult(string sessionId)
        {
            Session session = FindSession(sessionId);

            if (session.PreprocessQuestions != null && session.PreprocessQuestions.IKnowThatIWant == null)
            {
                // задаем вопрос, знаете, что вы хотите или как
                return СоздатьВопросЗнаетеЛиЧегоХотите(session);
            }
            else if (session.PreprocessQuestions?.IKnowThatIWant == true)
            {
                // предлагаем выбрать жанр или свойство
                return СоздатьВопросПоВыборуЖанраИлиТэга(session);
            }
            else
            {
                GoConsult(esName, session);
                return base.GetNextQuestionOrResult(sessionId);
            }
        }

        public override QuestionOrResultDto SetAnswerAndGetNextQuestionOrResult(string sessionId, VariableValue answer)
        {
            Session session = FindSession(sessionId);

            if (session.PreprocessQuestions != null && session.PreprocessQuestions.IKnowThatIWant == null)
            {
                return ПринятьОтветЗнаютЛиЧегоХотят(session, answer);
            }
            else if (session.PreprocessQuestions?.IKnowThatIWant == true)
            {
                return ПринятьОтветПоЖанрамИТэгам(session, answer);
            }
            else
            {
                // уже ответили, что они не знают, что хотят. Им надо в ЭС
                return base.SetAnswerAndGetNextQuestionOrResult(sessionId, answer);
            }
        }


        private QuestionOrResultDto ПринятьОтветЗнаютЛиЧегоХотят(Session session, VariableValue answer)
        {
            if (answer.Variable.ToLower() != переменнаяЗнаюЧегоХочу.ToLower())
                throw new ArgumentException("Неправильная последовательность ответов и вопросов");

            session.PreprocessQuestions.IKnowThatIWant = answer.Value.ToLower() == ответЗнаюЧегоХочу.ToLower();
            db.Update(session.PreprocessQuestions);

            if (session.PreprocessQuestions.IKnowThatIWant == true)
            {
                // если знают, чего хотят - предложить фильтры
                return СоздатьВопросПоВыборуЖанраИлиТэга(session);
            }
            else
            {
                // не знают чего хотят - запустим ЭС
                GoConsult(esName, session);
                return base.GetNextQuestionOrResult(session.SessionId);
            }
        }


        private QuestionOrResultDto ПринятьОтветПоЖанрамИТэгам(Session session, VariableValue answer)
        {
            if (answer.Variable.ToLower() != переменнаяЖанрыИТэги.ToLower())
                throw new ArgumentException("Неправильная последовательность ответов и вопросов");

            if (answer.Value.ToLower() == ответЗакончитьВыбор.ToLower())
            {
                return СоздатьОтветПоЖанрамИТэгам(session);
            }
            else if (answer.Value.ToLower() == ответПереходНаЖанры.ToLower())
            {
                session.PreprocessQuestions.ActiveFilterType = FilterType.Genre;
                db.Update(session.PreprocessQuestions);
                return СоздатьВопросПоВыборуЖанраИлиТэга(session);
            }
            else if (answer.Value.ToLower() == ответПереходНаТэги.ToLower())
            {
                session.PreprocessQuestions.ActiveFilterType = FilterType.CustomProperty;
                db.Update(session.PreprocessQuestions);
                return СоздатьВопросПоВыборуЖанраИлиТэга(session);
            }
            else
            {
                // выбор конкретного жанра или свойства
                if (session.PreprocessQuestions.ActiveFilterType == FilterType.Genre)
                {
                    Genre genre = db.GetFromDatabase<Genre>(x => x.Name.ToLower() == answer.Value.ToLower()).FirstOrDefault();
                    if (genre != null)
                    {
                        GenreForFilter gff = new GenreForFilter()
                        {
                            PreprocessQuestions = session.PreprocessQuestions,
                            Genre = genre
                        };

                        db.AddWithoutSave(gff);
                    }
                }
                else if (session.PreprocessQuestions.ActiveFilterType == FilterType.CustomProperty)
                {
                    CustomProperty customProp = db.GetFromDatabase<CustomProperty>(x => x.Name.ToLower() == answer.Value.ToLower()).FirstOrDefault();
                    if (customProp != null)
                    {
                        CustomPropertyForFilter cpff = new CustomPropertyForFilter()
                        {
                            PreprocessQuestions = session.PreprocessQuestions,
                            CustomProperty = customProp
                        };

                        db.AddWithoutSave(cpff);
                    }
                }

                db.Save();

                // заканчивать выбор, если выбрано уже много
                if (session.PreprocessQuestions.GenreForFilter.Count >= максимумЖанров)
                {
                    return СоздатьОтветПоЖанрамИТэгам(session);
                }
                else
                {
                    return СоздатьВопросПоВыборуЖанраИлиТэга(session);
                }
            }
        }

        private QuestionOrResultDto СоздатьВопросПоВыборуЖанраИлиТэга(Session session)
        {
            var result = new QuestionOrResultDto(session.SessionId)
            {
                Question = new VariableDto()
                {
                    Name = переменнаяЖанрыИТэги,
                }
            };

            List<string> selectedGenres = session.PreprocessQuestions.GenreForFilter.Select(x => x.Genre.Name).ToList();

            result.Question.Question = session.PreprocessQuestions.ActiveFilterType == FilterType.Genre
                ? $"Выберите жанры (можно выбрать еще до {3 - selectedGenres.Count} жанров)"
                : "Выберите тэги";

            result.Question.Domain = new List<QuestionDomainValueDto>();

            if (session.PreprocessQuestions.ActiveFilterType == FilterType.Genre)
            {
                int i = 0;
                foreach (var val in db.GetFromDatabase<Genre>()
                        .Where(x => !selectedGenres.Contains(x.Name))
                        .OrderBy(x => x.Name))
                {
                    result.Question.Domain.Add(new QuestionDomainValueDto(val.Name, DomainValueColor.Blue, i != 0 && i % 3 == 0));
                    i++;
                }

                //result.Question.Domain.Add(ответПереходНаТэги); // пока не делаем, пусть будет только по жанрам
            }
            else
            {
                int i = 0;
                foreach (var val in db.GetFromDatabase<CustomProperty>().OrderBy(x => x.Name))
                {
                    result.Question.Domain.Add(new QuestionDomainValueDto(val.Name, DomainValueColor.Blue, i !=0 && i % 3 == 0));
                    i++;
                }

                result.Question.Domain.Add(new QuestionDomainValueDto(ответПереходНаЖанры, DomainValueColor.Green, true));
            }

            result.Question.Domain.Add(new QuestionDomainValueDto(ответЗакончитьВыбор, DomainValueColor.Green, true));

            return result;
        }

        private QuestionOrResultDto СоздатьОтветПоЖанрамИТэгам(Session session)
        {
            // сохранить результат консультации
            Consultation consult = db.GetFromDatabase<Consultation>(x => x.Session.SessionId == session.SessionId).FirstOrDefault();
            if (consult == null)
            {
                consult = new Consultation()
                {
                    Session = session,
                    ExpertSystem = db.GetFromDatabase<ExpertSystem>(x => x.Name == esName).First()
                };
                db.Insert(consult);
            }

            FinalSolution final = new FinalSolution()
            {
                Consultation = consult,
                VariableName = FilmAndAdviceLogic.GenreAndTagResultVariableName,
                Value = session.SessionId
            };
            db.Insert(final);

            return new QuestionOrResultDto(session.SessionId)
            {
                Result = new ConsultResultDto()
                {
                    Fact = new FactDto()
                    {
                        VarName = FilmAndAdviceLogic.GenreAndTagResultVariableName, 
                        Value = session.SessionId
                    }
                }
            };
        }


        private QuestionOrResultDto СоздатьВопросЗнаетеЛиЧегоХотите(Session session)
        {
            return new QuestionOrResultDto(session.SessionId)
            {
                Question = new VariableDto()
                {
                    Name = переменнаяЗнаюЧегоХочу,
                    Question = "Вы примерно знаете, что хотите посмотреть?",
                    Domain = new List<QuestionDomainValueDto>()
                        {
                            new QuestionDomainValueDto(ответЗнаюЧегоХочу),
                            new QuestionDomainValueDto(ответНеЗнаюЧегоХочу)
                        }
                }
            };
        }


        private Session FindSession(string sessionId)
        {
            Session session = db.GetFromDatabase<Session>(x => x.SessionId == sessionId).FirstOrDefault();

            if (session == null)
                throw new ArgumentException("Не найдена сессия пользователя");

            return session;
        }



    }
}
