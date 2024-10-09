using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    private Card _card;

    [Header("Prefab Elements")]
    [SerializeField] private Image _cardImage;
    [SerializeField] private Image _elementBackground;
    [SerializeField] private Image _rarityBackground;

    [SerializeField] private TextMeshProUGUI _cardName;
    [SerializeField] private TextMeshProUGUI _cardType;
    [SerializeField] private TextMeshProUGUI _cardDescription;

    [Header("Sprite Assets")]
    [SerializeField] private Sprite _commonRarityBackground;
    [SerializeField] private Sprite _rareRarityBackground;
    [SerializeField] private Sprite _epicRarityBackground;
    [SerializeField] private Sprite _legendaryRarityBackground;

    //Add more when we learn what card types there are lmao
    private readonly string EFFECTTYPE_CURSE = "Curse";


    private void Awake()
    {
        _card = GetComponent<Card>();
        SetCardUI();
    }

    private void OnValidate()
    {
        Awake();
    }

    public void SetCardUI()
    {
        if (_card != null && _card.cardData != null)
        {
            SetCardTexts();
            SetRarityBackground();
            SetCardImage();
        }
    }

    private void SetCardTexts()
    {

        _cardName.text = _card.cardData.CardName;
        _cardDescription.text = _card.cardData.CardDescription;
    }

    private void SetRarityBackground()
    {
        switch (_card.cardData.Rarity)
        {
            case ScriptableCard.CardRarity.Common:
                _rarityBackground.sprite = _commonRarityBackground;
                break;
            case ScriptableCard.CardRarity.Rare:
                _rarityBackground.sprite = _rareRarityBackground;
                break;
            case ScriptableCard.CardRarity.Epic:
                _rarityBackground.sprite = _epicRarityBackground;
                break;
            case ScriptableCard.CardRarity.Legendary:
                _rarityBackground.sprite = _legendaryRarityBackground;
                break;
        }
    }

    private void SetCardImage()
    {
        _cardImage.sprite = _card.cardData.Image;
    }
}
