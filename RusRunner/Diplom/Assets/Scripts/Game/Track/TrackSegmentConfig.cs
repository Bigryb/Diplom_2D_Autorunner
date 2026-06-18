using UnityEngine;

namespace RusRunner.Game.Track
{
    [CreateAssetMenu(
        fileName = "TrackSegmentConfig",
        menuName = "RusRunner/Track Segment Config",
        order = 1)]
    public sealed class TrackSegmentConfig : ScriptableObject
    {
        [Min(4f)] public float Length = 8f;
        [Range(0f, 1f)] public float ObstacleDensity = 0.3f;
        [Range(0f, 1f)] public float CollectibleDensity = 0.2f;
        [Range(0f, 1f)] public float DifficultyWeight = 0.5f;
    }
}
