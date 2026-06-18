using RusRunner.Game.Runner;
using UnityEngine;

namespace RusRunner.Game.Presentation
{
    public sealed class ParallaxLoopLayer : MonoBehaviour
    {
        [SerializeField] private float speedFactor = 0.2f;
        [SerializeField] private float tileSpacing = 24f;
        [SerializeField] private Transform[] tiles;

        private RunnerBootstrap _runnerBootstrap;
        private Camera _mainCamera;

        public void Configure(float factor, float spacing, Transform[] layerTiles)
        {
            speedFactor = factor;
            tileSpacing = spacing;
            tiles = layerTiles;
        }

        private void Awake()
        {
            _runnerBootstrap = FindFirstObjectByType<RunnerBootstrap>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (tiles == null || tiles.Length == 0)
            {
                return;
            }

            if (_runnerBootstrap == null)
            {
                _runnerBootstrap = FindFirstObjectByType<RunnerBootstrap>();
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            var speed = _runnerBootstrap != null ? _runnerBootstrap.CurrentSpeed : 0f;
            var delta = speed * speedFactor * Time.deltaTime;
            if (delta <= 0f)
            {
                return;
            }

            for (var i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null)
                {
                    tiles[i].localPosition += Vector3.left * delta;
                }
            }

            RecycleTiles();
        }

        private void RecycleTiles()
        {
            if (_mainCamera == null)
            {
                return;
            }

            var halfViewWidth = _mainCamera.orthographic
                ? _mainCamera.orthographicSize * _mainCamera.aspect
                : 10f;

            var leftBound = _mainCamera.transform.position.x - halfViewWidth - tileSpacing;
            var rightMostX = GetRightMostTileX();

            for (var i = 0; i < tiles.Length; i++)
            {
                var tile = tiles[i];
                if (tile == null)
                {
                    continue;
                }

                if (transform.TransformPoint(tile.localPosition).x <= leftBound)
                {
                    tile.localPosition = new Vector3(rightMostX + tileSpacing, tile.localPosition.y, tile.localPosition.z);
                    rightMostX = tile.localPosition.x;
                }
            }
        }

        private float GetRightMostTileX()
        {
            var rightMost = float.MinValue;

            for (var i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != null && tiles[i].localPosition.x > rightMost)
                {
                    rightMost = tiles[i].localPosition.x;
                }
            }

            return rightMost;
        }
    }
}