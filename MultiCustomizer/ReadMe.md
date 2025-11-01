# MultiCustomizer

A plugin that allows loading in multiple texture packs and switching between them conditionally

## How It Works

The AddTexturePack function allows you to register a new texture pack to be loaded into the system. \\
Make sure that your sprite pack is located at `BepInEx/plugins/<name>/Customizer` \\
You can then activate the texture pack by setting the IsActive bool of the MaterialConditional obtained on using AddTexturePack \\


For a demo, check out [WandererTexturePack](../WandererTexturePack)


## TODO
- Loading in too many texture packs could cause increase in load time and memory usage
    - Possible to do something more dynamic? Use more threads?
- Overriding : Allow overriding parts of a texture pack with another
- Overlaying : Allow loading multiple texture packs on top of each other
- Any combination of the above 2, so that full texture packs for each condition need not be created, only sufficient overrides and overlays


------
Many thanks to RatherChaotic and his [Customizer](https://github.com/RatherChaotic/SSCustomizer), which was used as a base for this mod.
