using UnityEngine;

/// <summary>
/// ScriptableObject containing all audio clip references for the game.
/// Organized by category: BGM, Event sounds, Damage sounds.
/// Each sound has an adjustable volume (0-1).
/// </summary>
[CreateAssetMenu(fileName = "GameAudioSettings", menuName = "SurvivorGame/Audio/Audio Settings")]
public class GameAudioSettings : ScriptableObject
{
    [System.Serializable]
    public class AudioEntry
    {
        [Tooltip("Audio clip to play")]
        public AudioClip clip;

        [Tooltip("Volume multiplier (0-1)")]
        [Range(0f, 1f)]
        public float volume = 1f;
    }

    [Header("Background Music")]
    public AudioEntry defaultBGM = new AudioEntry();
    public AudioEntry menuBGM = new AudioEntry();
    public AudioEntry gameOverBGM = new AudioEntry();

    [Header("Damage Sounds")]
    public AudioEntry enemyHitSound = new AudioEntry();
    public AudioEntry critHitSound = new AudioEntry();
    public AudioEntry playerHitSound = new AudioEntry();
    public AudioEntry areaExplosionSound = new AudioEntry();

    [Header("Event Sounds")]
    public AudioEntry enemyDeathSound = new AudioEntry();
    public AudioEntry levelUpSound = new AudioEntry();
    public AudioEntry gameOverSound = new AudioEntry();
    public AudioEntry pauseSound = new AudioEntry();
    public AudioEntry resumeSound = new AudioEntry();

    // Helper methods to get clip and volume
    public (AudioClip clip, float volume) GetDefaultBGM() => (defaultBGM.clip, defaultBGM.volume);
    public (AudioClip clip, float volume) GetMenuBGM() => (menuBGM.clip, menuBGM.volume);
    public (AudioClip clip, float volume) GetGameOverBGM() => (gameOverBGM.clip, gameOverBGM.volume);

    public (AudioClip clip, float volume) GetEnemyHitSound() => (enemyHitSound.clip, enemyHitSound.volume);
    public (AudioClip clip, float volume) GetCritHitSound() => (critHitSound.clip, critHitSound.volume);
    public (AudioClip clip, float volume) GetPlayerHitSound() => (playerHitSound.clip, playerHitSound.volume);
    public (AudioClip clip, float volume) GetAreaExplosionSound() => (areaExplosionSound.clip, areaExplosionSound.volume);

    public (AudioClip clip, float volume) GetEnemyDeathSound() => (enemyDeathSound.clip, enemyDeathSound.volume);
    public (AudioClip clip, float volume) GetLevelUpSound() => (levelUpSound.clip, levelUpSound.volume);
    public (AudioClip clip, float volume) GetGameOverSound() => (gameOverSound.clip, gameOverSound.volume);
    public (AudioClip clip, float volume) GetPauseSound() => (pauseSound.clip, pauseSound.volume);
    public (AudioClip clip, float volume) GetResumeSound() => (resumeSound.clip, resumeSound.volume);
}
