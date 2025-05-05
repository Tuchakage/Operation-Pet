using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class AbilityCooldownUI : MonoBehaviourPunCallbacks
{
    public GameObject cooldownUIPanel; // Panel to show cooldown info
    public TextMeshProUGUI cooldownTextPrefab; // Prefab for cooldown display
    private Dictionary<string, float> abilityCooldowns = new Dictionary<string, float>();
    private Dictionary<string, TextMeshProUGUI> abilityUIElements = new Dictionary<string, TextMeshProUGUI>();

    void Start()
    {
        cooldownUIPanel.SetActive(false);
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            UpdateCooldowns();
        }
    }

    [PunRPC]
    public void StartCooldownRPC(string abilityName, float cooldownDuration)
    {
        if (!abilityCooldowns.ContainsKey(abilityName))
        {
            abilityCooldowns.Add(abilityName, cooldownDuration);
            TextMeshProUGUI cooldownText = Instantiate(cooldownTextPrefab, cooldownUIPanel.transform);
            cooldownText.text = $"{abilityName}: {cooldownDuration:F1}s";
            abilityUIElements.Add(abilityName, cooldownText);

            cooldownUIPanel.SetActive(true);
        }
    }

    private void UpdateCooldowns()
    {
        List<string> expiredAbilities = new List<string>();

        foreach (var ability in abilityCooldowns.Keys)
        {
            abilityCooldowns[ability] -= Time.deltaTime;
            if (abilityCooldowns[ability] <= 0)
            {
                expiredAbilities.Add(ability);
            }
            else
            {
                abilityUIElements[ability].text = $"{ability}: {abilityCooldowns[ability]:F1}s";
            }
        }

        foreach (string ability in expiredAbilities)
        {
            Destroy(abilityUIElements[ability].gameObject);
            abilityCooldowns.Remove(ability);
            abilityUIElements.Remove(ability);
        }

        if (abilityCooldowns.Count == 0)
        {
            cooldownUIPanel.SetActive(false);
        }
    }
}