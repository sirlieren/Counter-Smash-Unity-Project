using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sahne yeniden yüklendiğinde de yaşayan seviye durumu: hangi seviyedeyiz, tekrar/sonraki nasıl yüklenir.
/// </summary>
public static class LevelSession
{
    /// <summary>0 tabanlı seviye numarası.</summary>
    public static int LevelIndex { get; private set; }

    /// <summary>Seviyenin seed'i: LevelGenerator'daki temel seed + seviye numarası.</summary>
    public static int SeedFor(int baseSeed)
    {
        return baseSeed + LevelIndex;
    }

    public static void Retry()
    {
        Reload();
    }

    public static void NextLevel()
    {
        LevelIndex++;
        Reload();
    }

    private static void Reload()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Editörde "Enter Play Mode" ayarlarında domain reload kapalıysa statik değerler oyunlar arası
    // taşınır; her oynatmada seviye 1'den başlasın.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        LevelIndex = 0;
    }
}
