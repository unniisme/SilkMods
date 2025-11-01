using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiCustomizer
{
    public class TextureState
    {
        public string Name { get; }

        public Dictionary<Material, Texture> MaterialsToTextures { get; }


        public TextureState(string name)
        {
            Name = name;
            MaterialsToTextures = new Dictionary<Material, Texture>();
        }
    }


    [BepInPlugin("com.unniisme.MultiCustomizer", "MultiCustomizer", "1.0.0")]
    public class MultiCustomizer : BaseUnityPlugin
    {
        internal new static ManualLogSource Logger;
        internal static object _multiCustomizerLock = new object();

        private static readonly Dictionary<string, TextureState> TextureStates =
               new Dictionary<string, TextureState>();

        private static readonly List<TextureState> ActiveStates = new List<TextureState>();

        // Default silksong sprites
        private static TextureState defaultTextureState = new TextureState("");
        private static TextureState currentTextureState = defaultTextureState;


        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo("Plugin MultiCustomizer loaded");

            Harmony harmony = Harmony.CreateAndPatchAll(typeof(MultiCustomizer), null);

            // Conflicts with customizer
            // Shoutout to RatherChaotic, whose Customizer mod this mod is inspired out of
            // Check it out at https://github.com/RatherChaotic/SSCustomizer
            if (Chainloader.PluginInfos.ContainsKey("customizer"))
            {
                Logger.LogError("Detected customizer. Error: MultiCustomizer is not compatable with Customizer");
                Logger.LogError("Aborting MultiCustomizer");
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LoadTextures();
            Logger.LogInfo($"Loaded scene: {scene.name}");
        }
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // ------------ APIs -----------------------------------

        /// <summary>
        /// Add a new Texture Pack to the list of texture packs to load
        /// make sure to keep your sprite pack in the following directory:
        /// BepInEx/plugins/<name>/Customizer
        /// 
        /// Any texture not present will be replaced by the default texture on loading
        /// </summary>
        /// <param name="name">The name of the texture pack</param> 
        public static MaterialConditional AddTexturePack(string name)
        {
            TextureState newState =  new TextureState(name);
            MaterialConditional conditional = new MaterialConditional(newState);
            conditional.ActivityChangedEvent += HandleMaterialConditional;

            TextureStates.Add(name, newState);
            return conditional;
        }


        // ------------ Internals -------------------------------

        private static void HandleMaterialConditional(TextureState state, bool isActive)
        {
            if (TextureStates.ContainsKey(state.Name))
            {
                if (isActive)
                {
                    if (!ActiveStates.Contains(state))
                        ActiveStates.Add(state);

                    SetTexturePack(state.Name);
                }
                else
                {
                    if (ActiveStates.Contains(state))
                    {
                        ActiveStates.Remove(state);

                        if (currentTextureState.Name == state.Name)
                        {
                            if (ActiveStates.IsNullOrEmpty())
                            {
                                ResetTexturePack();
                            }
                            else
                            {
                                SetTexturePack(ActiveStates.Last().Name);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Sets the texture packs back to default
        /// </summary>
        private static void ResetTexturePack()
        {
            lock (_multiCustomizerLock)
            {
                if (currentTextureState.Name != "")
                {
                    Logger.LogDebug("Switching to texture pack " + currentTextureState.Name);

                    currentTextureState = defaultTextureState;

                    foreach (Material material in defaultTextureState.MaterialsToTextures.Keys)
                    {
                        if (currentTextureState.MaterialsToTextures.ContainsKey(material))
                        {
                            material.mainTexture = currentTextureState.MaterialsToTextures[material];
                        }
                        else
                        {
                            material.mainTexture = defaultTextureState.MaterialsToTextures[material];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Switch to a texture pack
        /// If textures are missing they are replaced with the default ones
        /// </summary>
        /// <param name="name"></param>
        private static void SetTexturePack(string name)
        {
            lock (_multiCustomizerLock)
            {
                if (currentTextureState.Name != name)
                {
                    Logger.LogDebug("Switching to texture pack " + currentTextureState.Name);

                    currentTextureState = TextureStates[name];

                    foreach (Material material in defaultTextureState.MaterialsToTextures.Keys)
                    {
                        if (currentTextureState.MaterialsToTextures.ContainsKey(material))
                        {
                            material.mainTexture = currentTextureState.MaterialsToTextures[material];
                        }
                        else
                        {
                            material.mainTexture = defaultTextureState.MaterialsToTextures[material];
                        }
                    }
                }
            }
        }


        private static void LoadTextures()
        {
            tk2dSpriteCollectionData[] collectionsArray = Resources.FindObjectsOfTypeAll<tk2dSpriteCollectionData>();
            Dictionary<string, tk2dSpriteCollectionData> collections = collectionsArray.ToDictionary((tk2dSpriteCollectionData c) => c.name, (tk2dSpriteCollectionData c) => c);

            GetDefaultTextures(collectionsArray);
            GetTextureStates(collections);
        }

        private static void GetDefaultTextures(tk2dSpriteCollectionData[] collectionsArray)
        {
            Logger.LogDebug("Loading default textures");
            // Load in defaults as whatever customizer loaded + unloaded vanilla textures
            foreach (tk2dSpriteCollectionData sprite in collectionsArray)
            {
                foreach (Material material in sprite.materials)
                {
                    lock (_multiCustomizerLock)
                    {
                        if (!defaultTextureState.MaterialsToTextures.ContainsKey(material))
                            defaultTextureState.MaterialsToTextures[material] = material.mainTexture;
                    }
                }
            }
        }

        private static void GetTextureStates(Dictionary<string, tk2dSpriteCollectionData> collections)
        {
            Logger.LogDebug("Entering GetTextureStates");

            Directory.GetDirectories(BepInEx.Paths.PluginPath, "Customizer", SearchOption.AllDirectories).ToList();

            // Sift through each texture pack mod folder that has been registered and look for a Customizer folder
            foreach (string textureStateName in TextureStates.Keys)
            {
                Logger.LogDebug("textureStateName : " + textureStateName);

                string packDir = Path.Combine(BepInEx.Paths.PluginPath, textureStateName, "Customizer");

                Logger.LogDebug("packDir : " + packDir);


                if (Directory.Exists(packDir))
                {
                    Logger.LogDebug("Loading Textures for " + textureStateName);

                    GetTextureState(TextureStates[textureStateName], packDir, collections);
                }

            }
        }

        // Saves this texture pack to memory
        private static void GetTextureState(TextureState textureState, string dir, Dictionary<string, tk2dSpriteCollectionData> spriteCollection)
        {
            Logger.LogDebug("Setting textures for pack " + textureState.Name);

            foreach (string subDir in Directory.GetDirectories(dir))
            {
                Logger.LogDebug("subDir : " + subDir);
                string fileName = Path.GetFileName(subDir);
                Logger.LogDebug("fileName : " + fileName);
                if (spriteCollection.ContainsKey(fileName))
                {
                    foreach (Material material in spriteCollection[fileName].materials)
                    {

                        string atlasFile = Path.Combine(subDir, material.mainTexture.name + ".png");
                        Logger.LogDebug("atlasFile : " + atlasFile);
                        if (File.Exists(atlasFile))
                        {
                            lock (_multiCustomizerLock)
                            {
                                Logger.LogDebug("Loading file " + atlasFile);
                                byte[] data = File.ReadAllBytes(atlasFile);
                                Texture2D texture2D = new Texture2D(2, 2);
                                texture2D.name = material.mainTexture.name;
                                if (texture2D.LoadImage(data))
                                {
                                    textureState.MaterialsToTextures[material] = texture2D;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
