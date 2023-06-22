using System;
using PluginAPI.Core;
using PluginAPI.Core.Factories;
using PluginAPI.Core.Interfaces;

namespace CedMod
{
    public class CedModPlayerFactory: PlayerFactory
    {
        public override Type BaseType { get; } = typeof(CedModPlayer);
        public override Player Create(IGameComponent component) => new CedModPlayer(component);
    }
}