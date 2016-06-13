using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace PatchKit.Unity.AssetBundles
{
    public partial class PatchKitAssetBundleasdas
    {
        private class PatchKitAssetBundleCache
        {
            private static string CachePath
            {
                get { return Path.Combine(Application.persistentDataPath, "pk_asset_bundles_cache"); }
            }

            private static string CacheFilePath
            {
                get { return Path.Combine(CachePath, "cache.json"); }
            }

            private static void EnsureThatCacheDirectoryIsCreated()
            {
                if (!Directory.Exists(CachePath))
                {
                    Directory.CreateDirectory(CachePath);
                }
            }

            private static readonly Dictionary<string, int> Versions;

            static PatchKitAssetBundleCache()
            {
                try
                {
                    if (File.Exists(CacheFilePath))
                    {
                        Versions =
                            JsonConvert.DeserializeObject(File.ReadAllText(CacheFilePath)) as Dictionary<string, int>;
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    Debug.LogWarning("Failed to load PatchKit Asset Bundles cache.");
                }

                if (Versions == null)
                {
                    Versions = new Dictionary<string, int>();
                }
            }

            private static void SaveCache()
            {
                EnsureThatCacheDirectoryIsCreated();

                try
                {
                    File.WriteAllText(CacheFilePath, JsonConvert.SerializeObject(Versions));
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    Debug.LogWarning("Failed to save PatchKit Asset Bundles cache.");
                }
            }

            /// <summary>
            /// Returns the path for asset bundle cache location.
            /// </summary>
            /// <param name="secret">Asset bundle secret.</param>
            public static string GetAssetBundlePath(string secret)
            {
                return Path.Combine(CachePath, string.Format("{0}.assetbundle", secret));
            }

            /// <summary>
            /// Checks whether asset bundle exists in cache.
            /// </summary>
            /// <param name="secret">Asset bundle secret.</param>
            public static bool Exists(string secret)
            {
                if (File.Exists(GetAssetBundlePath(secret)))
                {
                    if (Versions.ContainsKey(secret))
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Returns version of asset bundle. If asset bundle doesn't exist it returns <c>null</c>.
            /// </summary>
            /// <param name="secret">Asset bundle secret.</param>
            public static int? GetAssetBundleVersion(string secret)
            {
                if (Exists(secret))
                {
                    return Versions[secret];
                }
                return null;
            }

            /// <summary>
            /// Sets version of asset bundle.
            /// </summary>
            /// <param name="secret">Asset bundle secret.</param>
            /// <param name="version">Asset bundle version.</param>
            internal static void SetAssetBundleVersion(string secret, int version)
            {
                Versions[secret] = version;
                SaveCache();
            }

            /// <summary>
            /// Clears information about asset bundle version.
            /// </summary>
            /// <param name="secret">Asset bundle secret.</param>
            internal static void ClearAssetBundleVersion(string secret)
            {
                Versions.Remove(secret);
                SaveCache();
            }

            /// <summary>
            /// Loads asset bundle from cache.
            /// </summary>
            /// <param name="secret">Asset bundle secret.</param>
            /// <returns>
            /// Loaded asset bundle. 
            /// If asset bundle doesn't exist in the cache, returns <c>null</c>.
            /// </returns>
            public static UnityEngine.AssetBundle Load(string secret)
            {
                return Exists(secret) ? UnityEngine.AssetBundle.LoadFromFile(GetAssetBundlePath(secret)) : null;
            }

            /// <summary>
            /// Asynchronously loads asset bundle from cache.
            /// </summary>
            /// <param name="secret">Asset bundle secret.</param>
            /// <returns>
            /// Asynchronous create request for an assset bundle. Use AssetBundleCreateRequest.assetBundle property to get an asset bundle once it is loaded.
            /// If asset bundle doesn't exist in the cache, returns <c>null</c>.
            /// </returns>
            public static AssetBundleCreateRequest LoadAsync(string secret)
            {
                return Exists(secret) ? UnityEngine.AssetBundle.LoadFromFileAsync(GetAssetBundlePath(secret)) : null;
            }
        }
    }
}