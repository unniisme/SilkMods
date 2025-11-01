# MultiCustomizer

A plugin that allows loading in multiple texture packs and switching between them conditionally

## How It Works

The AddTexturePack function allows you to register a new texture pack to be loaded into the system. \\
Make sure that your sprite pack is located at `BepInEx/plugins/<name>/Customizer` \\
You can then activate the texture pack by setting the IsActive bool of the MaterialConditional obtained on using AddTexturePack \\

For a demo, check out [WandererTexturePack](../WandererTexturePack)