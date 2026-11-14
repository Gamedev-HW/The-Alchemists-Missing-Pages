using UnityEngine;
using UnityEngine.UI;

namespace PotionLab
{
    /// <summary>
    /// The name panel on the right: the held ingredient (with raw / sliced / heated badges), or the cauldron and
    /// what's in it. Also shows a badge for a brewed or failed potion.
    /// </summary>
    public class SelectionPanelView : MonoBehaviour
    {
        [SerializeField] Workbench workbench;
        [SerializeField] Cauldron cauldron;
        [SerializeField] GameEvent potionBrewed;
        [SerializeField] GameEvent potionFailed;

        [Header("Widgets")]
        [SerializeField] Image icon;
        [SerializeField] Text title;
        [SerializeField] Image[] badges = new Image[3];

        [Header("Sprites")]
        [SerializeField] Sprite cauldronIcon;
        [SerializeField] Sprite rawBadge;
        [SerializeField] Sprite slicedBadge;
        [SerializeField] Sprite heatedBadge;
        [SerializeField] Sprite inPotBadge;
        [SerializeField] Sprite brewedBadge;
        [SerializeField] Sprite failedBadge;

        [Header("Text")]
        [SerializeField] string emptyCauldronText = "Cauldron (empty)";
        [Tooltip("{0} = ingredients in it, {1} = times mixed.")]
        [SerializeField] string cauldronText = "Cauldron: {0} in, stirred {1}x";

        void OnEnable()
        {
            workbench.Changed += Refresh;
            cauldron.Changed += Refresh;
            potionBrewed.Raised += ShowBrewed;
            potionFailed.Raised += ShowFailed;
            Refresh();
        }

        void OnDisable()
        {
            workbench.Changed -= Refresh;
            cauldron.Changed -= Refresh;
            potionBrewed.Raised -= ShowBrewed;
            potionFailed.Raised -= ShowFailed;
        }

        void Refresh()
        {
            if (workbench.IsHolding) ShowIngredient(workbench.Held);
            else ShowCauldron();
        }

        void ShowIngredient(HeldIngredient held)
        {
            Prep prep = held.Prep;
            SetIcon(held.Data.SpriteFor(prep));
            title.text = held.Data.displayName;
            SetBadges(prep == Prep.Raw ? rawBadge : null,
                      prep.Includes(Prep.Sliced) ? slicedBadge : null,
                      prep.Includes(Prep.Heated) ? heatedBadge : null);
        }

        void ShowCauldron()
        {
            int count = cauldron.Ingredients.Count;
            SetIcon(cauldronIcon);
            title.text = cauldron.HasLiquid ? string.Format(cauldronText, count, cauldron.MixCount) : emptyCauldronText;
            SetBadges(count > 0 ? inPotBadge : null);
        }

        void ShowBrewed()
        {
            SetBadges(brewedBadge);
        }

        void ShowFailed()
        {
            SetBadges(failedBadge);
        }

        void SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        /// <summary>Shows the given badges left to right; empty entries are skipped.</summary>
        void SetBadges(params Sprite[] sprites)
        {
            int used = 0;
            foreach (Sprite sprite in sprites)
            {
                if (sprite == null || used >= badges.Length) continue;
                badges[used].sprite = sprite;
                badges[used].enabled = true;
                used++;
            }
            for (int i = used; i < badges.Length; i++) badges[i].enabled = false;
        }
    }
}
