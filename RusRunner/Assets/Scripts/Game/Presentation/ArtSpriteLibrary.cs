using System.Collections.Generic;
using UnityEngine;

namespace RusRunner.Game.Presentation
{
    public static class ArtSpriteLibrary
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Load(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            if (Cache.TryGetValue(relativePath, out var cached))
            {
                return cached;
            }

            var sprite = Resources.Load<Sprite>("Art/" + relativePath);
            Cache[relativePath] = sprite;
            return sprite;
        }
    }
}
