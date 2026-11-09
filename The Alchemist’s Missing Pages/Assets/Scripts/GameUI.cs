using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PotionLab
{
    /// <summary>
    /// Everything on the Canvas: the three round buttons and their plaques, the name panel, the speech bubble,
    /// the goal line and the recipe book with the journal. Built and wired by "Potion Lab ▸ 2 - Create Game UI".
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        public enum Plaque { Slice, Heat, ToPot, Pour, Stop, Mix, Serve }

        [Header("Language")]
        [Tooltip("Use the Hebrew plaque images on the buttons.")]
        public bool hebrewPlaques;

        [Header("Bar")]
        public Button[] buttons = new Button[3];
        public Image[] plaques = new Image[3];

        [Header("Name panel")]
        public Image panelIcon;
        public Text panelName;
        public Image[] badges = new Image[3];

        [Header("Messages")]
        public GameObject bubble;
        public Text bubbleText;
        public Text goalText;

        [Header("Recipe book")]
        public GameObject book;
        public Text bookTitle;
        public Text bookRecipe;
        public Text bookJournal;
        public Text bookHint;

        [Header("Phones")]
        [Tooltip("On phones these panels are drawn bigger so the text is easy to read on a small screen.")]
        [Range(1f, 1.5f)] public float phoneScale = 1.2f;
        public RectTransform[] phoneScaled = new RectTransform[0];

        [Header("Sprites")]
        public Sprite buttonNormal;
        public Sprite buttonActive;
        public Sprite cauldronIcon;
        public Sprite badgeRaw, badgeSliced, badgeHeated, badgeInPot, badgeOk, badgeFail;
        [Tooltip("Order: slice, heat, to the pot, pour, stop, mix, serve")]
        public Sprite[] plaquesEnglish = new Sprite[7];
        [Tooltip("Order: slice, heat, to the pot, pour, stop, mix, serve")]
        public Sprite[] plaquesHebrew = new Sprite[7];

        readonly List<string> journal = new List<string>();
        Coroutine sayRoutine;
        Coroutine openRoutine;
        bool phoneScaleApplied;

        public bool BookOpen { get { return book != null && book.activeSelf; } }

        void Awake()
        {
            ApplyPhoneScale();
        }

        /// <summary>
        /// Enlarges the book, the speech bubble and the goal line on phones. The text inside is re-rendered at the
        /// bigger size (instead of just being stretched) so it stays sharp.
        /// </summary>
        void ApplyPhoneScale()
        {
            if (phoneScaleApplied || phoneScaled == null || !Application.isMobilePlatform || phoneScale <= 1.001f) return;
            phoneScaleApplied = true;
            foreach (RectTransform panel in phoneScaled)
            {
                if (panel == null) continue;
                panel.localScale = Vector3.one * phoneScale;
                foreach (Text t in panel.GetComponentsInChildren<Text>(true))
                {
                    t.fontSize = Mathf.RoundToInt(t.fontSize * phoneScale);
                    t.resizeTextMinSize = Mathf.RoundToInt(t.resizeTextMinSize * phoneScale);
                    t.resizeTextMaxSize = Mathf.RoundToInt(t.resizeTextMaxSize * phoneScale);
                    RectTransform r = t.rectTransform;
                    r.sizeDelta *= phoneScale;
                    r.localScale /= phoneScale;
                }
            }
        }

        public void Bind(PotionGame game)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null) continue;
                int index = i;
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(() => game.OnButton(index));
            }
            if (bubble != null) bubble.SetActive(false);
            if (book != null) book.SetActive(false);
            if (bookJournal != null) bookJournal.text = "(nothing yet - every attempt is written here)";
            if (bookHint != null)
                bookHint.text = Application.isMobilePlatform
                    ? "Tap outside the book or press Back to close it"
                    : "Click outside the book or press Esc to close it";
        }

        Sprite PlaqueSprite(Plaque p)
        {
            Sprite[] set = hebrewPlaques ? plaquesHebrew : plaquesEnglish;
            int i = (int)p;
            return set != null && i < set.Length ? set[i] : null;
        }

        void SetPlaque(int slot, Plaque p)
        {
            if (slot < plaques.Length && plaques[slot] != null) plaques[slot].sprite = PlaqueSprite(p);
        }

        public void SetMode(Mode mode)
        {
            if (mode == Mode.Ingredient)
            {
                SetPlaque(0, Plaque.Slice);
                SetPlaque(1, Plaque.Heat);
                SetPlaque(2, Plaque.ToPot);
            }
            else
            {
                SetPlaque(0, Plaque.Pour);
                SetPlaque(1, Plaque.Mix);
                SetPlaque(2, Plaque.Serve);
            }
            SetPouring(false);
        }

        /// <summary>While pouring, the first button turns red and its plaque says STOP.</summary>
        public void SetPouring(bool on)
        {
            if (on) SetPlaque(0, Plaque.Stop);
            else if (plaques.Length > 0 && plaques[0] != null && plaques[0].sprite == PlaqueSprite(Plaque.Stop)) SetPlaque(0, Plaque.Pour);
            if (buttons.Length > 0 && buttons[0] != null)
            {
                var img = buttons[0].targetGraphic as Image;
                if (img != null) img.sprite = on ? buttonActive : buttonNormal;
            }
        }

        public void SetInteractable(bool on, int onlyButton = -1)
        {
            for (int i = 0; i < buttons.Length; i++)
                if (buttons[i] != null) buttons[i].interactable = on || i == onlyButton;
        }

        public void ShowIngredient(IngredientData d, Prep p)
        {
            if (d == null) return;
            if (panelIcon != null)
            {
                panelIcon.sprite = d.SpriteFor(p);
                panelIcon.enabled = panelIcon.sprite != null;
            }
            if (panelName != null) panelName.text = d.displayName;
            SetBadges(p == Prep.Raw ? badgeRaw : null,
                      p == Prep.Sliced || p == Prep.SlicedHeated ? badgeSliced : null,
                      p == Prep.Heated || p == Prep.SlicedHeated ? badgeHeated : null);
        }

        public void ShowCauldron(CauldronController c)
        {
            if (panelIcon != null)
            {
                panelIcon.sprite = cauldronIcon;
                panelIcon.enabled = cauldronIcon != null;
            }
            if (panelName != null)
            {
                if (c == null || !c.HasLiquid) panelName.text = "Cauldron (empty)";
                else panelName.text = "Cauldron: " + c.Contents.Count + " in, stirred " + c.Mixes + "x";
            }
            SetBadges(c != null && c.Contents.Count > 0 ? badgeInPot : null);
        }

        /// <summary>Shows the given badges left to right; null entries are skipped.</summary>
        public void SetBadges(params Sprite[] sprites)
        {
            var list = new List<Sprite>();
            foreach (var s in sprites) if (s != null) list.Add(s);
            for (int i = 0; i < badges.Length; i++)
            {
                if (badges[i] == null) continue;
                badges[i].sprite = i < list.Count ? list[i] : null;
                badges[i].enabled = i < list.Count;
            }
        }

        public void Say(string message, float seconds = 3.5f)
        {
            if (bubble == null || bubbleText == null)
            {
                Debug.Log("[Potion Lab] " + message);
                return;
            }
            if (sayRoutine != null) StopCoroutine(sayRoutine);
            sayRoutine = StartCoroutine(SayRoutine(message, seconds));
        }

        IEnumerator SayRoutine(string message, float seconds)
        {
            bubble.SetActive(true);
            bubbleText.text = message;
            yield return new WaitForSeconds(seconds);
            bubble.SetActive(false);
            sayRoutine = null;
        }

        /// <summary>Adds a line to the journal in the recipe book (newest on top).</summary>
        public void Log(string line)
        {
            journal.Insert(0, line);
            if (journal.Count > 14) journal.RemoveAt(journal.Count - 1);
            if (bookJournal != null) bookJournal.text = string.Join("\n", journal.ToArray());
        }

        public void SetRecipe(RecipeData r, int index, int total)
        {
            if (goalText != null) goalText.text = r == null ? "" : "Recipe " + (index + 1) + "/" + total + ":  " + r.potionName;
            if (bookTitle != null) bookTitle.text = r == null ? "" : r.potionName;
            if (bookRecipe != null) bookRecipe.text = r == null ? "" : r.bookText;
        }

        public void ToggleBook()
        {
            if (book == null) return;
            if (book.activeSelf) CloseBook();
            else if (openRoutine == null) openRoutine = StartCoroutine(OpenBook());
        }

        /// <summary>
        /// Opens on the next frame, so the tap that opened the book is already handled by the UI and can't also
        /// "click" the dark backdrop (which would close the book again straight away).
        /// </summary>
        IEnumerator OpenBook()
        {
            yield return null;
            book.SetActive(true);
            openRoutine = null;
        }

        public void CloseBook()
        {
            if (book != null) book.SetActive(false);
        }

        /// <summary>The dark area around the open book.</summary>
        public void OnBackdropClicked()
        {
            CloseBook();
        }
    }
}
