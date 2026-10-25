using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance;
    public static bool Active => Instance != null && Instance.IsMatchActive.Value;

    public NetworkVariable<int> MatchTime = new(120);
    public NetworkVariable<bool> IsMatchActive = new(false);

    [SerializeField] float matchLength = 120f;
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text winText;
    [SerializeField] Button restartButton;

    float timeLeft;

    void Awake() => Instance = this;

    public override void OnNetworkSpawn()
    {
        timerText.gameObject.SetActive(true);
        winText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);

        IsMatchActive.OnValueChanged += (o, n) =>
        {
            if (n) winText.gameObject.SetActive(false);
        };

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            restartButton.onClick.AddListener(StartMatch);
        }
    }

    void OnClientConnected(ulong id)
    {
        if (!IsMatchActive.Value && MatchTime.Value > 0 &&
            NetworkManager.ConnectedClients.Count >= 2)
            StartMatch();
    }

    void StartMatch()
    {
        foreach (var p in FindObjectsByType<PlayerCombat>(FindObjectsSortMode.None))
            p.ResetForNewMatch();

        timeLeft = matchLength;
        MatchTime.Value = (int)matchLength;
        IsMatchActive.Value = true;
        restartButton.gameObject.SetActive(false);
    }

    void Update()
    {
        timerText.text = MatchTime.Value.ToString();

        if (!IsServer || !IsMatchActive.Value) return;

        timeLeft -= Time.deltaTime;
        int s = Mathf.CeilToInt(timeLeft);
        if (s != MatchTime.Value) MatchTime.Value = Mathf.Max(s, 0);
        if (timeLeft <= 0) EndMatch();
    }

    void EndMatch()
    {
        IsMatchActive.Value = false;

        PlayerCombat best = null;
        bool tie = false;
        foreach (var p in FindObjectsByType<PlayerCombat>(FindObjectsSortMode.None))
        {
            if (best == null || p.Kills.Value > best.Kills.Value) { best = p; tie = false; }
            else if (p.Kills.Value == best.Kills.Value) tie = true;
        }

        string msg = (best == null || tie) ? "Draw!" : $"Player {best.OwnerClientId + 1} Wins!";
        GameOverRpc(msg);
    }

    [Rpc(SendTo.Everyone)]
    void GameOverRpc(string msg)
    {
        winText.text = msg;
        winText.gameObject.SetActive(true);
        if (IsServer) restartButton.gameObject.SetActive(true);
    }
}