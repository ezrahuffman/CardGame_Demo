using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardSelectElement : MonoBehaviour
{
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _healthText;
    [SerializeField] TMP_Text _damageText;
    [SerializeField] Image _image;

    CardData _cardData;

    public CardSelectGrid grid;

    // Called from button this component is attached to
    public void OnClick()
    {
        grid.ToggleCard(_cardData.selectionIndex, gameObject);
    }

    public void PopulateData(CardData data)
    {
        _nameText.text = data.cardName;
        _healthText.text = data.health.ToString();
        _damageText.text = data.effectAmnt.ToString();
        _image.sprite = data.cardSprite;

        _cardData = data;
    }
}
