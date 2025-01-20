using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayedCardDisplay : MonoBehaviour
{
    [SerializeField] private Image cardImage;

    public void DisplayCard(Card card)
    {
        cardImage.sprite = card.cardData.Image;
        cardImage.gameObject.SetActive(true);
        StartCoroutine(HideCardAfterDelay(3f));
    }

    private IEnumerator HideCardAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        cardImage.gameObject.SetActive(false);
    }
}
