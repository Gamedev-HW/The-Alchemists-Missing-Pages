using UnityEngine;

namespace PotionLab
{
    /// <summary>An action started by one of the three bar buttons. The buttons don't know the actions: they only raise an event.</summary>
    public abstract class ButtonAction : BrewAction
    {
        [SerializeField] BrewActionEvent buttonPressed;

        protected virtual void OnEnable()
        {
            buttonPressed.Raised += OnButtonPressed;
        }

        protected virtual void OnDisable()
        {
            buttonPressed.Raised -= OnButtonPressed;
        }

        protected virtual void OnButtonPressed(BrewActionId id)
        {
            if (id == Button) Request();
        }
    }
}
