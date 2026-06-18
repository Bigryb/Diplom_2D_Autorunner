using System.Collections.Generic;
using UnityEngine;

namespace RusRunner.Game.Track
{
    public sealed class TrackGenerator : MonoBehaviour
    {
        [SerializeField] private List<TrackSegmentConfig> segmentConfigs = new List<TrackSegmentConfig>();
        [SerializeField] private int initialSegments = 8;
        [SerializeField] private float despawnDistance = 24f;
        [SerializeField] private float lookAheadDistance = 40f;

        private readonly Queue<float> _spawnedSegments = new Queue<float>();
        private Transform _runner;
        private float _spawnX;
        private float _difficulty01;

        public void Initialize(Transform runnerTransform)
        {
            _runner = runnerTransform;
            _spawnX = 0f;
            _spawnedSegments.Clear();

            for (var i = 0; i < initialSegments; i++)
            {
                SpawnNextSegment();
            }
        }

        private void Update()
        {
            if (_runner == null || segmentConfigs.Count == 0)
            {
                return;
            }

            while (_spawnX - _runner.position.x < lookAheadDistance)
            {
                SpawnNextSegment();
            }

            while (_spawnedSegments.Count > 0 && _runner.position.x - _spawnedSegments.Peek() > despawnDistance)
            {
                _spawnedSegments.Dequeue();
                // Here pooled segment objects should be returned to pool.
            }
        }

        public void SetDifficulty(float difficulty01)
        {
            _difficulty01 = Mathf.Clamp01(difficulty01);
        }

        private void SpawnNextSegment()
        {
            var config = SelectSegmentByDifficulty();
            var segmentStartX = _spawnX;

            // Placeholder logic:
            // instantiate or fetch segment prefab from pool by config.

            _spawnedSegments.Enqueue(segmentStartX);
            _spawnX += config.Length;
        }

        private TrackSegmentConfig SelectSegmentByDifficulty()
        {
            var bestScore = float.MinValue;
            TrackSegmentConfig best = segmentConfigs[0];

            for (var i = 0; i < segmentConfigs.Count; i++)
            {
                var cfg = segmentConfigs[i];
                var score = 1f - Mathf.Abs(cfg.DifficultyWeight - _difficulty01);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = cfg;
                }
            }

            return best;
        }
    }
}
