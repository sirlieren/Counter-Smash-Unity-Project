using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sahne yeniden yüklendiğinde de yaşayan seviye durumu: hangi seviyedeyiz, tekrar/sonraki nasıl yüklenir.
/// </summary>
public static class LevelSession
{
    public const int LevelCount = 20;
    private const string UnlockedLevelCountKey = "ToyRoomSmash.UnlockedLevelCount";
    private const string BestStarsKeyPrefix = "ToyRoomSmash.BestStars.";

    /// <summary>0 tabanlı seviye numarası.</summary>
    public static int LevelIndex { get; private set; }
    public static bool IsLastLevel => LevelIndex >= LevelCount - 1;
    /// <summary>Oyuncunun erişebildiği seviye sayısı. İlk açılışta yalnızca seviye 1 açıktır.</summary>
    public static int UnlockedLevelCount => Mathf.Clamp(PlayerPrefs.GetInt(UnlockedLevelCountKey, 1), 1, LevelCount);
    private static bool openLevelSelectOnMenuLoad;

    /// <summary>Seviyenin seed'i: LevelGenerator'daki temel seed + seviye numarası.</summary>
    public static int SeedFor(int baseSeed)
    {
        return baseSeed + LevelIndex;
    }

    public static void SelectLevel(int index)
    {
        if (index < 0 || index >= LevelCount || index >= UnlockedLevelCount)
        {
            Debug.LogWarning($"[LevelSession] Level {index + 1} is locked or invalid.");
            return;
        }

        Time.timeScale = 1f;
        if (!SceneTransition.Load("Arena")) return;
        LevelIndex = index;
    }

    public static void CompleteCurrentLevel()
    {
        int unlockedCount = Mathf.Min(LevelIndex + 2, LevelCount);
        if (unlockedCount <= UnlockedLevelCount) return;

        PlayerPrefs.SetInt(UnlockedLevelCountKey, unlockedCount);
        PlayerPrefs.Save();
    }

    public static int BestStars(int index)
    {
        if (index < 0 || index >= LevelCount) return 0;
        return Mathf.Clamp(PlayerPrefs.GetInt(BestStarsKeyPrefix + index, 0), 0, 3);
    }

    public static void RecordWin(int stars)
    {
        int earned = Mathf.Clamp(stars, 1, 3);
        if (earned <= BestStars(LevelIndex)) return;

        PlayerPrefs.SetInt(BestStarsKeyPrefix + LevelIndex, earned);
        PlayerPrefs.Save();
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(UnlockedLevelCountKey);
        for (int index = 0; index < LevelCount; index++)
            PlayerPrefs.DeleteKey(BestStarsKeyPrefix + index);
        PlayerPrefs.Save();
        LevelIndex = 0;
    }

    public static bool ConsumeOpenLevelSelectRequest()
    {
        bool requested = openLevelSelectOnMenuLoad;
        openLevelSelectOnMenuLoad = false;
        return requested;
    }

    public static void ReturnToMenu()
    {
        OpenMenu(showLevelSelect: false);
    }

    public static void ReturnToLevelSelect()
    {
        OpenMenu(showLevelSelect: true);
    }

    public static void Retry()
    {
        Reload();
    }

    public static void NextLevel()
    {
        if (IsLastLevel)
        {
            ReturnToLevelSelect();
            return;
        }

        if (LevelIndex + 1 >= UnlockedLevelCount)
        {
            Debug.LogWarning("[LevelSession] Next level is still locked.");
            return;
        }

        Time.timeScale = 1f;
        if (!SceneTransition.Load(SceneManager.GetActiveScene().name)) return;
        LevelIndex++;
    }

    private static void OpenMenu(bool showLevelSelect)
    {
        Time.timeScale = 1f;
        if (!SceneTransition.Load("MainMenu")) return;
        openLevelSelectOnMenuLoad = showLevelSelect;
    }

    private static void Reload()
    {
        Time.timeScale = 1f;
        SceneTransition.Load(SceneManager.GetActiveScene().name);
    }

    // Editörde "Enter Play Mode" ayarlarında domain reload kapalıysa statik değerler oyunlar arası
    // taşınır; her oynatmada seviye 1'den başlasın.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        LevelIndex = 0;
        openLevelSelectOnMenuLoad = false;
    }
}
