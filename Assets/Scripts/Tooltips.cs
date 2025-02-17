using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tooltips : MonoBehaviour
{
    public static Tooltips Instance { get; private set; }
    [Header("Mushi Sprites")]
    public GameObject baseUnitImage;
    public GameObject gunUnitImage;
    public GameObject swordUnitImage;

    [Header("Hero Health")]
    public GameObject heroHealth;
    public TMP_Text heroHealthText;

    [Header("Unit Revivals")]
    public Transform revivalPanel;
    public GameObject unitRevivalPrefab;

    [Header("Turn Text")]
    public TMP_Text playerTurnText;

    [Header("Floating Text")]
    public GameObject textContainer;
    public GameObject textPrefab;

    //Pooling multiple objects to be reused
    private List<FloatingText> floatingTexts = new List<FloatingText>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        foreach (FloatingText text in floatingTexts)
            text.UpdateFloatingText();
    }

    #region General Tooltips

    public void ShowHeroHealthTooltip(BaseHero hero)
    {
        heroHealthText.text = "Health: " + hero.Health.ToString();
        heroHealth.transform.position = Input.mousePosition + new Vector3(10, 30, 0);
        heroHealth.SetActive(true);
    }

    public void HideHeroHealthTooltip() => heroHealth.SetActive(false);

    public void ShowRevivalTooltip(List<BaseUnit> unitsToRevive)
    {
        revivalPanel.gameObject.SetActive(true);

        //Clear UI
        foreach (Transform child in revivalPanel)
            Destroy(child.gameObject);

        for (int i = 0; i < unitsToRevive.Count; i++)
        {
            CreateRevivalItem(unitsToRevive[i], i);
        }
    }

    public void HideRevivalTooltip() => revivalPanel.gameObject.SetActive(false);

    public void ShowPlayerTurn(string nickname)
    {
        playerTurnText.text = nickname + " Turn!";
    }

    #endregion

    #region Floating Text Methods

    private FloatingText GetFloatingText()
    {
        FloatingText txt = floatingTexts.Find(t => !t.active);

        if (txt == null)
        {
            txt = new FloatingText();
            txt.go = Instantiate(textPrefab);
            txt.go.transform.SetParent(textContainer.transform);
            txt.text = txt.go.GetComponent<TextMeshProUGUI>();

            floatingTexts.Add(txt);
        }

        return txt;
    }

    public void Show(string msg, Color color, Vector3Int position, Vector3 motion, int fontSize = 36, float duration = 1)
    {
        FloatingText floatingText = GetFloatingText();

        floatingText.text.text = msg;
        floatingText.text.fontSize = fontSize;
        floatingText.text.color = color;

        floatingText.go.transform.position = GridManager.Instance.TilemapToCanvas(position) + new Vector3(0, 60, 0);
        floatingText.motion = motion;
        floatingText.duration = duration;

        floatingText.Show();
    }

    #endregion

    #region Helper Methods

    private void CreateRevivalItem(BaseUnit unit, int index)
    {
        GameObject square = Instantiate(unitRevivalPrefab, revivalPanel);
        Transform unitImage = square.GetComponent<RectTransform>();
        TextMeshProUGUI numberText = square.GetComponentInChildren<TextMeshProUGUI>();

        GameObject unitPrefab = GetUnitPrefab(unit);
        GameObject unitInstance = Instantiate(unitPrefab, unitImage);
        unitInstance.transform.localPosition = new Vector3(-7.5f, -4.8f, 0);

        numberText.text = index.ToString();
        numberText.transform.SetAsLastSibling();
    }

    private GameObject GetUnitPrefab(BaseUnit unit)
    {
        if (unit is SwordUnit)
            return swordUnitImage;
        else if (unit is GunUnit)
            return gunUnitImage;
        else
            return baseUnitImage;
    }

    #endregion
}
