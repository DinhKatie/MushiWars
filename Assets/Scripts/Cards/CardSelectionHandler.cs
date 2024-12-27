using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSelectionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private float _verticalMoveAmount = 30f;
    [SerializeField] private float _moveTime = 0.1f;
    [SerializeField] private float _scaleAmount = 1.1f;

    private Vector3 _startPos;
    private Vector3 _startScale;
    public int _originalIndex; //Sorting order

    private Coroutine _currentCoroutine;

    private void Start()
    {
        _startPos = transform.position;
        _startScale = transform.localScale;
        _originalIndex = transform.GetSiblingIndex();
    }

    private IEnumerator MoveCard(bool startingAnim)
    {
        if (GetComponent<CardMovement>()._isBeingDragged)
        {
            yield break;
        }

        Vector3 endPos = startingAnim ? _startPos + new Vector3(0f, _verticalMoveAmount, 0f) : _startPos;
        Vector3 endScale = startingAnim ? _startScale * _scaleAmount : _startScale;

        float elapsedTime = 0f;
        while (elapsedTime < _moveTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _moveTime;

            transform.position = Vector3.Lerp(transform.position, endPos, t);
            transform.localScale = Vector3.Lerp(transform.localScale, endScale, t);

            yield return null;
        }
    }

    public void ResetScale()
    {
        transform.localScale = _startScale;
    }

    public void ResetCard()
    {
        _startPos = transform.position;
        _startScale = transform.localScale;
        _originalIndex = transform.GetSiblingIndex();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        eventData.selectedObject = gameObject;

        // Bring the card to the front
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        eventData.selectedObject = null;

        // Restore the card's original sibling index
        transform.SetSiblingIndex(_originalIndex);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(MoveCard(true));
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);
        _currentCoroutine = StartCoroutine(MoveCard(false));
    }
}
