using CardSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoapAgentAbilityController : MonoBehaviour
{
    // Dictionary of goap ability key and int array value which represents remainingCD and totalUses of the ability key (indices 0 & 1 respectively)
    private Dictionary<GoapAgentAbility, int[]> _harmfulAbilities = new();
    private Dictionary<GoapAgentAbility, int[]> _helpfulAbilities = new();

    public CardAbilityDefinition GetHarmfulAbility => GetBestAbilityFromDict(_harmfulAbilities)?.ability;
    public CardAbilityDefinition GetHelpfulAbility => GetBestAbilityFromDict(_helpfulAbilities)?.ability;
    public bool CheckCanUseHeal => CheckCanUse(_helpfulAbilities, GetBestAbilityFromDict(_helpfulAbilities));
    public bool CheckCanUseAttack => CheckCanUse(_harmfulAbilities, GetBestAbilityFromDict(_harmfulAbilities));

    public void InitAbilities(GoapAgentSO agentSO)
    {
        _harmfulAbilities = new();
        _helpfulAbilities = new();

        foreach (var abilty in agentSO.GetHarmfulAbilities)
            _harmfulAbilities.Add(abilty, new int[2]);

        foreach (var abilty in agentSO.GetHelpfulAbilities)
            _helpfulAbilities.Add(abilty, new int[2]);
    }
    public void OnAbilityUse(CardAbilityDefinition abilityDef)
    {
        if (abilityDef == null) return;
        UpdateTrackerOnUse(_harmfulAbilities, abilityDef);
        UpdateTrackerOnUse(_helpfulAbilities, abilityDef);
    }
    public void OnAgentTurnStart()
    {
        DecrementAbilityCooldowns(_harmfulAbilities);
        DecrementAbilityCooldowns(_helpfulAbilities);
    }
    
    private void UpdateTrackerOnUse(Dictionary<GoapAgentAbility, int[]> dict, CardAbilityDefinition abilityDef)
    {
        for (int i = dict.Count - 1; i >= 0; i--)
        {
            var kvp = dict.ElementAt(i);
            var ability = kvp.Key;
            if (ability == null || ability.ability == null)
            {
                Debug.Log($"Ability null, removing from dict.");
                dict.Remove(kvp.Key);
                continue;
            }
            if (ability.ability != abilityDef) continue;
            var trackerArray = kvp.Value;
            trackerArray[0] = ability.cooldownInTurns;
            trackerArray[1] += ability.maxUses != -1 ? 1 : 0;
        }
    }
    private void DecrementAbilityCooldowns(Dictionary<GoapAgentAbility, int[]> dict)
    {
        for (int i = 0; i < dict.Count; i++)
        {
            var trackerArray = dict.ElementAt(i).Value;
            if (trackerArray[0] > 0)
                trackerArray[0]--;
            else
                trackerArray[0] = 0; // ensure no negative cd remaining
        }
    }

    // grab highest "prio" ability based on cooldown timer. Longer cd = higer prio
    private GoapAgentAbility GetBestAbilityFromDict(Dictionary<GoapAgentAbility, int[]> dict)
    {
        GoapAgentAbility temp = null;
        int highestPrio = int.MinValue;

        for (int i = dict.Count - 1; i >= 0; i--)
        {
            var kvp = dict.ElementAt(i);
            var tracker = kvp.Value;
            var ability = kvp.Key;
            if (ability == null || ability.ability == null)
            {
                Debug.LogWarning($"Goap Agent Ability null, removing from dict.");
                dict.Remove(kvp.Key);
                continue;
            }
            if (tracker[0] > 0 || ability.maxUses != -1 && tracker[1] >= ability.maxUses)
                continue;
            if (ability.cooldownInTurns < highestPrio) continue;
            temp = ability;
            highestPrio = ability.cooldownInTurns;
        }

        //if (temp == null)
            //Debug.LogWarning($"Null ability returned");
        return temp;
    }
    private bool CheckCanUse(Dictionary<GoapAgentAbility, int[]> dict, GoapAgentAbility ability)
    {
        if (ability == null || !dict.ContainsKey(ability)) return false;
        var tracker = dict[ability];
        return tracker[0] <= 0 && (ability.maxUses == -1 || tracker[1] < ability.maxUses);
    }
}
