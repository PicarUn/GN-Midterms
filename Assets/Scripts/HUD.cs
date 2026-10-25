using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] TMP_Text p1Text;
    [SerializeField] TMP_Text p2Text;

    void Update()
    {
        bool p1Found = false, p2Found = false;

        foreach (var p in FindObjectsByType<PlayerCombat>(FindObjectsSortMode.None))
        {
            string s = $"Kills: {p.Kills.Value}\nHP: {p.Health.Value}\nAmmo: {p.Ammo.Value}";

            if (p.OwnerClientId == 0) { p1Text.text = "P1\n" + s; p1Found = true; }
            else if (p.OwnerClientId == 1) { p2Text.text = "P2\n" + s; p2Found = true; }
        }

        p1Text.gameObject.SetActive(p1Found);
        p2Text.gameObject.SetActive(p2Found);
    }
}