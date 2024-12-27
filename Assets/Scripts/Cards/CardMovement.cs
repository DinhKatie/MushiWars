using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardMovement : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{

    public bool _isBeingDragged;
    private Canvas _cardCanvas;
    private RectTransform _rectTransform;
    private Card _card;
    private CardSelectionHandler _cardSelectionHandler;

    private Vector2 _originalPos;

    private readonly string CANVAS_TAG = "CardCanvas";


    private void Start()
    {
        _cardCanvas = GameObject.FindGameObjectWithTag(CANVAS_TAG).GetComponent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
        _card = GetComponent<Card>();
        _cardSelectionHandler = GetComponent<CardSelectionHandler>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isBeingDragged = true;
        _originalPos = _rectTransform.position;
        _cardSelectionHandler.ResetScale();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Convert screen position to world position based on the canvas's scale and positioning
        Vector3 worldPointerPosition;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out worldPointerPosition
        );

        // Apply world position and offset to the card's anchored position (position in relation to its parent)
        _rectTransform.position = worldPointerPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isBeingDragged = false;
        RectTransform handRect = _cardCanvas.transform.Find("Hand").GetComponent<RectTransform>();

        // Check if the card is outside the hand canvas
        if (!RectTransformUtility.RectangleContainsScreenPoint(handRect, Input.mousePosition, eventData.pressEventCamera))
        {
            Deck.Instance.DiscardCard(_card);
            _card.PlayEffect(); //Apply its effect
        }
        else
        {
            _rectTransform.position = _originalPos;
            _rectTransform.transform.SetSiblingIndex(_cardSelectionHandler._originalIndex);
        }
            

        HandManager.Instance.ArrangeCardsInHand();
    }
}
