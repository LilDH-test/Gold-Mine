using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ShopItemUI — Individual card item in the shop grid.
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeAndCostText;
    public TextMeshProUGUI levelTagText;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public TextMeshProUGUI buyButtonText;

    [Header("State Colors")]
    public Color normalButtonColor = new Color(0.06f, 0.07f, 0.09f, 0.92f);
    public Color ownedButtonColor = new Color(0.3f, 0.3f, 0.3f, 0.65f);
    public Color lockedButtonColor = new Color(0.2f, 0.2f, 0.2f, 0.55f);

    private System.Action onBuy;

    public void Setup(CardDatabase.CardDef card, bool owned, bool locked, System.Action buyCallback)
    {
        onBuy = buyCallback;

        if (nameText != null)
            nameText.text = card.name;

        if (typeAndCostText != null)
            typeAndCostText.text = $"{card.type} • ⚡ {card.cost}";

        if (levelTagText != null)
            levelTagText.text = $"Lvl {card.unlockLevel}";

        if (priceText != null)
            priceText.text = card.priceGold == 0 ? "Free" : $"{card.priceGold} Gold";

        // Button state
        if (buyButtonText != null)
        {
            if (owned)
                buyButtonText.text = "Owned";
            else if (locked)
                buyButtonText.text = "Locked";
            else if (card.priceGold == 0)
                buyButtonText.text = "Owned";
            else
                buyButtonText.text = "Buy";
        }

        if (buyButton != null)
        {
            var colors = buyButton.colors;
            if (owned)
                colors.normalColor = ownedButtonColor;
            else if (locked)
                colors.normalColor = lockedButtonColor;
            else
                colors.normalColor = normalButtonColor;
            buyButton.colors = colors;

            buyButton.interactable = !owned && !locked;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuy?.Invoke());
        }
    }
}
