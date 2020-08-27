using ExpertSystemDb;
using ExpertSystemDb.DTOs;
using Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ЭС
{
    public partial class frmConsultation : Form
    {
        private DBWork db;
        private EsAlgorithm algorithm;
        private Session session;
        private const string esName = "Фильмы";
        private List<RadioButton> rbAnswerList = new List<RadioButton>();
        private VariableDto currentQuest;


        public frmConsultation()
        {
            InitializeComponent();
        }

        private void frmConsultation_Load(object sender, EventArgs e)
        {
            db = new DBWork();
            algorithm = new EsAlgorithm(db);
            session = algorithm.CreateSessionAndGoConsult(esName);
            QuestionOrResultDto result = algorithm.GetNextQuestionOrResult(session.SessionId);
            ShowQuestion(result.Question);
        }


        private void btnAnswer_Click(object sender, EventArgs e)
        {
            var rbAns = rbAnswerList.FirstOrDefault(x => x.Checked);
            AnswerAndNext(currentQuest.Name, rbAns.Text);
        }





        private void AnswerAndNext(string variable, string value)
        {
            var answer = new VariableValue()
            {
                Variable = variable,
                Value = value,
            };

            QuestionOrResultDto result = algorithm.SetAnswerAndGetNextQuestionOrResult(session.SessionId, answer);


            if (result.Question != null)
            {
                ShowQuestion(result.Question);
            }
            else
            {
                // готов ответ, получим подробности
                string key = result.Result.Fact.Value;
                Guid guid;
                if (!Guid.TryParse(key, out guid))
                {
                    MessageBox.Show("Не удалось найти ответ по GUID", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Advice advice = db.GetFromDatabase<Advice>(x => x.Key == guid).FirstOrDefault();

                if (advice == null)
                {
                    MessageBox.Show("Не удалось найти совет", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var filmList = new FilmAndAdviceLogic().FindFilmsByAdvice(advice);
                var filmDtos = filmList.Select(x => new FilmDto(x)).ToList();

                int index = new Random().Next(0, filmDtos.Count - 1);
                var film = filmDtos[index];
                ShowResult(film);
            }
        }



        private void ShowQuestion(VariableDto question)
        {
            lblQName.Text = question.Name;
            lblQText.Text = question.Question;
            currentQuest = question;

            foreach (var rb in rbAnswerList)
            {
                rb.Dispose();
            }
            rbAnswerList.Clear();

            int left = 40;
            int top = 20;
            int topInc = 30;
            foreach (var v in question.Domain)
            {
                RadioButton rb = new RadioButton()
                {
                    Parent = panel1,
                    Top = top,
                    Left = left,
                    Dock = DockStyle.Top,
                    Text = v
                };

                top += topInc;
                rbAnswerList.Add(rb);
                panel1.Controls.Add(rb);
            }
        }


        private void ShowResult(FilmDto film)
        {
            tabControl1.SelectedTab = tabPage2;

            lblName.Text = film.Name;
            lblDescription.Text = film.Description;
            lblYear.Text = film.Year.ToString();
            lblSlogan.Text = film.Slogan;
            lblRating.Text = film.Rating.ToString();
            lblGenre.Text = string.Join(", ", film.Genries.Select(x => x.Name));
            lblCountry.Text = string.Join(", ", film.Countries.Select(x => x.Name));
            lblActor.Text = string.Join(", ", film.Actors.Select(x => x.Name));
            lblProducer.Text = string.Join(", ", film.Producers.Select(x => x.Name));
            lblImdb.Text = film.KinopoiskId;
            lblImdb.LinkClicked += (s, a) => Process.Start($"https://www.imdb.com/title/tt{film.KinopoiskId}");
            lblPoster.Text = film.PosterUrl;
            lblPoster.LinkClicked += (s, a) => Process.Start(film.PosterUrl);
            lblTags.Text = string.Join(", ", film.CustomProperties.OrderByDescending(x => x.Percent).Select(x => $"{x.Name} ({x.Percent})"));
        }


    }
}
