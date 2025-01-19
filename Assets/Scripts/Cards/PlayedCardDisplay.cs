using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayedCardDisplay : MonoBehaviour
{
    [SerializeField] private Image cardImage;

    public void DisplayCard(Card card)
    {
        Debug.Log($"card.cardData.Image: {card.cardData.Image}");
        cardImage.sprite = card.cardData.Image;
        if (cardImage.sprite == null)
            Debug.Log("Sprite is null.");
        //cardImage.transform.localScale = Vector3.one * 2;

        cardImage.gameObject.SetActive(true);
        StartCoroutine(HideCardAfterDelay(3f));
    }

    private IEnumerator HideCardAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        cardImage.gameObject.SetActive(false);
    }
}
