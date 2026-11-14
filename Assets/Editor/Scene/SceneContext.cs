using System.Collections.Generic;
using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>What the scene builders share while they build: the data assets and the objects made so far.</summary>
    public class SceneContext
    {
        public readonly GameData data;

        /// <summary>Scene objects that other components refer to by type (Workbench, Cauldron, SpellBeam...).</summary>
        public readonly List<Object> services = new List<Object>();

        public Camera camera;
        public BackButton backButton;
        public ShelfItem shelfItemPrefab;
        public Transform workSpot;
        public readonly List<MonoBehaviour> backHandlers = new List<MonoBehaviour>();

        public SceneContext(GameData data)
        {
            this.data = data;
        }

        public T Add<T>(T service) where T : Object
        {
            services.Add(service);
            return service;
        }

        /// <summary>Data assets that fields get by their name (every "speech" field gets the Student Says event...).</summary>
        public Dictionary<string, Object> AssetsByFieldName()
        {
            return new Dictionary<string, Object>
            {
                { "library", data.library },
                { "speech", data.speech },
                { "journal", data.journal },
                { "buttonPressed", data.buttonPressed },
                { "shelfItemClicked", data.shelfItemClicked },
                { "cauldronClicked", data.cauldronClicked },
                { "toggleRequested", data.recipeBookToggled },
                { "potionBrewed", data.potionBrewed },
                { "potionFailed", data.potionFailed }
            };
        }
    }
}
