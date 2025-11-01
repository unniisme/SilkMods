using BepInEx;

namespace MultiCustomizer
{
    /// <summary>
    /// Sample pack to demonstrate how to use MultiCustomizer
    /// This pack loads in a texture pack just for the wanderer crest
    /// Sprites for the texture pack is to be kept in [BepInEx]/plugins/WandererTexturePack/Customizer
    /// </summary>

    [BepInPlugin("com.unniisme.WandererTexturePack", "WandererTexturePack", "1.0.0")]
    [BepInDependency("com.unniisme.MultiCustomizer", BepInDependency.DependencyFlags.HardDependency)]
    public class WandererTexturePack : BaseUnityPlugin
    {
        // Name of the texture pack and the folder to keep it in in plugins
        private string wandererStateName = "WandererTexturePack";

        // Object that tells MultiCustomizer to load in this texture
        private MaterialConditional wandererConditional;

        void Awake()
        {
            Logger.LogInfo("Plugin Wanderer Texture Pack loaded");

            // Call to initialize
            wandererConditional = MultiCustomizer.AddTexturePack(wandererStateName);
        }

        void Update()
        {
            // Call to change to this texture pack
            // Your condition to change texture packs goes here
            // This function need not be in Update, any updation of TextureConditional.IsActive does the job
            wandererConditional.IsActive = PlayerData.instance.CurrentCrestID == "Wanderer";
        }
    }
}
