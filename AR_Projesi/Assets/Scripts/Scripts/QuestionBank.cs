using UnityEngine;

[CreateAssetMenu(fileName = "QuestionBank",
menuName = "Quiz/Question Bank")]
public class QuestionBank : ScriptableObject
{
    public QuestionData[] questions;
}