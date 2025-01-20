using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using TMPro;

public class Tooltips : MonoBehaviour
{
    public static Tooltips Instance { get; private set; }
    public GameObject heroHealth;
    public TMP_Text heroHealthText;

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


}
