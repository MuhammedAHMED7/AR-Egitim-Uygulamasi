using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizManager : MonoBehaviour
{
    public QuestionBank questionBank;

    public TMP_Text questionText;

    public TMP_Text feedbackText;

    public Button[] answerButtons;

    public GameObject quizPanel;

    public GameObject summaryPanel;

    public TMP_Text finalScoreText;

    public TMP_Text accuracyText;

    private int currentQuestion = 0;

    private int score = 0;

    void Start()
    {
        quizPanel.SetActive(false);

        summaryPanel.SetActive(false);
    }

    public void StartQuiz()
    {
        quizPanel.SetActive(true);

        ShowQuestion();
    }

    void ShowQuestion()
    {
        QuestionData q =
            questionBank.questions[currentQuestion];

        questionText.text = q.question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i;

            answerButtons[i]
                .GetComponentInChildren<TMP_Text>()
                .text = q.answers[i];

            answerButtons[i].onClick.RemoveAllListeners();

            answerButtons[i].onClick.AddListener(() =>
            {
                CheckAnswer(index);
            });
        }
    }

    void CheckAnswer(int selected)
    {
        QuestionData q =
            questionBank.questions[currentQuestion];

        if (selected == q.correctAnswer)
        {
            feedbackText.text = "Correct!";

            score++;
        }
        else
        {
            feedbackText.text = "Wrong!";
        }

        currentQuestion++;

        if (currentQuestion < questionBank.questions.Length)
        {
            Invoke(nameof(ShowQuestion), 1.5f);
        }
        else
        {
            Invoke(nameof(ShowSummary), 1.5f);
        }
    }

    void ShowSummary()
    {
        quizPanel.SetActive(false);

        summaryPanel.SetActive(true);

        finalScoreText.text =
            "Score: " + score;

        float accuracy =
            ((float)score /
            questionBank.questions.Length) * 100f;

        accuracyText.text =
            "Accuracy: " + accuracy + "%";

        SaveScore(score);
    }

    void SaveScore(int value)
    {
        PlayerPrefs.SetInt("LastScore", value);

        PlayerPrefs.Save();
    }
}
