using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardMovement : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{

    private bool _isBeingDragged;
    private Canvas _cardCanvas;
    private RectTransform _rectTransform;
    private Card _card;

    private readonly string CANVAS_TAG = "CardCanvas";


    private void Start()
    {
        _cardCanvas = GameObject.FindGameObjectWithTag(CANVAS_TAG).GetComponent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
        _card = GetComponent<Card>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isBeingDragged = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition += (eventData.delta / _cardCanvas.scaleFactor);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isBeingDragged = false;
        Deck.Instance.DiscardCard(_card);
    }
}
