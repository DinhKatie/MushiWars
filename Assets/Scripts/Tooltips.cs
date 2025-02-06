using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    public void ShowHeroHealthTooltip(BaseHero hero)
    {
        heroHealthText.text = "Health: " + hero.Health.ToString();

        Vector3 mousePosition = Input.mousePosition;
        heroHealth.transform.position = mousePosition + new Vector3(10, 30, 0);

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
            GameObject square = Instantiate(unitRevivalPrefab, revivalPanel);
            Transform unitImage = square.GetComponent<RectTransform>();
            TextMeshProUGUI numberText = square.GetComponentInChildren<TextMeshProUGUI>();

            GameObject unitPrefab;
            if (unitsToRevive[i] is SwordUnit)
                unitPrefab = swordUnitImage;
            else if (unitsToRevive[i] is GunUnit)
                unitPrefab = gunUnitImage;
            else
                unitPrefab = baseUnitImage;

            GameObject unitInstance = Instantiate(unitPrefab, unitImage);
            unitInstance.transform.localPosition = new Vector3(-7.5f, -4.8f,0);

            numberText.text = i.ToString();
            numberText.transform.SetAsLastSibling();
        }
    }
    public void HideRevivalTooltip() => revivalPanel.gameObject.SetActive(false);

    public void ShowPlayerTurn(string nickname)
    {
        playerTurnText.text = nickname + " Turn!";
    }
}
