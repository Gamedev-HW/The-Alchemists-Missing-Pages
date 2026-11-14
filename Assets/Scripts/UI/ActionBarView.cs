using UnityEngine;
using UnityEngine.UI;

namespace PotionLab
{
    /// <summary>
    /// The three round buttons. Shows the ingredient's actions while one is held and the cauldron's otherwise,
    /// greys them out while an action runs, and turns POUR into STOP while pouring.
    /// Pressing a button only raises an event; the actions listen for it.
    /// </summary>
    public class ActionBarView : MonoBehaviour
    {
        [SerializeField] Workbench workbench;
        [SerializeField] ActionRunner runner;
        [SerializeField] BrewActionEvent buttonPressed;

        [Header("Buttons, left to right")]
        [SerializeField] Button[] buttons = new Button[3];
        [SerializeField] Image[] plaques = new Image[3];
        [SerializeField] BrewActionId[] ingredientActions = { BrewActionId.Slice, BrewActionId.Heat, BrewActionId.DropInPot };
        [SerializeField] BrewActionId[] cauldronActions = { BrewActionId.Pour, BrewActionId.Mix, BrewActionId.Serve };

        [Header("Look")]
        [SerializeField] bool hebrewPlaques;
        [Tooltip("One plaque per action, in the order of BrewActionId: slice, heat, to the pot, pour, stop, mix, serve.")]
        [SerializeField] Sprite[] englishPlaqueSprites = new Sprite[7];
        [SerializeField] Sprite[] hebrewPlaqueSprites = new Sprite[7];
        [SerializeField] Sprite normalButton;
        [Tooltip("The red button for an action that can be stopped (STOP while pouring).")]
        [SerializeField] Sprite stopButton;

        BrewActionId[] shown;

        void Awake()
        {
            shown = new BrewActionId[buttons.Length];
            for (int i = 0; i < buttons.Length; i++)
            {
                int slot = i;
                buttons[i].onClick.AddListener(() => buttonPressed.Raise(shown[slot]));
            }
        }

        void OnEnable()
        {
            workbench.Changed += Refresh;
            runner.RunningChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            workbench.Changed -= Refresh;
            runner.RunningChanged -= Refresh;
        }

        void Refresh()
        {
            BrewActionId[] actions = workbench.IsHolding ? ingredientActions : cauldronActions;
            BrewAction running = runner.Running;
            for (int i = 0; i < buttons.Length; i++)
            {
                BrewActionId action = actions[i];
                bool canStop = running != null && running.Button == action && running.StopButton.HasValue;
                shown[i] = canStop ? running.StopButton.Value : action;
                plaques[i].sprite = PlaqueFor(shown[i]);
                buttons[i].interactable = running == null || canStop;
                ((Image)buttons[i].targetGraphic).sprite = canStop ? stopButton : normalButton;
            }
        }

        Sprite PlaqueFor(BrewActionId action)
        {
            Sprite[] sprites = hebrewPlaques ? hebrewPlaqueSprites : englishPlaqueSprites;
            int index = (int)action;
            return index < sprites.Length ? sprites[index] : null;
        }
    }
}
