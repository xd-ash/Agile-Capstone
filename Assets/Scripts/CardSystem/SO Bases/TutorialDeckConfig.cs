using System.Collections.Generic;
using CardSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialDeckConfig", menuName = "Tutorial/TutorialDeckConfig")]
public class TutorialDeckConfig : ScriptableObject
{
    [SerializeField] private List<CardAbilityDefinition> _tutorialCards = new();

    public List<CardAbilityDefinition> GetTutorialCards => _tutorialCards;
}