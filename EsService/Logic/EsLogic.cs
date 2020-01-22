using ExpertSystemDb;
using ExpertSystemDb.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic
{
    /// <summary>
    /// Непосредственно запуск консультации и поиск цели.
    /// </summary>
    public class EsLogic
    {
        private DBWork db;

        public EsLogic(DBWork db)
        {
            this.db = db;
        }


        /// <summary>
        /// Создает (и сохраняет) новую консультацию на базе экспертной системы
        /// </summary>
        public Consultation CreateConsult(Session session, ExpertSystem es)
        {
            Consultation consult = new Consultation()
            {
                Session = session,
                ExpertSystem = es,
                // CurrentRule = ... ?
            };

            PushGoal(consult, es.Target); // первая текущая цель консультации - это цель ЭС
            db.Insert(consult);

            return consult;
        }




        /// <summary>
        /// Добавляет переменную как текущую цель консультации (и сохраняет)
        /// </summary>
        /// <param name="consult"></param>
        /// <param name="variable"></param>
        public void PushGoal(Consultation consult, Variable variable)
        {
            GoalStack temp = consult.GoalStack;

            consult.GoalStack = new GoalStack()
            {
                Variable = variable,
                NextGoal = temp
            };

            db.Insert(consult.GoalStack);
        }

        public GoalStack PopGoal(Consultation consult)
        {
            var pred = consult.GoalStack;
            consult.GoalStack = consult.GoalStack?.NextGoal;
            db.Update(consult);

            return pred;
        }


        /// <summary>
        /// Устанавливает значение переменной из стэка целей на основании имеющихся правил
        /// </summary>
        public FactQuestionOrResult DoConsult(Consultation consult)
        {
            Variable tempGoal = consult.GoalStack?.Variable;

            if (tempGoal == null)
            {
                // результат уже есть
                PopGoal(consult);
                return GetProvedFact(consult, tempGoal);
            }
            else
            {
                if (tempGoal.Type == VariableType.Queriable)
                {
                    // запрашиваемая - запрашиваем 
                    return new FactQuestionOrResult() { Question = tempGoal };
                }
                else
                {
                    List<Rule> rules = consult.ExpertSystem.AllRules.OrderBy(x => x.Order).ToList();

                    // выводимая - ищем правила с этим фактом в выводе
                    foreach (Rule r in rules)
                    {
                        if (r.Result != null && r.Result.Variable == tempGoal)
                        {
                            RuleQuestionOrResult workedRule = DoRule(consult, r);
                            if (workedRule.Question != null)
                            {
                                return new FactQuestionOrResult() { Question = workedRule.Question };
                            }

                            if (workedRule.Result.Work != RuleWork.Signify)
                            {
                                if (tempGoal.Type == VariableType.QuerriableDetective) // TODO: вообще-то это надо только когда все выводимые правила не сработали
                                {
                                    // теперь запросим
                                    return new FactQuestionOrResult() { Question = tempGoal };
                                }
                                continue;
                            }
                            else
                            {
                                PopGoal(consult);
                                return new FactQuestionOrResult() { Result = CreateConsultationFact(consult, r.Result, FactTruly.IsTrue) };
                            }
                        }
                    }
                }

                // не удалось вывести значение
                ConsultationFact cf = new ConsultationFact()
                {
                    Consultation = consult,
                    Truly = FactTruly.Unknown,
                    Fact = new Fact() { Variable = tempGoal }       // ничего не сохраняем - так прокатит?
                };
                PopGoal(consult);
                return new FactQuestionOrResult() { Result = cf };
            }
        }

        /// <summary>
        /// Создает и сохраняет в базу новый проверенный факт в консультации
        /// </summary>
        /// <param name="consult"></param>
        /// <param name="fact"></param>
        /// <param name="isTrue"></param>
        /// <returns></returns>
        private ConsultationFact CreateConsultationFact(Consultation consult, Fact fact, FactTruly isTrue)
        {
            ConsultationFact cf = new ConsultationFact()
            {
                Consultation = consult,
                Truly = isTrue,
                Fact = fact
            };

            db.Insert(cf);

            return cf;
        }




        private FactQuestionOrResult GetProvedFact(Consultation consult, Variable tempGoal)
        {
            ConsultationFact provedFact = consult.ProvedFacts
                .Where(x => x.Fact.Variable.Name == tempGoal.Name
                    && x.Truly == FactTruly.IsTrue)
                .FirstOrDefault();
            if (provedFact == null)
            {
                provedFact = consult.ProvedFacts
                .Where(x => x.Fact.Variable.Name == tempGoal.Name
                    && x.Truly == FactTruly.Unknown)
                .FirstOrDefault();
            }

            return new FactQuestionOrResult() { Result = provedFact };
        }


        /// <summary>
        /// Применение одного правила
        /// </summary>
        /// <param name="r">Это правило</param>
        public RuleQuestionOrResult DoRule(Consultation consult, Rule r)
        {
            // SetCurrentRule
            consult.CurrentRule = r;

            bool isTrue = true;
            foreach (Fact fact in r.Conditions.Select(x => x.Fact))
            {
                ConsultationFact provedFact = НайтиУжеПроверенныйФакт(consult.ProvedFacts, fact);
                if (provedFact == null)
                {
                    // доказываем и записываем факты
                    PushGoal(consult, fact.Variable);
                    var consultRes = DoConsult(consult);

                    if (consultRes.Question != null)
                    {
                        return new RuleQuestionOrResult() { Question = consultRes.Question };
                    }

                    if (consultRes.Result.Truly == FactTruly.IsTrue)
                        isTrue = (fact == consultRes.Result.Fact);
                    else
                        isTrue = false;

                    if (!isTrue)
                        break; // если факт не верен - неверно все правило
                }
                else
                {
                    // факт уже известен
                    if (provedFact.Truly == FactTruly.IsTrue)
                        isTrue = true;
                    else
                    {
                        isTrue = false;
                        break;
                    }
                }
            }

            if (isTrue) // если все верно - делаем вывод, сохраняем ConsultationRule
            {
                if (r.Result == null || !r.Result.Variable.Domain.DomainValue.Contains(r.Result.DomainValue))
                {
                    throw new ArgumentOutOfRangeException("Правило " + r.Name + " пытается присвоить значение не из домена!");
                }

                ConsultationRule resultRule = new ConsultationRule()
                {
                    Rule = r,
                    Work = RuleWork.Signify, // означилось
                    Consultation = consult
                };
                // proved.Add(r.Result); // это уже вроде делается в GoConsult?
                db.Insert(resultRule);

                return new RuleQuestionOrResult()
                {
                    Result = resultRule
                };
            }
            else
            {
                ConsultationRule resultRule = new ConsultationRule()
                {
                    Rule = r,
                    Work = RuleWork.UnSignify, // не означилось
                    Consultation = consult
                };
                db.Insert(resultRule);

                return new RuleQuestionOrResult()
                {
                    Result = resultRule
                };
            }
        }

        /// <summary>
        /// Среди проверенный ищет факт с такой же переменной. Если значение другое, возвращает, что факт ложный.
        /// </summary>
        /// <param name="provedFacts"></param>
        /// <param name="fact"></param>
        /// <returns></returns>
        private ConsultationFact НайтиУжеПроверенныйФакт(ICollection<ConsultationFact> provedFacts, Fact fact)
        {
            // сначала поищем доказанные с этим значением
            var result = provedFacts.FirstOrDefault(x => x.Fact.Variable == fact.Variable && x.Fact.DomainValue == fact.DomainValue);
            if (result != null)
                return result;

            // если не нашли, но есть с этой переменной и другим значеним, то этот - ложный
            result = provedFacts.FirstOrDefault(x => x.Fact.Variable == fact.Variable && x.Truly == FactTruly.IsTrue);
            if (result != null)
                return result;

            // ничего не нашли - null
            return null;
        }


        public FactQuestionOrResult ContinueConsult(Consultation consult, VariableValue answer)
        {
            var currentRule = consult.CurrentRule;
            var lastGoal = PopGoal(consult);

            // надо записать ответ
            List<Fact> фактыДляОзначивания = db.GetFromDatabase<Fact>(x => x.Variable.Name == answer.Variable); // TODO: тут могут попасть факты из других ЭС
            foreach (var f in фактыДляОзначивания)
            {
                var cf = new ConsultationFact()
                {
                    Consultation = consult,
                    Fact = f,
                    Truly = f.DomainValue.Value == answer.Value ? FactTruly.IsTrue : FactTruly.IsFalse
                };
                db.Insert(cf);
            }


            if (consult.GoalStack == null || currentRule == null) // текущего правила может не быть только если главная цель была запрашиваемая
            {
                // мы нашли значение цели консультации
                ConsultationFact resultFact;
                if (lastGoal == null)
                {
                    // вероятно, нашли цель когда-то давно
                    resultFact = db.GetFromDatabase<ConsultationFact>(x => 
                            x.Consultation == consult
                            && x.Fact.Variable.Name == consult.ExpertSystem.Target.Name 
                            && x.Truly == FactTruly.IsTrue)
                        .FirstOrDefault();
                }
                else
                {
                    resultFact = db.GetFromDatabase<ConsultationFact>(x => 
                            x.Consultation == consult
                            && x.Fact.Variable == lastGoal.Variable 
                            && x.Truly == FactTruly.IsTrue)
                        .FirstOrDefault();
                }

                if (resultFact != null)
                {
                    return new FactQuestionOrResult() { Result = resultFact };
                }
                else
                {
                    // не удалось установить истину
                    return new FactQuestionOrResult()
                    {
                        Result = new ConsultationFact()
                        {
                            Consultation = consult, 
                            Fact = new Fact()
                            {
                                Variable = lastGoal.Variable,
                                DomainValue = lastGoal.Variable.Domain.DomainValue.First()
                            },
                            Truly = FactTruly.Unknown
                        }
                    };
                }
            }
            else
            {
                return DoConsult(consult); // ищем следующую цель из стека (текущие означенные уже запрашиваться не будут)
            }
        }





    }
}
