using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// CardUI — Individual card in the player's hand.
/// Displays card name, type, cost, and selection highlight.
/// </summary>
public class CardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI costText;
    public Image background;
    public Image selectionBorder;
    public Button button;

    [Header("Colors")]
    public Color normalColor = new Color(0.06f, 0.07f, 0.09f, 0.92f);
    public Color selectedColor = new Color(1f, 0.82f, 0.4f, 0.85f);
    public Color selectedBorderColor = new Color(1f, 0.82f, 0.4f, 0.85f);

    public event System.Action OnClicked;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(() => OnClicked?.Invoke());
    }

    public void Setup(CardDatabase.CardDef card, int index, bool isSelected)
    {
        if (nameText != null)
            nameText.text = card.name;

        if (typeText != null)
            typeText.text = card.type.ToString();

        if (costText != null)
            costText.text = $"⚡ {card.cost}";

        if (background != null)
            background.color = isSelected ? selectedColor : normalColor;

        if (selectionBorder != null)
        {
            selectionBorder.gameObject.SetActive(isSelected);
            selectionBorder.color = selectedBorderColor;
        }
    }
}
